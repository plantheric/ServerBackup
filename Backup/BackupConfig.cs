using System.Configuration;

namespace nopBackup
{
    class BackupConfig : ConfigurationSection
    {
        public static BackupConfig GetConfig()
        {
            return (BackupConfig)ConfigurationManager.GetSection("BackupConfig") ?? new BackupConfig();
        }

        [ConfigurationProperty("Bucket", IsRequired = true)]
        public string Bucket
        {
            get { return this["Bucket"] as string; }
        }

        [ConfigurationProperty("AccessKey", IsRequired = true)]
        public string AccessKey
        {
            get { return this["AccessKey"] as string; }
        }

        [ConfigurationProperty("SecretKey", IsRequired = true)]
        public string SecretKey
        {
            get { return this["SecretKey"] as string; }
        }

        [ConfigurationProperty("KeyPrefix", IsRequired = true)]
        public string KeyPrefix
        {
            get { return this["KeyPrefix"] as string; }
        }

        [ConfigurationProperty("Database")]
        public DatabaseBackup Database
        {
            get { return this["Database"] as DatabaseBackup; }
        }

        [ConfigurationProperty("Directory")]
        public DirectoryBackup Directory
        {
            get { return this["Directory"] as DirectoryBackup; }
        }
    }

    public class DatabaseBackup : BackupTarget
    {
        [ConfigurationProperty("Server", IsRequired = true)]
        public string Server
        {
            get { return this["Server"] as string; }
        }

        [ConfigurationProperty("Name", IsRequired = true)]
        public string Name
        {
            get { return this["Name"] as string; }
        }
    }

    public class DirectoryBackup : BackupTarget
    {
        [ConfigurationProperty("Path", IsRequired = true)]
        public string Path
        {
            get { return this["Path"] as string; }
        }
    }

    public class BackupTarget : ConfigurationElement
    {
        [ConfigurationProperty("KeyPrefix", IsRequired = true)]
        public string KeyPrefix
        {
            get { return this["KeyPrefix"] as string; }
        }
    }

}
