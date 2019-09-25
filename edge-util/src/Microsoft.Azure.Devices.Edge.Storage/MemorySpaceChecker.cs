// Copyright (c) Microsoft. All rights reserved.
namespace Microsoft.Azure.Devices.Edge.Storage
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Edge.Util;
    using Microsoft.Extensions.Logging;

    public class MemorySpaceChecker : IStorageSpaceChecker
    {
        enum MemoryUsageStatus
        {
            Unknown = 0,
            Available,
            Critical,
            Full
        }

        readonly PeriodicTask storageSpaceChecker;
        long maxStorageSpaceBytes;
        Func<Task<long>> getTotalMemoryUsage;
        MemoryUsageStatus memoryUsageStatus;

        public MemorySpaceChecker(TimeSpan checkFrequency, long maxStorageSpaceBytes, Func<Task<long>> getTotalMemoryUsage)
        {
            Preconditions.CheckNotNull(getTotalMemoryUsage, nameof(getTotalMemoryUsage));
            this.maxStorageSpaceBytes = maxStorageSpaceBytes;
            this.getTotalMemoryUsage = getTotalMemoryUsage;
            this.storageSpaceChecker = new PeriodicTask(this.PeriodicTaskCallback, checkFrequency, TimeSpan.FromSeconds(5), Events.Log, "Memory usage check");
        }

        public Func<Task<long>> GetTotalMemoryUsage
        {
            get { return this.getTotalMemoryUsage; }
            set
            {
                Preconditions.CheckNotNull(value, nameof(value));
                this.getTotalMemoryUsage = value;
            }
        }

        public bool IsFull => this.memoryUsageStatus == MemoryUsageStatus.Full;

        public void SetStorageUsageComputer(Func<Task<long>> storageUsageComputer) => this.getTotalMemoryUsage = storageUsageComputer;

        public void SetMaxStorageSize(long maxStorageBytes)
        {
            Events.SetMaxSizeDiskSpaceUsage(maxStorageBytes);
            this.maxStorageSpaceBytes = maxStorageBytes;
        }

        async Task PeriodicTaskCallback()
        {
            try
            {
                this.memoryUsageStatus = await this.GetMemoryUsageStatus();
            }
            catch (Exception e)
            {
                Events.Log.LogWarning(e, "Error updating memory usage status.");
            }
        }

        async Task<MemoryUsageStatus> GetMemoryUsageStatus()
        {
            long memoryUsageBytes = await this.getTotalMemoryUsage();
            Events.Log.LogInformation($"Memory usage bytes: {memoryUsageBytes}");
            Events.Log.LogInformation($"Memory limit bytes: {this.maxStorageSpaceBytes}");

            double usagePercentage = (double)memoryUsageBytes * 100 / this.maxStorageSpaceBytes;
            Events.Log.LogInformation($"Usage %: {usagePercentage}");

            MemoryUsageStatus memoryUsageStatus = GetMemoryUsageStatus(usagePercentage);
            if (memoryUsageStatus != MemoryUsageStatus.Available)
            {
                Events.Log.LogWarning($"High memory usage detected - using {usagePercentage}% of {this.maxStorageSpaceBytes} bytes");
            }

            return memoryUsageStatus;
        }

        static MemoryUsageStatus GetMemoryUsageStatus(double usagePercentage)
        {
            if (usagePercentage < 90)
            {
                return MemoryUsageStatus.Available;
            }

            if (usagePercentage < 100)
            {
                return MemoryUsageStatus.Critical;
            }

            return MemoryUsageStatus.Full;
        }

        static class Events
        {
            public static readonly ILogger Log = Logger.Factory.CreateLogger<MemorySpaceChecker>();
            const int IdStart = UtilEventsIds.MemorySpaceChecker;

            enum EventIds
            {
                Created = IdStart,
                SetMaxSizeDiskSpaceUsage,
                SetMaxPercentageUsage,
                FoundDrive,
                NoMatchingDriveFound,
                ErrorGettingMatchingDrive
            }

            public static void SetMaxSizeDiskSpaceUsage(long maxSizeBytes)
            {
                Log.LogInformation((int)EventIds.SetMaxSizeDiskSpaceUsage, $"Setting maximum memory space usage to {maxSizeBytes} bytes.");
            }
        }
    }
}
