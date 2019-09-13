// Copyright (c) Microsoft. All rights reserved.
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Edge.Storage
{
    public interface IDbStore : IKeyValueStore<byte[], byte[]>
    {
        Task BackupAsync(string backupPath);
    }
}
