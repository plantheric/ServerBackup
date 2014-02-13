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
            S3Client = AWSClientFactory.CreateAmazonS3Client(AWSAccessKey, AWSSecretKey);
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
                metadata.Add(response.Metadata);
            }

            return metadata;
        }

        private static string AWSAccessKey;
        private static string AWSSecretKey;
        private static string AWSBucket;

        private AmazonS3 S3Client;

        private static readonly ILog log = LogManager.GetLogger(typeof(S3Interface));
    }
}
