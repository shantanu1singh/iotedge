// Copyright (c) Microsoft. All rights reserved.
namespace Microsoft.Azure.Devices.Edge.Agent.Core.ConfigSources
{
    using System;
    using System.IO;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Edge.Agent.Core.Serde;
    using Microsoft.Azure.Devices.Edge.Util;
    using Microsoft.Azure.Devices.Edge.Util.Concurrency;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Primitives;

    public class FileConfigSource : IConfigSource
    {
        const double FileChangeWatcherDebounceInterval = 500;

        readonly FileSystemWatcher watcher;
        readonly PhysicalFileProvider fileProvider;
        readonly string configFilePath;
        readonly IDisposable watcherSubscription;
        readonly AtomicReference<DeploymentConfigInfo> current;
        readonly AsyncLock sync;
        readonly ISerde<DeploymentConfigInfo> serde;
        IChangeToken fileChangeToken;

        FileConfigSource(FileSystemWatcher watcher, PhysicalFileProvider fileProvider, string fileName, DeploymentConfigInfo initial, IConfiguration configuration, ISerde<DeploymentConfigInfo> serde)
        {
            this.watcher = Preconditions.CheckNotNull(watcher, nameof(watcher));
            this.Configuration = Preconditions.CheckNotNull(configuration, nameof(configuration));
            this.current = new AtomicReference<DeploymentConfigInfo>(Preconditions.CheckNotNull(initial, nameof(initial)));
            this.serde = Preconditions.CheckNotNull(serde, nameof(serde));
            this.configFilePath = Path.Combine(fileProvider.Root, fileName);
            this.fileProvider = Preconditions.CheckNotNull(fileProvider, nameof(fileProvider));

            this.sync = new AsyncLock();
            this.watcherSubscription = Observable
                .FromEventPattern<FileSystemEventArgs>(this.watcher, "Changed")
                // Rx.NET's "Throttle" is really "Debounce". An unfortunate naming mishap.
                .Throttle(TimeSpan.FromMilliseconds(FileChangeWatcherDebounceInterval))
                .Subscribe(this.WatcherOnChanged);
            this.watcher.EnableRaisingEvents = true;

            this.fileChangeToken = this.fileProvider.Watch(fileName);
            this.fileChangeToken.RegisterChangeCallback(this.WatcherOnChanged, default);
            Events.Created(this.configFilePath);
        }

        public IConfiguration Configuration { get; }

        public static async Task<FileConfigSource> Create(string configFilePath, IConfiguration configuration, ISerde<DeploymentConfigInfo> serde, bool usePollingFileWatcher)
        {
            Preconditions.CheckNotNull(serde, nameof(serde));
            string path = Preconditions.CheckNonWhiteSpace(Path.GetFullPath(configFilePath), nameof(configFilePath));
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Invalid config file path", path);
            }

            string directoryName = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);

            DeploymentConfigInfo initial = await ReadFromDisk(path, serde);

            if (usePollingFileWatcher)
            {
                PhysicalFileProvider fileProvider = new PhysicalFileProvider(directoryName);
            }
            else
            {
                var watcher = new FileSystemWatcher(directoryName, fileName)
                {
                    NotifyFilter = NotifyFilters.LastWrite
                };
            }

            return new FileConfigSource(fileProvider, fileName, initial, configuration, serde);
        }

        public Task<DeploymentConfigInfo> GetDeploymentConfigInfoAsync() => Task.FromResult(this.current.Value);

        public void Dispose()
        {
            this.fileProvider.Dispose();
        }

        static async Task<DeploymentConfigInfo> ReadFromDisk(string path, ISerde<DeploymentConfigInfo> serde)
        {
            string json = await DiskFile.ReadAllAsync(path);
            DeploymentConfigInfo deploymentConfig = serde.Deserialize(json);
            return deploymentConfig;
        }

        void UpdateCurrent(DeploymentConfigInfo updated)
        {
            DeploymentConfigInfo snapshot = this.current.Value;
            if (!this.current.CompareAndSet(snapshot, updated))
            {
                throw new InvalidOperationException("Invalid update current moduleset operation.");
            }
        }

        async void WatcherOnChanged(object state)
        {
            try
            {
                using (await this.sync.LockAsync())
                {
                    DeploymentConfigInfo newConfig = await ReadFromDisk(this.configFilePath, this.serde);
                    this.UpdateCurrent(newConfig);
                }
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                Events.NewConfigurationFailed(ex, this.configFilePath);
            }
            finally
            {
                this.fileChangeToken = this.fileProvider.Watch(Path.GetFileName(this.configFilePath));
                this.fileChangeToken.RegisterChangeCallback(this.WatcherOnChanged, default);
            }
        }

        static class Events
        {
            const int IdStart = AgentEventIds.FileConfigSource;
            static readonly ILogger Log = Logger.Factory.CreateLogger<FileConfigSource>();

            enum EventIds
            {
                Created = IdStart,
                NewConfigurationFailed
            }

            public static void Created(string filename)
            {
                Log.LogDebug((int)EventIds.Created, $"FileConfigSource created with filename {filename}");
            }

            public static void NewConfigurationFailed(Exception exception, string filename)
            {
                Log.LogError((int)EventIds.NewConfigurationFailed, exception, $"FileConfigSource failed reading new configuration file, {filename}");
            }
        }
    }
}
