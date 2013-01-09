using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using System.IO.Compression;
using log4net;

namespace nopBackup
{
    class BackupDatabase
    {
        public BackupDatabase()
        {
        }

        public List<string> MakeBackupFile()
        {
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
                backup.Incremental = true;

                backup.SqlBackup(server);
                string zipFilePath = Utilities.MakeZipFile(FilePath);
                File.Delete(FilePath);
                FilePath = zipFilePath;
            }
            catch (Exception e)
            {
                log.Error("MakeBackupFile error", e);
            }

            log.Info("End backup");

            return new List<string> { FilePath };
        }


        static void backup_Complete(object sender, ServerMessageEventArgs e)
        {
        }

        static void backup_PercentComplete(object sender, PercentCompleteEventArgs e)
        {
        }

        public string DatabaseName { get; set; }
        public string ServerName { get; set; }

        private string FilePath { get; set; }
        private static readonly ILog log = LogManager.GetLogger(typeof(BackupDatabase));
    }
}
