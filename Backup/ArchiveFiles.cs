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
            var localFiles = new List<string>();
            try
            {
                log.InfoFormat("Start GetFilesToUpload for {0}", LocalDirectory);

                localFiles = new List<string>(Directory.GetFiles(LocalDirectory));
                localFiles = localFiles.ConvertAll(f => Path.GetFileName(f));

                List<S3Object> s3Objects = new S3Interface().ObjectsFromKey(FullKeyPrefix);
                List<string> s3Files = s3Objects.ConvertAll(f => f.Key.Substring(FullKeyPrefix.Length));

                localFiles = new List<string>(localFiles.Except(s3Files));
                localFiles = localFiles.ConvertAll(f => Path.Combine(LocalDirectory, f));
            }
            catch (Exception e)
            {
                log.Error("GetFilesToUpload error", e);
            }

            log.InfoFormat("End GetFilesToUpload found {0}", localFiles.Count);

            return localFiles.ConvertAll(f => new UploadItem { FilePath = f });
        }

        public string LocalDirectory;
        public string FullKeyPrefix;

        private static readonly ILog log = LogManager.GetLogger(typeof(ArchiveFiles));
    }
}
