using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using log4net;


namespace nopBackup
{
    class S3Interface
    {
        public static void Setup(string accessKey, string secretKey, string bucket)
        {
            AWSAccessKey = accessKey;
            AWSSecretKey = secretKey;
            AWSBucket = bucket;
        }

        public S3Interface()
        {
            AmazonS3Config config = new AmazonS3Config
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName("eu-west-2"),
                SignatureMethod = Amazon.Runtime.SigningAlgorithm.HmacSHA256,
                SignatureVersion = "4"
            };
            S3Client = new AmazonS3Client(AWSAccessKey, AWSSecretKey, config);
        }

        public List<S3Object> ObjectsFromKey(string key)
        {
            List<S3Object> s3Files = new List<S3Object>();

            try
            {
                var request = new ListObjectsRequest { BucketName = AWSBucket, Prefix = key, Delimiter = @"/" };

                do
                {
                    var response = S3Client.ListObjects(request);
                    s3Files.AddRange(response.S3Objects);

                    if (response.IsTruncated)
                        request.Marker = response.NextMarker;
                    else
                        request = null;

                } while (request != null);
            }
            catch (Exception e)
            {
                log.Error("ObjectsFromKey", e);
            }
            return s3Files;
        }

        public List<NameValueCollection> GetObjectMetadata(List<S3Object> objects)
        {
            var metadata = new List<NameValueCollection>();

            foreach (var s3Object in objects)
            {
                var request = new GetObjectMetadataRequest { BucketName = AWSBucket, Key = s3Object.Key };
                var response = S3Client.GetObjectMetadata(request);
                var m = new NameValueCollection();
                foreach (string key in response.Metadata.Keys)
                {
                    m.Add(key, response.Metadata[key]);
                }
                metadata.Add(m);
            }

            return metadata;
        }

        private static string AWSAccessKey;
        private static string AWSSecretKey;
        private static string AWSBucket;

        private AmazonS3Client S3Client;

        private static readonly ILog log = LogManager.GetLogger(typeof(S3Interface));
    }
}
