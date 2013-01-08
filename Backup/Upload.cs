using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

namespace nopBackup
{
    class UploadItem
    {
        public List<string> FilePaths;
        public string KeyPrefix;
        public int Lifetime;
    }

    class Upload
    {
        public bool TransferFiles(List<UploadItem> uploads)
        {
            S3Client = AWSClientFactory.CreateAmazonS3Client(AWSAccessKey, AWSSecretKey);
            GetLifeCycleConfiguration();
            var tranferUtility = new TransferUtility(S3Client);

            foreach (var upload in uploads)
            {
                string fullPrefix = KeyPrefix + @"/" + upload.KeyPrefix;
                foreach (string file in upload.FilePaths)
                {
                    string fileKey = fullPrefix + @"/" + Path.GetFileName(file);
                    tranferUtility.Upload(file, AWSBucket, fileKey);
                }

                LifecycleConfiguration.Rules.RemoveAll(rule => rule.Prefix == fullPrefix);
                if (upload.Lifetime != int.MaxValue)
                {
                    LifecycleConfiguration.Rules.Add(new LifecycleRule
                    {
                        Id = string.Format("Auto Purge {0} after {1} days", fullPrefix, upload.Lifetime),
                        Prefix = fullPrefix,
                        Expiration = new LifecycleRuleExpiration { Days = upload.Lifetime }
                    });
                }
            }
            PutLifeCycleConfiguration();
            return true;
        }


        private void GetLifeCycleConfiguration()
        {
            var request = new GetLifecycleConfigurationRequest { BucketName = AWSBucket };
            var response = S3Client.GetLifecycleConfiguration(request);
            LifecycleConfiguration = response.Configuration;

            if (LifecycleConfiguration == null)
                LifecycleConfiguration = new LifecycleConfiguration();
        }

        private void PutLifeCycleConfiguration()
        {
            var request = new PutLifecycleConfigurationRequest { BucketName = AWSBucket, Configuration = LifecycleConfiguration };
            var response = S3Client.PutLifecycleConfiguration(request);
        }

        public string AWSAccessKey;
        public string AWSSecretKey;
        public string AWSBucket;
        public string KeyPrefix;

        private AmazonS3 S3Client;
        private LifecycleConfiguration LifecycleConfiguration;
    }
}
