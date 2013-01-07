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
        public List<UploadItem> GetFilesToUpload()
        {
            string fullPrefix = BaseKeyPrefix + @"/" + SubKeyPrefix + @"/";
            var filesToUpload = new List<UploadItem>();

            List<string> localFiles = new List<string>(Directory.GetFiles(LocalDirectory));
            localFiles = localFiles.ConvertAll(f => Path.GetFileName(f));

            var request = new ListObjectsRequest
            {
                BucketName = AWSBucket,
                Prefix = fullPrefix,
                Delimiter = @"/"
            };

            List<string> s3Files = new List<string>();

            using (var s3Client = AWSClientFactory.CreateAmazonS3Client(AWSAccessKey, AWSSecretKey))
            {
                using (var response = s3Client.ListObjects(request))
                {
                    foreach (S3Object file in response.S3Objects)
                    {
                        s3Files.Add(file.Key.Substring(fullPrefix.Length));
                    }
                }
            }

            localFiles = new List<string>(localFiles.Except(s3Files));
            localFiles = localFiles.ConvertAll(f => Path.Combine(LocalDirectory, f));

            foreach (var file in localFiles)
            {
                filesToUpload.Add(new UploadItem { FilePath = file, KeyPrefix = SubKeyPrefix, Lifetime = TimeSpan.MaxValue });
            }

            return filesToUpload;
        }

        public string LocalDirectory;
        public string AWSAccessKey;
        public string AWSSecretKey;
        public string AWSBucket;
        public string BaseKeyPrefix;
        public string SubKeyPrefix;
    }
}
