// Copyright (c) Microsoft. All rights reserved.
using System;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Edge.Storage
{
    public class NullStorageSpaceChecker : IStorageSpaceChecker
    {
        public void SetMaxStorageSize(long maxStorageBytes)
        {
        }

        public void SetStorageUsageComputer(Func<Task<long>> storageUsageComputer)
        {
        }

        public bool IsFull => false;
    }
}
