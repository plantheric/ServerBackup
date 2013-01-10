using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3.Model;
using log4net;

namespace nopBackup
{
    class ArchiveFiles
    {
        public List<UploadItem> GetFilesToUpload()
        {
            var uploads = new List<UploadItem>();
            try
            {
                log.InfoFormat("Start GetFilesToUpload for {0}", LocalDirectory);

                var s3Objects = new S3Interface().ObjectsFromKey(FullKeyPrefix);
                var localPaths = Directory.GetFiles(LocalDirectory);

                //  Make Dictionaries with the file name and mod date for the local and remote lists
                var remotes = s3Objects.ToDictionary(o => o.Key.Substring(FullKeyPrefix.Length), o => DateTime.Parse(o.LastModified));
                var locals = localPaths.ToDictionary(p => Path.GetFileName(p), p => File.GetLastWriteTimeUtc(p));

                //  Make list of file names that only local or newer on local
                var newNames = locals.Where(l => !remotes.ContainsKey(l.Key) || remotes[l.Key] < l.Value).Select(p => p.Key).ToList();

                uploads = newNames.ConvertAll(fn => new UploadItem { FilePath = Path.Combine(LocalDirectory, fn) });
            }
            catch (Exception e)
            {
                log.Error("GetFilesToUpload error", e);
            }

            log.InfoFormat("End GetFilesToUpload found {0}", uploads.Count);

            return uploads;
        }

        public string LocalDirectory;
        public string FullKeyPrefix;

        private static readonly ILog log = LogManager.GetLogger(typeof(ArchiveFiles));
    }
}
