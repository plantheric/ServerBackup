using System;
using System.Collections.Generic;
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
            var server = new Server(@".\SQLExpress");

            var backup = new Backup ();
            backup.Action = BackupActionType.Database;
            backup.Database = "nopcommerce497";
            backup.Devices.AddDevice(@"c:\Users\Public\backup.bak", DeviceType.File);
            backup.BackupSetName = "Backup";
            backup.BackupSetDescription = "Backup";
            backup.Initialize = true;
            backup.PercentComplete += backup_PercentComplete;
            backup.Complete += backup_Complete;
            backup.SqlBackup(server);
        }

        static void backup_Complete(object sender, ServerMessageEventArgs e)
        {
            Console.WriteLine("Done");
        }

        static void backup_PercentComplete(object sender, PercentCompleteEventArgs e)
        {
            Console.WriteLine("Percent Complete {0}%", e.Percent);
        }
    }
}

