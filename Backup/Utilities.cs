using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nopBackup
{
    class Utilities
    {
        public static string MakeZipFile(string filePath)
        {
            string zipFilePath = filePath + ".zip";

            using (ZipArchive archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
            {
                archive.CreateEntryFromFile(filePath, Path.GetFileName(filePath));
            }

            return zipFilePath;
        }

        public static string SystemTempFolder { get { return Environment.GetEnvironmentVariable("TEMP", EnvironmentVariableTarget.Machine); } }
    }
}
