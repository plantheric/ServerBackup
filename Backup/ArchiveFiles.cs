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
        public UploadItem GetFilesToUpload()
        {
            string fullPrefix = string.Format("{0}/{1}/", BaseKeyPrefix, SubKeyPrefix);

            List<string> localFiles = new List<string>(Directory.GetFiles(LocalDirectory));
            localFiles = localFiles.ConvertAll(f => Path.GetFileName(f));

            var request = new ListObjectsRequest { BucketName = AWSBucket, Prefix = fullPrefix, Delimiter = @"/" };

            List<string> s3Files;

            using (var s3Client = AWSClientFactory.CreateAmazonS3Client(AWSAccessKey, AWSSecretKey))
            {
                using (var response = s3Client.ListObjects(request))
                {
                    s3Files = response.S3Objects.ConvertAll(f => f.Key.Substring(fullPrefix.Length));
                }
            }

            localFiles = new List<string>(localFiles.Except(s3Files));
            localFiles = localFiles.ConvertAll(f => Path.Combine(LocalDirectory, f));

            return new UploadItem { FilePaths = localFiles, KeyPrefix = SubKeyPrefix, Lifetime = TimeSpan.MaxValue };
        }

        public string LocalDirectory;
        public string AWSAccessKey;
        public string AWSSecretKey;
        public string AWSBucket;
        public string BaseKeyPrefix;
        public string SubKeyPrefix;
    }
}
