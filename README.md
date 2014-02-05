#Backup

Backup is a tool to allow automated backup of file and SQL Server databases to AWS S3.

Once configured you can use Windows Scheduled Tasks to run Backup as desired.

##Configuration
Backup is configured by editing the `Backup.config` file.


	<BackupConfig AccessKey="" SecretKey="" Bucket="" KeyPrefix="">
	  <Databases>
    	<Database Server="" Name="" FullBackupFrequency="5" KeyPrefix="" Lifetime="" />
	  </Databases>
	  <Directories>
		<Directory Path="" KeyPrefix="Images" />
    	<Directory Path="" KeyPrefix="Logs" Lifetime="5"/>
	  </Directories>
	</BackupConfig>

###AccessKey, SecretKey, Bucket
For security reason you should generate an AccessKey and SecretKey specifically for Backup to use. They should be configured to only allow read and write access to the Bucket you want to store your backups in. 

###KeyPrefix
KeyPrefix can be specified for the whole backup and for each backup set. This allows you to build S3 keys to create a folder hierarchy for your backups.

###Lifetime
Backup can use S3 lifecycle rules to auto delete older backups. The Lifetime is specified in days, after which time the object will be expired according to S3's rules. Note that lifetime rules apply to the S3 'folder' so you shouldn't store different backup sets in the same folder.

###FullBackupFrequency
This controls how often Backup performs a full versus an incremental database backup.
When a database backup is uploaded metadata is added to the S3 object to indicate whether it was a full or incremental backup was performed. Backup is then able to look at the most recent backups to determine which backup type to perform. 
