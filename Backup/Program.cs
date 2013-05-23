using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using log4net;
using log4net.Config;

namespace nopBackup
{
    class Program
    {
        static void Main(string[] args)
        {
            XmlConfigurator.Configure();
            log.Info("Start");

            BackupConfig config = BackupConfig.GetConfig();
            S3Interface.Setup(config.AccessKey, config.SecretKey, config.Bucket);

            var files = new List<UploadSet>();

            foreach (DatabaseBackup database in config.Databases)
            {
                var backup = new BackupDatabase
                {
                    ServerName = database.Server,
                    DatabaseName = database.Name,
                    FullKeyPrefix = config.FullKeyPrefix(database.KeyPrefix),
                    FullBackupFrequency = database.FullBackupFrequency
                };
                files.Add(new UploadSet(backup.MakeBackupFile(), database.KeyPrefix, database.Lifetime, true));
            }

            foreach (DirectoryBackup directory in config.Directories)
            {
                var archiveFiles = new ArchiveFiles
                {
                    LocalDirectory = directory.Path,
                    FullKeyPrefix = config.FullKeyPrefix(directory.KeyPrefix),
                    BackupLifetime = directory.Lifetime
                };
                files.Add(new UploadSet(archiveFiles.GetFilesToUpload(), directory.KeyPrefix, directory.Lifetime, false));
            }

            var upload = new Upload
            {
                AWSBucket = config.Bucket,
                AWSAccessKey = config.AccessKey,
                AWSSecretKey = config.SecretKey,
                KeyPrefix = config.KeyPrefix
            };

            if (config.NoUpload == false)
                upload.TransferFiles(files);
            else
                log.Info("UPLOAD DISABLED");

            log.Info("End");
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(Program));
    }
}

