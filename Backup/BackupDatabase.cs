using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using System.IO.Compression;

namespace nopBackup
{
    class BackupDatabase
    {
        public BackupDatabase()
        {
        }

        public UploadItem MakeBackupFile()
        {
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

            backup.SqlBackup(server);
            string zipFilePath = Utilities.MakeZipFile(FilePath);
            File.Delete(FilePath);
            FilePath = zipFilePath;

            return new UploadItem { FilePath = FilePath, KeyPrefix = "Database", Lifetime = TimeSpan.MaxValue };
        }


        static void backup_Complete(object sender, ServerMessageEventArgs e)
        {
            Console.WriteLine("Done");
        }

        static void backup_PercentComplete(object sender, PercentCompleteEventArgs e)
        {
            Console.WriteLine("Percent Complete {0}%", e.Percent);
        }

        public string DatabaseName { get; set; }
        public string ServerName { get; set; }

        private string FilePath { get; set; }
    }
}
