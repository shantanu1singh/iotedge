// Copyright (c) Microsoft. All rights reserved.
namespace Microsoft.Azure.Devices.Edge.Storage
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Edge.Util;
    using Microsoft.Extensions.Logging;
    using ProtoBuf;

    public class InMemoryDbStoreProvider : IDbStoreProvider
    {
        const string BackupMetadataFileName = "meta.bin";
        const string DefaultPartitionName = "$Default";
        readonly ConcurrentDictionary<string, IDbStore> partitionDbStoreDictionary = new ConcurrentDictionary<string, IDbStore>();
        readonly Option<string> backupPath;
        readonly bool useBackupAndRestore;

        public InMemoryDbStoreProvider()
            : this(Option.None<string>(), false)
        {
        }

        public InMemoryDbStoreProvider(Option<string> backupPath, bool useBackupAndRestore)
        {
            this.backupPath = backupPath;
            this.useBackupAndRestore = useBackupAndRestore;

            // Restore from a previous backup if enabled.
            if (useBackupAndRestore)
            {
                string backupPathValue = this.backupPath.Expect(() => new ArgumentException($"The value of {nameof(backupPath)} needs to be specified if backup and restore is enabled."));
                Preconditions.CheckNonWhiteSpace(backupPathValue, nameof(backupPath));

                this.RestoreDb(backupPathValue);
            }
        }

        private void RestoreDb(string backupPath)
        {
            string backupMetadataFilePath = Path.Combine(backupPath, BackupMetadataFileName);

            if (!File.Exists(backupMetadataFilePath))
            {
                Events.NoBackupsForRestore();
                return;
            }

            try
            {
                using (FileStream file = File.OpenRead(backupMetadataFilePath))
                {
                    BackupMetadataList backupMetadataList = Serializer.Deserialize<BackupMetadataList>(file);
                    BackupMetadata backupMetadata = backupMetadataList.Backups[0];
                    Events.BackupInformation(backupMetadata.Id, backupMetadata.TimestampUtc, backupMetadata.Stores);

                    string latestBackupDirPath = Path.Combine(backupPath, backupMetadata.Id.ToString());
                    if (Directory.Exists(latestBackupDirPath))
                    {
                        foreach (string store in backupMetadata.Stores)
                        {
                            InMemoryDbStore dbStore = new InMemoryDbStore(store, latestBackupDirPath);
                            this.partitionDbStoreDictionary.AddOrUpdate(store, dbStore, (_, __) => dbStore);
                        }
                    }
                    else
                    {
                        Events.NoBackupsForRestore();
                    }
                }
            }
            catch (IOException exception)
            {
                Events.RestoreFailure($"The restore operation failed with error ${exception}.");
            }
        }

        public IDbStore GetDbStore(string partitionName)
        {
            Preconditions.CheckNonWhiteSpace(partitionName, nameof(partitionName));
            IDbStore dbStore = this.partitionDbStoreDictionary.GetOrAdd(partitionName, new InMemoryDbStore(partitionName));
            return dbStore;
        }

        public IDbStore GetDbStore() => this.GetDbStore(DefaultPartitionName);

        public void RemoveDbStore(string partitionName)
        {
            Preconditions.CheckNonWhiteSpace(partitionName, nameof(partitionName));
            this.partitionDbStoreDictionary.TryRemove(partitionName, out IDbStore _);
        }

        public void Close()
        {
            this.CloseAsync().Wait();
        }

        public async Task CloseAsync()
        {
            if (this.useBackupAndRestore)
            {
                Events.StartingBackup();
                string backupPathValue = this.backupPath.Expect(() => new InvalidOperationException($"The value of {nameof(this.backupPath)} is expected to be a valid path if backup and restore is enabled."));
                Guid backupId = Guid.NewGuid();
                string dbBackupDirectory = Path.Combine(backupPathValue, backupId.ToString());

                try
                {
                    Directory.CreateDirectory(dbBackupDirectory);
                    foreach (IDbStore dbStore in this.partitionDbStoreDictionary.Values)
                    {
                        await dbStore.BackupAsync(dbBackupDirectory);
                    }

                    BackupMetadata newBackupMetadata = new BackupMetadata(backupId, DateTime.UtcNow, this.partitionDbStoreDictionary.Keys.ToList());
                    using (FileStream file = File.Create(Path.Combine(backupPathValue, BackupMetadataFileName)))
                    {
                        Serializer.Serialize(file, newBackupMetadata);
                    }
                }
                catch (IOException exception)
                {
                    Events.BackupFailure($"The backup operation failed with error ${exception}. Deleting left-over backup artifacts.");
                    if (Directory.Exists(dbBackupDirectory))
                    {
                        File.Delete(dbBackupDirectory);
                    }
                }
            }
        }

        public void Dispose()
        {
            // No-op
        }

        [ProtoContract]
        class BackupMetadataList
        {
            public BackupMetadataList()
            {
            }

            public BackupMetadataList(IList<BackupMetadata> backups)
            {
                this.Backups = backups;
            }

            [ProtoMember(1)]
            public IList<BackupMetadata> Backups { get; }
        }

        [ProtoContract]
        class BackupMetadata
        {
            public BackupMetadata()
            {
            }

            public BackupMetadata(Guid id, DateTime timestampUtc, IList<string> stores)
            {
                this.Id = id;
                this.TimestampUtc = timestampUtc;
                this.Stores = stores;
            }

            [ProtoMember(1)]
            public Guid Id { get; }

            [ProtoMember(2)]
            public DateTime TimestampUtc { get; set; }

            [ProtoMember(3)]
            public IList<string> Stores { get; set; }
        }

        static class Events
        {
            const int IdStart = UtilEventsIds.InMemoryDbStore;
            static readonly ILogger Log = Logger.Factory.CreateLogger<InMemoryDbStoreProvider>();

            enum EventIds
            {
                StartingBackup = IdStart,
                BackupComplete,
                BackupDirectoryNotFound,
                BackupInformation,
                BackupFailure,
                RestoringFromBackup,
                NoBackupsForRestore,
                RestoreComplete,
                RestoreFailure
            }

            internal static void StartingBackup()
            {
                Log.LogInformation((int)EventIds.StartingBackup, "Starting backup of database.");
            }

            internal static void BackupComplete()
            {
                Log.LogInformation((int)EventIds.BackupComplete, $"Backup of database complete.");
            }

            internal static void BackupDirectoryNotFound(string backupDirectoryPath)
            {
                Log.LogInformation((int)EventIds.BackupDirectoryNotFound, $"The database backup directory {backupDirectoryPath} doesn't exist.");
            }

            internal static void BackupInformation(Guid backupId, DateTime backupTimestamp, IList<string> stores)
            {
                Log.LogDebug((int)EventIds.BackupInformation, $"Backup Info: Timestamp={backupTimestamp}, ID={backupId}. Stores={string.Join(",", stores)}");
            }

            internal static void BackupFailure(string details = null)
            {
                Log.LogError((int)EventIds.BackupFailure, $"Error occurred while attempting to create a database backup. Details: {details}.");
            }

            internal static void RestoringFromBackup()
            {
                Log.LogInformation((int)EventIds.RestoringFromBackup, "Starting restore of database from last backup.");
            }

            internal static void NoBackupsForRestore()
            {
                Log.LogInformation((int)EventIds.NoBackupsForRestore, "No backups were found to restore database with.");
            }

            internal static void RestoreComplete()
            {
                Log.LogInformation((int)EventIds.RestoreComplete, "Database restore from backup complete.");
            }

            internal static void RestoreFailure(string details = null)
            {
                Log.LogError((int)EventIds.RestoreFailure, $"Error occurred while attempting a database restore from the last available backup. Details: {details}.");
            }
        }
    }
}
