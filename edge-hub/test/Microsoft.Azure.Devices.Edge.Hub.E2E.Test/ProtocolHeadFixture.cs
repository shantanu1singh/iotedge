// Copyright (c) Microsoft. All rights reserved.
namespace Microsoft.Azure.Devices.Edge.Hub.E2E.Test
{
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Azure.Devices.Edge.Hub.Amqp;
    using Microsoft.Azure.Devices.Edge.Hub.Core;
    using Microsoft.Azure.Devices.Edge.Hub.Core.Cloud;
    using Microsoft.Azure.Devices.Edge.Hub.Core.Config;
    using Microsoft.Azure.Devices.Edge.Hub.Http;
    using Microsoft.Azure.Devices.Edge.Hub.Mqtt;
    using Microsoft.Azure.Devices.Edge.Hub.Service;
    using Microsoft.Azure.Devices.Edge.Storage;
    using Microsoft.Azure.Devices.Edge.Util.Test.Common;
    using Microsoft.Extensions.Logging;
    using Constants = Microsoft.Azure.Devices.Edge.Hub.Service.Constants;

    public class ProtocolHeadFixture : IDisposable
    {
        InternalProtocolHeadFixture internalFixture;

        public ProtocolHeadFixture()
        {
            this.internalFixture = new InternalProtocolHeadFixture();
            this.ProtocolHead = this.internalFixture.ProtocolHead;
            //this.ProtocolHead = InternalProtocolHeadFixture.Instance.ProtocolHead;
            //this.DbStoreProvider = InternalProtocolHeadFixture.Instance.DbStoreProvider;
        }

        public IProtocolHead ProtocolHead { get; private set; }

        //IDbStoreProvider DbStoreProvider { get; }

        public void Dispose()
        {
            this.internalFixture?.Dispose();
            this.ProtocolHead = null;
            this.internalFixture = null;
        }

        public class InternalProtocolHeadFixture : IDisposable
        {
            //IContainer container;
            IProtocolHead protocolHead;
            //IDbStoreProvider dbStoreProvider;
            Hosting hosting;

            internal InternalProtocolHeadFixture()
            {
                bool.TryParse(ConfigHelper.TestConfig["Tests_StartEdgeHubService"], out bool shouldStartEdge);
                if (shouldStartEdge)
                {
                    this.StartProtocolHead().Wait();
                }
            }

            ~InternalProtocolHeadFixture()
            {
                this.Dispose();
                //this.protocolHead?.Dispose();
                //this.dbStoreProvider?.Dispose();
            }

            public static InternalProtocolHeadFixture Instance { get; } = new InternalProtocolHeadFixture();

            public IProtocolHead ProtocolHead => this.protocolHead;

            //public IDbStoreProvider DbStoreProvider => this.dbStoreProvider;

            // Device SDK caches the AmqpTransportSettings that are set the first time and ignores
            // all the settings used thereafter from that process. So set up a dummy connection using the test
            // AmqpTransportSettings, so that Device SDK caches it and uses it thereafter
            static async Task ConnectToIotHub(string connectionString)
            {
                DeviceClient dc = DeviceClient.CreateFromConnectionString(connectionString, TestSettings.AmqpTransportSettings);
                await dc.OpenAsync();
                await dc.CloseAsync();
            }

            async Task StartProtocolHead()
            {
                string certificateValue = await SecretsHelper.GetSecret("IotHubMqttHeadCert");
                byte[] cert = Convert.FromBase64String(certificateValue);
                var certificate = new X509Certificate2(cert);
                // TODO for now this is empty as will suffice for SAS X.509 thumbprint auth but we will need other CA certs for X.509 CA validation
                var trustBundle = new List<X509Certificate2>();

                string edgeDeviceConnectionString = await SecretsHelper.GetSecretFromConfigKey("edgeCapableDeviceConnStrKey");

                // TODO - After IoTHub supports MQTT, remove this and move to using MQTT for upstream connections
                await ConnectToIotHub(edgeDeviceConnectionString);

                ConfigHelper.TestConfig[Constants.ConfigKey.IotHubConnectionString] = edgeDeviceConnectionString;
                Hosting hosting = Hosting.Initialize(ConfigHelper.TestConfig, certificate, new DependencyManager(ConfigHelper.TestConfig, certificate, trustBundle), true);
                this.hosting = hosting;
                //this.container = hosting.Container;
                IContainer container = hosting.Container;

                // CloudConnectionProvider and RoutingEdgeHub have a circular dependency. So set the
                // EdgeHub on the CloudConnectionProvider before any other operation
                ICloudConnectionProvider cloudConnectionProvider = await container.Resolve<Task<ICloudConnectionProvider>>();
                IEdgeHub edgeHub = await container.Resolve<Task<IEdgeHub>>();
                cloudConnectionProvider.BindEdgeHub(edgeHub);

                IConfigSource configSource = await container.Resolve<Task<IConfigSource>>();
                ConfigUpdater configUpdater = await container.Resolve<Task<ConfigUpdater>>();
                await configUpdater.Init(configSource);

                ILogger logger = container.Resolve<ILoggerFactory>().CreateLogger("EdgeHub");
                MqttProtocolHead mqttProtocolHead = await container.Resolve<Task<MqttProtocolHead>>();
                AmqpProtocolHead amqpProtocolHead = await container.Resolve<Task<AmqpProtocolHead>>();
                var httpProtocolHead = new HttpProtocolHead(hosting.WebHost);
                this.protocolHead = new EdgeHubProtocolHead(new List<IProtocolHead> { mqttProtocolHead, amqpProtocolHead, httpProtocolHead }, logger);
                await this.protocolHead.StartAsync();
            }

            public void Dispose()
            {
                this.protocolHead?.CloseAsync(CancellationToken.None).Wait();

                if (this.hosting != null)
                {
                    IContainer container = this.hosting.Container;
                    IDbStoreProvider dbStoreProvider = container.Resolve<IDbStoreProvider>();
                    dbStoreProvider.Close();

                    this.protocolHead?.Dispose();
                    dbStoreProvider.Dispose();

                    this.hosting.Container.Dispose();
                    //this.hosting.WebHost.Dispose();
                }
            }
        }
    }
}
