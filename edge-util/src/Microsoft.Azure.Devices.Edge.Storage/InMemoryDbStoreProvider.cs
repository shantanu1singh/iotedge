// Copyright (c) Microsoft. All rights reserved.
namespace Microsoft.Azure.Devices.Edge.Storage
{
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Edge.Storage.Disk;
    using Microsoft.Azure.Devices.Edge.Util;
    using Microsoft.Extensions.Logging;

    public class InMemoryDbStoreProvider : IDbStoreProvider
    {
        static readonly ILogger Log = Logger.Factory.CreateLogger<InMemoryDbStoreProvider>();

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

        async Task<long> GetTotalMemoryUsage()
        {
            long dbSizeInBytes = 0;
            foreach (var inMemoryDbStore in this.partitionDbStoreDictionary)
            {
                dbSizeInBytes += await inMemoryDbStore.Value.GetDbSizeInBytes();
                Log.LogInformation($"Db Name: {inMemoryDbStore.Key} . DB size {dbSizeInBytes}");
            }

            return dbSizeInBytes;
        }
    }
}
