// Copyright (c) Microsoft. All rights reserved.
namespace Microsoft.Azure.Devices.Edge.Storage.Disk
{
    using System;

    public interface IStorageSpaceChecker
    {
        void SetMaxStorageSize(long maxStorageBytes);

        void SetStorageUsageComputer(Func<long> storageUsageComputer);

        bool IsFull { get; }
    }
}
