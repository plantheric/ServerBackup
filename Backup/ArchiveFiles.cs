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

                var localNames = new List<string>(Directory.GetFiles(LocalDirectory)).ConvertAll(f => Path.GetFileName(f));

                var s3Objects = new S3Interface().ObjectsFromKey(FullKeyPrefix);
                var remoteNames = s3Objects.ConvertAll(f => f.Key.Substring(FullKeyPrefix.Length));

                var newNames = new List<string>(localNames.Except(remoteNames));

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
