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
            var files = new List<UploadItem>();
            var backup = new BackupDatabase
            {
                ServerName = ConfigurationManager.AppSettings["SQLServerName"],
                DatabaseName = ConfigurationManager.AppSettings["SQLDatabaseName"]
            };
            files.Add(backup.MakeBackupFile());

            var archiveFiles = new ArchiveFiles
            {
                LocalDirectory = ConfigurationManager.AppSettings["ImagesDirectory"],
                AWSBucket = ConfigurationManager.AppSettings["AWSBucket"],
                AWSAccessKey = ConfigurationManager.AppSettings["AWSAccessKey"],
                AWSSecretKey = ConfigurationManager.AppSettings["AWSSecretKey"],
                BaseKeyPrefix = ConfigurationManager.AppSettings["AWSKeyPrefix"],
                SubKeyPrefix = @"Images"
            };
            files.AddRange(archiveFiles.GetFilesToUpload());

            var upload = new Upload
            {
                Bucket = ConfigurationManager.AppSettings["AWSBucket"],
                AccessKey = ConfigurationManager.AppSettings["AWSAccessKey"],
                SecretKey = ConfigurationManager.AppSettings["AWSSecretKey"],
                KeyPrefix = ConfigurationManager.AppSettings["AWSKeyPrefix"]
            };

            upload.TransferFiles(files);
        }
    }
}

