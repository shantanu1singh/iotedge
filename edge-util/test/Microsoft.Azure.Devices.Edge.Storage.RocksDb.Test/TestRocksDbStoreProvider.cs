// Copyright (c) Microsoft. All rights reserved.
namespace Microsoft.Azure.Devices.Edge.Storage.RocksDb.Test
{
    using System;
    using System.IO;
    using Microsoft.Azure.Devices.Edge.Util;

    public class TestRocksDbStoreProvider : IDbStoreProvider
    {
        readonly string rocksDbFolder;
        readonly string rocksDbBackupFolder;
        readonly DbStoreProvider rocksDbStoreProvider;

        public TestRocksDbStoreProvider()
        {
            var options = new RocksDbOptionsProvider(new SystemEnvironment(), true);
            string tempFolder = Path.GetTempPath();
            this.rocksDbFolder = Path.Combine(tempFolder, $"edgeTestDb{Guid.NewGuid()}");
            if (Directory.Exists(this.rocksDbFolder))
            {
                Directory.Delete(this.rocksDbFolder);
            }

            Directory.CreateDirectory(this.rocksDbFolder);

            this.rocksDbBackupFolder = Path.Combine(tempFolder, $"edgeTestBackupDb{Guid.NewGuid()}");
            if (Directory.Exists(this.rocksDbBackupFolder))
            {
                Directory.Delete(this.rocksDbBackupFolder);
            }

            Directory.CreateDirectory(this.rocksDbBackupFolder);

            this.rocksDbStoreProvider = DbStoreProvider.Create(options, this.rocksDbFolder, new string[0], Option.Some(this.rocksDbBackupFolder), true);
        }

        public void Dispose()
        {
            this.rocksDbStoreProvider?.Close();
            this.rocksDbStoreProvider?.Dispose();
            if (!string.IsNullOrWhiteSpace(this.rocksDbFolder) && Directory.Exists(this.rocksDbFolder))
            {
                Directory.Delete(this.rocksDbFolder, true);
            }
        }

        public IDbStore GetDbStore(string partitionName) => this.rocksDbStoreProvider.GetDbStore(partitionName);

        public IDbStore GetDbStore() => this.rocksDbStoreProvider.GetDbStore("default");

        public void RemoveDbStore(string partitionName) => throw new NotImplementedException();

        public void Close() => this.rocksDbStoreProvider.Close();
    }
}
