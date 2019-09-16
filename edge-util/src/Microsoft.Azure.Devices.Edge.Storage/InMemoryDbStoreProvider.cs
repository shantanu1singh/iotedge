// Copyright (c) Microsoft. All rights reserved.
namespace Microsoft.Azure.Devices.Edge.Storage
{
    using System.Collections.Concurrent;
    using Microsoft.Azure.Devices.Edge.Storage.Disk;
    using Microsoft.Azure.Devices.Edge.Util;

    public class InMemoryDbStoreProvider : IDbStoreProvider
    {
        const string DefaultPartitionName = "$Default";
        readonly ConcurrentDictionary<string, InMemoryDbStore> partitionDbStoreDictionary = new ConcurrentDictionary<string, InMemoryDbStore>();
        readonly Option<IStorageSpaceChecker> memoryStorageSpaceChecker;

        public InMemoryDbStoreProvider()
            : this(Option.None<IStorageSpaceChecker>())
        {
        }

        public InMemoryDbStoreProvider(Option<IStorageSpaceChecker> memoryStorageSpaceChecker)
        {
            this.memoryStorageSpaceChecker = memoryStorageSpaceChecker;
            if (this.memoryStorageSpaceChecker.HasValue)
            {
                this.memoryStorageSpaceChecker = this.memoryStorageSpaceChecker.Map(
                    m => {
                        m.SetStorageUsageComputer(GetTotalMemoryUsage); return m;
                    });
            }
        }

        public IDbStore GetDbStore(string partitionName)
        {
            Preconditions.CheckNonWhiteSpace(partitionName, nameof(partitionName));
            IDbStore dbStore = this.partitionDbStoreDictionary.GetOrAdd(partitionName, new InMemoryDbStore());
            return dbStore;
        }

        public IDbStore GetDbStore() => this.GetDbStore(DefaultPartitionName);

        public void RemoveDbStore(string partitionName)
        {
            Preconditions.CheckNonWhiteSpace(partitionName, nameof(partitionName));
            this.partitionDbStoreDictionary.TryRemove(partitionName, out InMemoryDbStore _);
        }

        public void Dispose()
        {
            // No-op
        }

        long GetTotalMemoryUsage()
        {
            long dbSizeInBytes = 0;
            foreach (InMemoryDbStore inMemoryDbStore in this.partitionDbStoreDictionary.Values)
            {
                dbSizeInBytes += inMemoryDbStore.GetDbSizeInBytes();
            }

            return dbSizeInBytes;
        }
    }
}
