// Copyright (c) Microsoft. All rights reserved.
namespace Microsoft.Azure.Devices.Edge.Storage.RocksDb
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using Microsoft.Azure.Devices.Edge.Util;
    using Microsoft.Azure.Devices.Edge.Util.Concurrency;
    using Microsoft.Extensions.Logging;
    using RocksDbSharp;

    /// <summary>
    /// Wrapper around RocksDb. This is mainly needed because each ColumnFamilyDbStore contains an instance of this object,
    /// and hence it could get disposed multiple times. This class makes sure the underlying RocksDb instance is disposed only once.
    /// </summary>
    sealed class RocksDbWrapper : IRocksDb
    {
        static readonly ILogger Log = Logger.Factory.CreateLogger<RocksDbWrapper>();

        readonly AtomicBoolean isDisposed = new AtomicBoolean(false);
        readonly RocksDb db;
        readonly string path;
        readonly DbOptions dbOptions;
        readonly Option<string> backupPath;
        readonly bool useBackupAndRestore;

        RocksDbWrapper(DbOptions dbOptions, RocksDb db, string path, Option<string> backupPath, bool useBackupAndRestore)
        {
            this.db = db;
            this.path = path;
            this.dbOptions = dbOptions;
            this.useBackupAndRestore = useBackupAndRestore;
            this.backupPath = backupPath;
        }

        public static RocksDbWrapper Create(IRocksDbOptionsProvider optionsProvider, string path, IEnumerable<string> partitionsList, Option<string> storageBackupPath, bool useBackupAndRestore)
        {
            Preconditions.CheckNonWhiteSpace(path, nameof(path));
            Preconditions.CheckNotNull(optionsProvider, nameof(optionsProvider));
            DbOptions dbOptions = Preconditions.CheckNotNull(optionsProvider.GetDbOptions());

            if (useBackupAndRestore)
            {
                string backupPath = storageBackupPath.Expect(() => new ArgumentException($"The value of {nameof(storageBackupPath)} needs to be specified if backup and restore is enabled."));
                Preconditions.CheckNonWhiteSpace(backupPath, nameof(storageBackupPath));

                RestoreDb(dbOptions, path, backupPath);
            }

            IEnumerable<string> existingColumnFamilies = ListColumnFamilies(dbOptions, path);
            IEnumerable<string> columnFamiliesList = existingColumnFamilies.Union(Preconditions.CheckNotNull(partitionsList, nameof(partitionsList)), StringComparer.OrdinalIgnoreCase).ToList();
            var columnFamilies = new ColumnFamilies();
            foreach (string columnFamilyName in columnFamiliesList)
            {
                columnFamilies.Add(columnFamilyName, optionsProvider.GetColumnFamilyOptions());
            }

            RocksDb db = RocksDb.Open(dbOptions, path, columnFamilies);
            var rocksDbWrapper = new RocksDbWrapper(dbOptions, db, path, storageBackupPath, useBackupAndRestore);
            return rocksDbWrapper;
        }

        public void Compact(ColumnFamilyHandle cf) => this.db.CompactRange(string.Empty, string.Empty, cf);

        public IEnumerable<string> ListColumnFamilies() => ListColumnFamilies(this.dbOptions, this.path);

        public ColumnFamilyHandle GetColumnFamily(string columnFamilyName) => this.db.GetColumnFamily(columnFamilyName);

        public ColumnFamilyHandle CreateColumnFamily(ColumnFamilyOptions columnFamilyOptions, string entityName) => this.db.CreateColumnFamily(columnFamilyOptions, entityName);

        public void DropColumnFamily(string columnFamilyName) => this.db.DropColumnFamily(columnFamilyName);

        public byte[] Get(byte[] key, ColumnFamilyHandle handle) => this.db.Get(key, handle);

        public void Put(byte[] key, byte[] value, ColumnFamilyHandle handle) => this.db.Put(key, value, handle);

        public void Remove(byte[] key, ColumnFamilyHandle handle) => this.db.Remove(key, handle);

        public Iterator NewIterator(ColumnFamilyHandle handle, ReadOptions readOptions) => this.db.NewIterator(handle, readOptions);

        public Iterator NewIterator(ColumnFamilyHandle handle) => this.db.NewIterator(handle);

        public void Dispose()
        {
            Log.LogInformation($"RocksDbWrapper Dispose called");
            if (!this.isDisposed.GetAndSet(true))
            {
                this.Backup();
                this.db?.Dispose();
            }
        }

        static void RestoreDb(DbOptions dbOptions, string path, string backupPath)
        {
            Log.LogInformation($"Restore called {backupPath}");

            // Backup DB from last backup if available.
            if (Directory.Exists(backupPath))
            {
                Log.LogInformation("backup Directory sizeon restore:" + GetDirectorySize(backupPath));
                Events.RestoringFromBackup();
                IntPtr backupEngine = Native.Instance.rocksdb_backup_engine_open(dbOptions.Handle, backupPath, out IntPtr err);
                Debug.Assert(err == IntPtr.Zero);

                IntPtr backupInfo = Native.Instance.rocksdb_backup_engine_get_backup_info(backupEngine);
                int numberOfBackups = Native.Instance.rocksdb_backup_engine_info_count(backupInfo);

                if (numberOfBackups > 0)
                {
                    for (int i = 0; i < numberOfBackups; i++)
                    {
                        Events.BackupInformation(
                            i,
                            Native.Instance.rocksdb_backup_engine_info_timestamp(backupInfo, i),
                            Native.Instance.rocksdb_backup_engine_info_backup_id(backupInfo, i),
                            Native.Instance.rocksdb_backup_engine_info_size(backupInfo, i),
                            Native.Instance.rocksdb_backup_engine_info_number_files(backupInfo, i));
                    }

                    IntPtr restore_options = Native.Instance.rocksdb_restore_options_create();
                    Native.Instance.rocksdb_backup_engine_restore_db_from_latest_backup(
                        backupEngine, path, path, restore_options, out err);
                    //Debug.Assert(err == IntPtr.Zero);

                    Native.Instance.rocksdb_restore_options_destroy(restore_options);
                    Events.RestoreComplete();
                }
                else
                {
                    Events.NoBackupsForRestore();
                }

                Native.Instance.rocksdb_backup_engine_info_destroy(backupInfo);
                Native.Instance.rocksdb_backup_engine_close(backupEngine);
            }
            else
            {
                Events.BackupDirectoryNotFound(backupPath);
            }
        }

        public void Backup()
        {
            string backupPath1 = this.backupPath.OrDefault();
            Log.LogInformation($"Backup called {backupPath1}");
            Log.LogInformation("Directory size:" + GetDirectorySize(this.path));
            Log.LogInformation("backup Directory size:" + GetDirectorySize(backupPath1));

            if (this.useBackupAndRestore)
            {
                Events.StartingBackup();
                string backupPathValue = this.backupPath.Expect(() => new InvalidOperationException($"The value of {nameof(this.backupPath)} is expected to be a valid path if backup and restore is enabled."));

                IntPtr backupEngine = Native.Instance.rocksdb_backup_engine_open(this.dbOptions.Handle, backupPathValue, out IntPtr err);
                Debug.Assert(err == IntPtr.Zero);

                // Create a new DB backup.
                //Native.Instance.rocksdb_backup_engine_create_new_backup(backupEngine, this.db.Handle, out err);
                Native.Instance.rocksdb_backup_engine_create_new_backup_flush(backupEngine, this.db.Handle, true, out err);
                Debug.Assert(err == IntPtr.Zero);

                IntPtr backupInfo = Native.Instance.rocksdb_backup_engine_get_backup_info(backupEngine);
                int numberOfBackups = Native.Instance.rocksdb_backup_engine_info_count(backupInfo);
                uint lastBackupId = Native.Instance.rocksdb_backup_engine_info_backup_id(backupInfo, numberOfBackups-1);

                Events.BackupInformation(
                    numberOfBackups - 1,
                    Native.Instance.rocksdb_backup_engine_info_timestamp(backupInfo, numberOfBackups - 1),
                    lastBackupId,
                    Native.Instance.rocksdb_backup_engine_info_size(backupInfo, numberOfBackups - 1),
                    Native.Instance.rocksdb_backup_engine_info_number_files(backupInfo, numberOfBackups - 1));

                Native.Instance.rocksdb_backup_engine_verify_backup(backupEngine, lastBackupId, out err);
                if (err != IntPtr.Zero)
                {
                    Events.BackupVerificationFailed();
                }
                else
                {
                    // Purge old backups but the last one.
                    Native.Instance.rocksdb_backup_engine_purge_old_backups(backupEngine, 1, out err);
                    Log.LogInformation("Purged old backups: " + err.ToInt64());
                    Debug.Assert(err == IntPtr.Zero);
                }

                Log.LogInformation("backup Directory size:" + GetDirectorySize(backupPathValue));
                Native.Instance.rocksdb_backup_engine_close(backupEngine);
                Events.BackupComplete();
            }
        }

        static long GetDirectorySize(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                return 0;
            }

            DirectoryInfo directory = new DirectoryInfo(directoryPath);
            return GetDirectorySize(directory);
        }

        static long GetDirectorySize(DirectoryInfo directory)
        {
            long size = 0;

            // Get size for all files in directory
            FileInfo[] files = directory.GetFiles("*", SearchOption.AllDirectories);
            foreach (FileInfo file in files)
            {
                size += file.Length;
            }

            return size;
        }

        static IEnumerable<string> ListColumnFamilies(DbOptions dbOptions, string path)
        {
            Preconditions.CheckNonWhiteSpace(path, nameof(path));
            // ListColumnFamilies will throw if the DB doesn't exist yet, so wrap it in a try catch.
            IEnumerable<string> columnFamilies = null;
            try
            {
                columnFamilies = RocksDb.ListColumnFamilies(dbOptions, path);
            }
            catch
            {
                // ignored since ListColumnFamilies will throw if the DB doesn't exist yet.
            }

            return columnFamilies ?? Enumerable.Empty<string>();
        }

        static class Events
        {
            const int IdStart = UtilEventsIds.RocksDb;
            static readonly ILogger Log = Logger.Factory.CreateLogger<RocksDbWrapper>();

            enum EventIds
            {
                StartingBackup = IdStart,
                BackupComplete,
                BackupInfo,
                BackupVerificationFailure,
                RestoringFromBackup,
                NoBackupsForRestore,
                RestoreComplete,
                BackupDirectoryNotFound
            }

            internal static void StartingBackup()
            {
                Log.LogInformation((int)EventIds.StartingBackup, "Starting backup of database.");
            }

            internal static void BackupComplete()
            {
                Log.LogInformation((int)EventIds.BackupComplete, $"Backup of database complete.");
            }

            internal static void BackupVerificationFailed()
            {
                Log.LogError((int)EventIds.BackupVerificationFailure, $"Verification of created backup failed.");
            }

            internal static void RestoringFromBackup()
            {
                Log.LogInformation((int)EventIds.RestoringFromBackup, "Starting restore of database from last backup.");
            }

            internal static void BackupInformation(int backupIndex, long backupTimestamp, uint backupId, ulong backupSize, uint numberOfFilesInBackup)
            {
                Log.LogDebug((int)EventIds.BackupComplete, $"Backup Info: Index={backupIndex}, Timestamp={backupTimestamp}, ID={backupId}, Size={backupSize}, NumberOfFiles={numberOfFilesInBackup}.");
            }

            internal static void NoBackupsForRestore()
            {
                Log.LogInformation((int)EventIds.NoBackupsForRestore, "No backups were found to restore database with.");
            }

            internal static void RestoreComplete()
            {
                Log.LogInformation((int)EventIds.RestoreComplete, "Database restore from backup complete.");
            }

            internal static void BackupDirectoryNotFound(string backupDirectoryPath)
            {
                Log.LogInformation((int)EventIds.BackupDirectoryNotFound, $"The database backup directory {backupDirectoryPath} doesn't exist.");
            }
        }
    }
}
