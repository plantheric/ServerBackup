using System;
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

        [ConfigurationProperty("Directories")]
        [ConfigurationCollection(typeof(DirectoriesBackup), AddItemName="Directory")]
        public DirectoriesBackup Directories
        {
            get { return this["Directories"] as DirectoriesBackup; }
        }

        public string FullKeyPrefix(string subKeyPrefix)
        {
            return KeyPrefix + @"/" + subKeyPrefix + @"/";
        }
    }

    public class DirectoriesBackup : ConfigurationElementCollection
    {
        public DirectoryBackup this[int index]
        {
            get { return base.BaseGet(index) as DirectoryBackup; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new DirectoryBackup();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DirectoryBackup)element).KeyPrefix;
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

        [ConfigurationProperty("Lifetime", DefaultValue = int.MaxValue)]
        public int Lifetime
        {
            get { return (int)this["Lifetime"]; }
        }
    }

}
