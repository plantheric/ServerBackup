using System;
using System.Collections.Generic;
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

        public string MakeBackupFile()
        {
            FilePath = @"c:\Users\Public\backup.bak";
            var server = new Server(ServerName);

            var backup = new Backup ();
            backup.Action = BackupActionType.Database;
            backup.Database = DatabaseName;
            backup.Devices.AddDevice(FilePath, DeviceType.File);
            backup.BackupSetName = DatabaseName + "-Backup";
            backup.BackupSetDescription = "Backup";
            backup.Initialize = true;
            backup.PercentComplete += backup_PercentComplete;
            backup.Complete += backup_Complete;
            backup.SqlBackup(server);

            return FilePath;
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
