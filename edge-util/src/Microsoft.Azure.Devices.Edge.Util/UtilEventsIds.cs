// Copyright (c) Microsoft. All rights reserved.
namespace Microsoft.Azure.Devices.Edge.Util
{
    public struct UtilEventsIds
    {
        public const int HttpUdsMessageHandler = EventIdStart;
        public const int EdgeletWorkloadClient = EventIdStart + 100;
        public const int InMemoryDbStore = EventIdStart + 200;
        public const int RocksDb = EventIdStart + 300;
        const int EventIdStart = 100000;
    }
}
