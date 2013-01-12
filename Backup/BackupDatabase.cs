using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using System.IO.Compression;
using Amazon.S3.Model;
using log4net;

namespace nopBackup
{
    class BackupDatabase
    {
        public BackupDatabase()
        {
        }

        public List<UploadItem> MakeBackupFile()
        {
            var uploads = new List<UploadItem>();

            try
            {
                log.InfoFormat("Start backup of {0}", DatabaseName);

                var server = new Server(ServerName);
                string backupName = string.Format("{0}-Backup-{1}", DatabaseName, DateTime.Now.ToString("ddMMyyy_HHmm"));

                FilePath = Path.Combine(Utilities.SystemTempFolder, backupName + ".bak");

                var backup = new Backup();
                backup.Action = BackupActionType.Database;
                backup.Database = DatabaseName;
                backup.Devices.AddDevice(FilePath, DeviceType.File);
                backup.BackupSetName = backupName;
                backup.BackupSetDescription = string.Format("Backup for database {0} from server {1}", DatabaseName, server.Name); ;
                backup.Initialize = true;
                backup.PercentComplete += backup_PercentComplete;
                backup.Complete += backup_Complete;
                backup.Incremental = NeedIncrementalBackup();

                backup.SqlBackup(server);
                string zipFilePath = Utilities.MakeZipFile(FilePath);
                File.Delete(FilePath);
                FilePath = zipFilePath;

                var metadata = new NameValueCollection();
                metadata.Add("backuptype", backup.Incremental ? "Differential" : "Full");

                uploads.Add(new UploadItem { FilePath = FilePath, Metadata = metadata });
            }
            catch (Exception e)
            {
                log.Error("MakeBackupFile error", e);
            }

            log.Info("End backup");

            return uploads;
        }

        //  Checks the metadata of the last $FullBackupFrequency backsup, if none were a 
        //  full backup then we need to do one.
        private bool NeedIncrementalBackup()
        {
            var s3Interface = new S3Interface();
            List<S3Object> s3Objects = s3Interface.ObjectsFromKey(FullKeyPrefix);

            int allowedDifferentials = Math.Max(FullBackupFrequency - 1, 0);
            s3Objects.Sort((a, b) => DateTime.Parse(a.LastModified).CompareTo(DateTime.Parse(b.LastModified)));
            if (s3Objects.Count > allowedDifferentials)
                s3Objects = s3Objects.GetRange(s3Objects.Count - allowedDifferentials, allowedDifferentials);

            List<NameValueCollection> metadata = s3Interface.GetObjectMetadata(s3Objects);

            bool recentFullBackup = metadata.Exists(md => md.Get("x-amz-meta-backuptype") == "Full");
            return recentFullBackup;
        }

        static void backup_Complete(object sender, ServerMessageEventArgs e)
        {
        }

        static void backup_PercentComplete(object sender, PercentCompleteEventArgs e)
        {
        }

        public string DatabaseName;
        public string ServerName;
        public string FullKeyPrefix;
        public int FullBackupFrequency = 1;

        private string FilePath;
        private static readonly ILog log = LogManager.GetLogger(typeof(BackupDatabase));
    }
}
