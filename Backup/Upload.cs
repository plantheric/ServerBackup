using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3.Transfer;

namespace nopBackup
{
    class UploadItem
    {
        public string FilePath;
        public string KeyPrefix;
        public TimeSpan Lifetime;
    }

    class Upload
    {
        public bool TransferFiles(List<UploadItem> files)
        {
            var s3Client = AWSClientFactory.CreateAmazonS3Client(AccessKey, SecretKey);
            var tranferUtility = new TransferUtility(s3Client);

            foreach (UploadItem file in files)
            {
                string fileKey = string.Join("/", new[] { KeyPrefix, file.KeyPrefix, Path.GetFileName(file.FilePath) });
                var request = new TransferUtilityUploadRequest().WithBucketName(Bucket).WithKey(fileKey).WithFilePath(file.FilePath);

                tranferUtility.Upload(request);
            }

            return true;
        }

        public string AccessKey;
        public string SecretKey;
        public string KeyPrefix;
        public string Bucket;
    }
}
