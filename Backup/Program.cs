using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;

namespace nopBackup
{
    class Program
    {
        static void Main(string[] args)
        {
            BackupConfig config = BackupConfig.GetConfig();

            var files = new List<UploadItem>();
            var backup = new BackupDatabase
            {
                ServerName = config.Database.Server,
                DatabaseName = config.Database.Name
            };
            files.Add(new UploadItem
                        {
                            FilePaths = backup.MakeBackupFile(),
                            KeyPrefix = config.Database.KeyPrefix,
                            Lifetime = config.Database.Lifetime
                        });

            var archiveFiles = new ArchiveFiles
            {
                LocalDirectory = config.Directory.Path,
                AWSBucket = config.Bucket,
                AWSAccessKey = config.AccessKey,
                AWSSecretKey = config.SecretKey,
                FullKeyPrefix = config.KeyPrefix + @"/" + config.Directory.KeyPrefix + @"/"
            };
            files.Add(new UploadItem
                        {
                            FilePaths = archiveFiles.GetFilesToUpload(),
                            KeyPrefix = config.Directory.KeyPrefix,
                            Lifetime = config.Directory.Lifetime
                        });

            var upload = new Upload
            {
                AWSBucket = config.Bucket,
                AWSAccessKey = config.AccessKey,
                AWSSecretKey = config.SecretKey,
                KeyPrefix = config.KeyPrefix
            };

            upload.TransferFiles(files);
        }
    }
}

