// Copyright (c) Microsoft. All rights reserved.
namespace Microsoft.Azure.Devices.Edge.Agent.Edgelet
{
    using System.Threading.Tasks;

    public interface IDeviceManager
    {
        Task ReprovisionDeviceAsync();
    }
}
