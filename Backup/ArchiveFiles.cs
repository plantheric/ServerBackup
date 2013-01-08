using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3.Model;

namespace nopBackup
{
    class ArchiveFiles
    {
        public List<string> GetFilesToUpload()
        {
            List<string> localFiles = new List<string>(Directory.GetFiles(LocalDirectory));
            localFiles = localFiles.ConvertAll(f => Path.GetFileName(f));

            var request = new ListObjectsRequest { BucketName = AWSBucket, Prefix = FullKeyPrefix, Delimiter = @"/" };

            List<string> s3Files;

            using (var s3Client = AWSClientFactory.CreateAmazonS3Client(AWSAccessKey, AWSSecretKey))
            {
                using (var response = s3Client.ListObjects(request))
                {
                    s3Files = response.S3Objects.ConvertAll(f => f.Key.Substring(FullKeyPrefix.Length));
                }
            }

            localFiles = new List<string>(localFiles.Except(s3Files));
            localFiles = localFiles.ConvertAll(f => Path.Combine(LocalDirectory, f));

            return localFiles;
        }

        public string LocalDirectory;
        public string AWSAccessKey;
        public string AWSSecretKey;
        public string AWSBucket;
        public string FullKeyPrefix;
    }
}
