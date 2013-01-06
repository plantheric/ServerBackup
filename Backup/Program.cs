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
            var backup = new BackupDatabase { ServerName = @".\SQLExpress", DatabaseName = "nopcommerce497" };
            backup.MakeBackupFile();
        }
    }
}

