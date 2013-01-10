using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using log4net;

namespace nopBackup
{
    class UploadItem
    {
        public string FilePath;
        public NameValueCollection Metadata;
    }

    class UploadSet
    {
        public UploadSet(List<UploadItem> items, string keyPrefix, int lifetime)
        {
            Items = items;
            KeyPrefix = keyPrefix;
            Lifetime = lifetime;
        }
        public List<UploadItem> Items;
        public string KeyPrefix;
        public int Lifetime;
    }

    class Upload
    {
        public bool TransferFiles(List<UploadSet> uploads)
        {
            try
            {
                log.Info("Start TransferFiles");

                S3Client = AWSClientFactory.CreateAmazonS3Client(AWSAccessKey, AWSSecretKey);
                GetLifeCycleConfiguration();
                var tranferUtility = new TransferUtility(S3Client);

                foreach (var upload in uploads)
                {
                    string fullPrefix = KeyPrefix + @"/" + upload.KeyPrefix;
                    foreach (var file in upload.Items)
                    {
                        string fileKey = fullPrefix + @"/" + Path.GetFileName(file.FilePath);
                        var request = new TransferUtilityUploadRequest { FilePath = file.FilePath, BucketName = AWSBucket, Key = fileKey };
                        request.WithMetadata(file.Metadata);
                        tranferUtility.Upload(request);

                        log.InfoFormat("Uploaded {0}", fileKey);
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
            }
            catch (Exception e)
            {
                log.Error("TransferFiles error", e);
            }
            log.Info("End TransferFiles");
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

        private static readonly ILog log = LogManager.GetLogger(typeof(Upload));
    }
}
