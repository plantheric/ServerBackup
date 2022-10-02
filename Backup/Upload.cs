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
        public UploadSet(List<UploadItem> items, string keyPrefix, int lifetime, bool deleteAfterUpload)
        {
            Items = items;
            KeyPrefix = keyPrefix;
            Lifetime = lifetime;
            DeleteAfterUpload = deleteAfterUpload;
        }
        public List<UploadItem> Items;
        public string KeyPrefix;
        public int Lifetime;
        public bool DeleteAfterUpload;
    }

    class Upload
    {
        public bool TransferFiles(List<UploadSet> uploads)
        {
            try
            {
                log.Info("Start TransferFiles");

                AmazonS3Config config = new AmazonS3Config
                {
                    RegionEndpoint = RegionEndpoint.GetBySystemName(Region),
                    SignatureMethod = Amazon.Runtime.SigningAlgorithm.HmacSHA256,
                    SignatureVersion = "4"
                };
                S3Client = new AmazonS3Client(AWSAccessKey, AWSSecretKey, config);

                GetLifeCycleConfiguration();
                var tranferUtility = new TransferUtility(S3Client);

                foreach (var upload in uploads)
                {
                    string fullPrefix = KeyPrefix + @"/" + upload.KeyPrefix;
                    foreach (var file in upload.Items)
                    {
                        string fileKey = fullPrefix + @"/" + Path.GetFileName(file.FilePath);
                        var request = new TransferUtilityUploadRequest { BucketName = AWSBucket, Key = fileKey };
                        foreach (string key in file.Metadata)
                        {
                            request.Metadata.Add(key, file.Metadata[key]);
                        }
                        request.InputStream = new FileStream(file.FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        tranferUtility.Upload(request);

                        log.InfoFormat("Uploaded {0}", fileKey);
                        if (upload.DeleteAfterUpload)
                            File.Delete(file.FilePath);
                    }

                    LifecycleConfiguration.Rules.RemoveAll(rule => rule.Prefix == fullPrefix);
                    if (upload.Lifetime != int.MaxValue)
                    {
                        LifecycleConfiguration.Rules.Add(new LifecycleRule
                        {
                            Id = string.Format("Auto Purge {0} after {1} days", fullPrefix, upload.Lifetime),
                            Prefix = fullPrefix,
                            Expiration = new LifecycleRuleExpiration { Days = upload.Lifetime },
                            Status = LifecycleRuleStatus.Enabled
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
        public string Region;
        public string AWSBucket;
        public string KeyPrefix;

        private AmazonS3Client S3Client;
        private LifecycleConfiguration LifecycleConfiguration;

        private static readonly ILog log = LogManager.GetLogger(typeof(Upload));
    }
}
