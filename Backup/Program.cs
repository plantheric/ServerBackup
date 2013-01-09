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
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));
        static void Main(string[] args)
        {
            XmlConfigurator.Configure();
            BackupConfig config = BackupConfig.GetConfig();

            log.Info("Start");

            var files = new List<UploadItem>();

            foreach (DatabaseBackup database in config.Databases)
            {
                var backup = new BackupDatabase
                {
                    ServerName = database.Server,
                    DatabaseName = database.Name
                };
                files.Add(new UploadItem(backup.MakeBackupFile(), database.KeyPrefix, database.Lifetime));
            }

            foreach (DirectoryBackup directory in config.Directories)
            {
                var archiveFiles = new ArchiveFiles
                {
                    LocalDirectory = directory.Path,
                    AWSBucket = config.Bucket,
                    AWSAccessKey = config.AccessKey,
                    AWSSecretKey = config.SecretKey,
                    FullKeyPrefix = config.FullKeyPrefix(directory.KeyPrefix)
                };
                files.Add(new UploadItem(archiveFiles.GetFilesToUpload(), directory.KeyPrefix, directory.Lifetime));
            }

            var upload = new Upload
            {
                AWSBucket = config.Bucket,
                AWSAccessKey = config.AccessKey,
                AWSSecretKey = config.SecretKey,
                KeyPrefix = config.KeyPrefix
            };

            upload.TransferFiles(files);
            log.Info("End");
        }
    }
}

