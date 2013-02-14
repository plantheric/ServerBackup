using System;
using System.Collections.Generic;
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

        [ConfigurationProperty("NoUpload", DefaultValue=false)]
        public bool NoUpload
        {
            get { return (bool)this["NoUpload"]; }
        }

        [ConfigurationProperty("Databases")]
        [ConfigurationCollection(typeof(ConfigCollection<DatabaseBackup>), AddItemName = "Database")]
        public ConfigCollection<DatabaseBackup> Databases
        {
            get { return this["Databases"] as ConfigCollection<DatabaseBackup>; }
        }

        [ConfigurationProperty("Directories")]
        [ConfigurationCollection(typeof(ConfigCollection<DirectoryBackup>), AddItemName = "Directory")]
        public ConfigCollection<DirectoryBackup> Directories
        {
            get { return this["Directories"] as ConfigCollection<DirectoryBackup>; }
        }

        public string FullKeyPrefix(string subKeyPrefix)
        {
            return KeyPrefix + @"/" + subKeyPrefix + @"/";
        }
    }

    public class ConfigCollection<T> : ConfigurationElementCollection, IEnumerable<T> where T : BackupTarget, new() 
    {
        public T this[int index]
        {
            get { return base.BaseGet(index) as T; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new T();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((T)element).KeyPrefix;
        }

        public new IEnumerator<T> GetEnumerator()
        {
            int count = base.Count;
            for (int i = 0; i < count; i++)
            {
                yield return this[i];
            }
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
        [ConfigurationProperty("FullBackupFrequency", DefaultValue = 1)]
        public int FullBackupFrequency
        {
            get { return (int)this["FullBackupFrequency"]; }
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
