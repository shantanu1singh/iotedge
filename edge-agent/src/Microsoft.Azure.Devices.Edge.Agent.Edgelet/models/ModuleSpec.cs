// Copyright (c) Microsoft. All rights reserved.
namespace Microsoft.Azure.Devices.Edge.Agent.Edgelet.Models
{
    using System.Collections.Generic;
    using Microsoft.Azure.Devices.Edge.Agent.Core;
    using Microsoft.Azure.Devices.Edge.Util;

    public class ModuleSpec
    {
        public ModuleSpec(string name, string type, PullPolicy pullPolicy, object settings)
            : this(name, type, pullPolicy, settings, new List<EnvVar>())
        {
        }

        public ModuleSpec(string name, string type, PullPolicy pullPolicy, object settings, IEnumerable<EnvVar> environmentVariables)
        {
            this.PullPolicy = PullPolicy;
            this.Name = Preconditions.CheckNonWhiteSpace(name, nameof(name));
            this.Type = Preconditions.CheckNonWhiteSpace(type, nameof(type));
            this.Settings = Preconditions.CheckNotNull(settings, nameof(settings));
            this.EnvironmentVariables = Preconditions.CheckNotNull(environmentVariables, nameof(environmentVariables));
        }

        public string Name { get; }

        public string Type { get; }

        public object Settings { get; }

        public IEnumerable<EnvVar> EnvironmentVariables { get; }

        public PullPolicy PullPolicy { get; }
    }
}
