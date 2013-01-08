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
        public List<string> FilePaths;
        public string KeyPrefix;
        public TimeSpan Lifetime;
    }

    class Upload
    {
        public bool TransferFiles(List<UploadItem> uploads)
        {
            var s3Client = AWSClientFactory.CreateAmazonS3Client(AWSAccessKey, AWSSecretKey);
            var tranferUtility = new TransferUtility(s3Client);

            foreach (var upload in uploads)
            {
                foreach (string file in upload.FilePaths)
                {
                    string fileKey = string.Join("/", new[] { KeyPrefix, upload.KeyPrefix, Path.GetFileName(file) });
                    var request = new TransferUtilityUploadRequest { BucketName = AWSBucket, Key = fileKey, FilePath = file };

                    tranferUtility.Upload(request);
                }
            }
            return true;
        }

        public string AWSAccessKey;
        public string AWSSecretKey;
        public string AWSBucket;
        public string KeyPrefix;
    }
}
