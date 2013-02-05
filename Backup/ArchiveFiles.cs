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

                locals.ForEach(f => log.DebugFormat("Local , {0} : {1}", f.Key, f.Value));
                remotes.ForEach(f => log.DebugFormat("Remote, {0} : {1}", f.Key, f.Value));

                var startDate = DateTime.Now.AddDays(-BackupLifetime);

                //  Make list of file names that only local or newer on local
                var newNames = locals.Where(l =>                                                        //  Upload file when:-
                                            (!remotes.ContainsKey(l.Key) && l.Value > startDate) ||     //  File not on remote and not old enough to be purged
                                            (remotes.ContainsKey(l.Key) && l.Value > remotes[l.Key])).  //  File on remote but newer version on local

                                            Select(p => p.Key).ToList();                                //  convert to list of file names

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
        public int BackupLifetime;

        private static readonly ILog log = LogManager.GetLogger(typeof(ArchiveFiles));
    }
}
