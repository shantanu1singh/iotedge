//----------------------
// <auto-generated>
//     Generated using the NSwag toolchain v13.1.3.0 (NJsonSchema v10.0.27.0 (Newtonsoft.Json v11.0.0.0)) (http://NSwag.org)
// </auto-generated>
//----------------------

//#pragma warning disable 108 // Disable "CS0108 '{derivedDto}.ToJson()' hides inherited member '{dtoBase}.ToJson()'. Use the new keyword if hiding was intended."
//#pragma warning disable 114 // Disable "CS0114 '{derivedDto}.RaisePropertyChanged(String)' hides inherited member 'dtoBase.RaisePropertyChanged(String)'. To make the current member override that implementation, add the override keyword. Otherwise add the new keyword."
//#pragma warning disable 472 // Disable "CS0472 The result of the expression is always 'false' since a value of type 'Int32' is never equal to 'null' of type 'Int32?'
//#pragma warning disable 1573 // Disable "CS1573 Parameter '...' has no matching param tag in the XML comment for ...
//#pragma warning disable 1591 // Disable "CS1591 Missing XML comment for publicly visible type or member ..."

namespace Microsoft.Azure.Devices.Edge.Agent.Edgelet.Test.TestServer.Controllers
{
    #pragma warning disable // Disable all warnings

    using System = global::System;

    [System.CodeDom.Compiler.GeneratedCode("NSwag", "13.1.3.0 (NJsonSchema v10.0.27.0 (Newtonsoft.Json v11.0.0.0))")]
    public interface IController
    {
        /// <summary>List modules.</summary>
        /// <param name="api_version">The version of the API.</param>
        /// <returns>Ok</returns>
        System.Threading.Tasks.Task<ModuleList> ListModulesAsync(string api_version);

        /// <summary>Create module.</summary>
        /// <param name="api_version">The version of the API.</param>
        /// <returns>Created</returns>
        System.Threading.Tasks.Task<ModuleDetails> CreateModuleAsync(string api_version, ModuleSpec module);

        /// <summary>Get a module's status.</summary>
        /// <param name="api_version">The version of the API.</param>
        /// <param name="name">The name of the module to get. (urlencoded)</param>
        /// <returns>Ok</returns>
        System.Threading.Tasks.Task<ModuleDetails> GetModuleAsync(string api_version, string name);

        /// <summary>Update a module.</summary>
        /// <param name="api_version">The version of the API.</param>
        /// <param name="name">The name of the module to update. (urlencoded)</param>
        /// <param name="start">Flag indicating whether module should be started after updating.</param>
        /// <returns>Ok</returns>
        System.Threading.Tasks.Task<ModuleDetails> UpdateModuleAsync(string api_version, string name, bool start, ModuleSpec module);

        /// <summary>Delete a module.</summary>
        /// <param name="api_version">The version of the API.</param>
        /// <param name="name">The name of the module to delete. (urlencoded)</param>
        /// <returns>No Content</returns>
        System.Threading.Tasks.Task DeleteModuleAsync(string api_version, string name);

        /// <summary>Prepare to update a module.</summary>
        /// <param name="api_version">The version of the API.</param>
        /// <param name="name">The name of the module to update. (urlencoded)</param>
        /// <returns>No Content</returns>
        System.Threading.Tasks.Task PrepareUpdateModuleAsync(string api_version, string name, ModuleSpec module);

        /// <summary>Start a module.</summary>
        /// <param name="api_version">The version of the API.</param>
        /// <param name="name">The name of the module to start. (urlencoded)</param>
        /// <returns>No Content</returns>
        System.Threading.Tasks.Task StartModuleAsync(string api_version, string name);

        /// <summary>Stop a module.</summary>
        /// <param name="api_version">The version of the API.</param>
        /// <param name="name">The name of the module to stop. (urlencoded)</param>
        /// <returns>No Content</returns>
        System.Threading.Tasks.Task StopModuleAsync(string api_version, string name);

        /// <summary>Restart a module.</summary>
        /// <param name="api_version">The version of the API.</param>
        /// <param name="name">The name of the module to restart. (urlencoded)</param>
        /// <returns>No Content</returns>
        System.Threading.Tasks.Task RestartModuleAsync(string api_version, string name);

        /// <summary>Get module logs.</summary>
        /// <param name="api_version">The version of the API.</param>
        /// <param name="name">The name of the module to obtain logs for. (urlencoded)</param>
        /// <param name="follow">Return the logs as a stream.</param>
        /// <param name="tail">Only return this number of lines from the end of the logs.</param>
        /// <param name="since">Only return logs since this time, as a UNIX timestamp.</param>
        /// <returns>Logs returned as a string in response body</returns>
        System.Threading.Tasks.Task ModuleLogsAsync(string api_version, string name, bool follow, string tail, int? since);

        /// <summary>List identities.</summary>
        /// <param name="api_version">The version of the API.</param>
        /// <returns>Ok</returns>
        System.Threading.Tasks.Task<IdentityList> ListIdentitiesAsync(string api_version);

        /// <summary>Create an identity.</summary>
        /// <param name="api_version">The version of the API.</param>
        /// <returns>Created</returns>
        System.Threading.Tasks.Task<Identity> CreateIdentityAsync(string api_version, IdentitySpec identity);

        /// <summary>Update an identity.</summary>
        /// <param name="api_version">The version of the API.</param>
        /// <param name="name">The name of the identity to update. (urlencoded)</param>
        /// <returns>Updated</returns>
        System.Threading.Tasks.Task<Identity> UpdateIdentityAsync(string api_version, string name, UpdateIdentity updateinfo);

        /// <summary>Delete an identity.</summary>
        /// <param name="api_version">The version of the API.</param>
        /// <param name="name">The name of the identity to delete. (urlencoded)</param>
        /// <returns>Ok</returns>
        System.Threading.Tasks.Task DeleteIdentityAsync(string api_version, string name);

        /// <summary>Return host system information.</summary>
        /// <param name="api_version">The version of the API.</param>
        /// <returns>Ok</returns>
        System.Threading.Tasks.Task<SystemInfo> GetSystemInfoAsync(string api_version);

        /// <summary>Trigger a device reprovisioning flow.</summary>
        /// <param name="api_version">The version of the API.</param>
        /// <returns>Ok</returns>
        System.Threading.Tasks.Task ReprovisionDeviceAsync(string api_version);

    }

    [System.CodeDom.Compiler.GeneratedCode("NSwag", "13.1.3.0 (NJsonSchema v10.0.27.0 (Newtonsoft.Json v11.0.0.0))")]
    public partial class Controller : Microsoft.AspNetCore.Mvc.Controller
    {
        private IController _implementation;

        public Controller(IController implementation)
        {
            _implementation = implementation;
        }

        /// <summary>List modules.</summary>
        /// <param name="api_version">The version of the API.</param>
        /// <returns>Ok</returns>
        [Microsoft.AspNetCore.Mvc.HttpGet, Microsoft.AspNetCore.Mvc.Route("modules")]
        public System.Threading.Tasks.Task<ModuleList> ListModules([Microsoft.AspNetCore.Mvc.FromQuery(Name = "api-version")] string api_version)
        {
            return _implementation.ListModulesAsync(api_version);
        }

        /// <summary>Create module.</summary>
        /// <param name="api_version">The version of the API.</param>
        /// <returns>Created</returns>
        [Microsoft.AspNetCore.Mvc.HttpPost, Microsoft.AspNetCore.Mvc.Route("modules")]
        public System.Threading.Tasks.Task<ModuleDetails> CreateModule([Microsoft.AspNetCore.Mvc.FromQuery(Name = "api-version")] string api_version, [Microsoft.AspNetCore.Mvc.FromBody] ModuleSpec module)
        {
            return _implementation.CreateModuleAsync(api_version, module);
        }

        /// <summary>Get a module's status.</summary>
        /// <param name="api_version">The version of the API.</param>
        /// <param name="name">The name of the module to get. (urlencoded)</param>
        /// <returns>Ok</returns>
        [Microsoft.AspNetCore.Mvc.HttpGet, Microsoft.AspNetCore.Mvc.Route("modules/{name}")]
        public System.Threading.Tasks.Task<ModuleDetails> GetModule([Microsoft.AspNetCore.Mvc.FromQuery(Name = "api-version")] string api_version, string name)
        {
            return _implementation.GetModuleAsync(api_version, name);
        }

        /// <summary>Update a module.</summary>
        /// <param name="api_version">The version of the API.</param>
        /// <param name="name">The name of the module to update. (urlencoded)</param>
        /// <param name="start">Flag indicating whether module should be started after updating.</param>
        /// <returns>Ok</returns>
        [Microsoft.AspNetCore.Mvc.HttpPut, Microsoft.AspNetCore.Mvc.Route("modules/{name}")]
        public System.Threading.Tasks.Task<ModuleDetails> UpdateModule([Microsoft.AspNetCore.Mvc.FromQuery(Name = "api-version")] string api_version, string name, [Microsoft.AspNetCore.Mvc.FromQuery] bool? start, [Microsoft.AspNetCore.Mvc.FromBody] ModuleSpec module)
        {
            return _implementation.UpdateModuleAsync(api_version, name, start ?? false, module);
        }

        /// <summary>Delete a module.</summary>
        /// <param name="api_version">The version of the API.</param>
        /// <param name="name">The name of the module to delete. (urlencoded)</param>
        /// <returns>No Content</returns>
        [Microsoft.AspNetCore.Mvc.HttpDelete, Microsoft.AspNetCore.Mvc.Route("modules/{name}")]
        public System.Threading.Tasks.Task DeleteModule([Microsoft.AspNetCore.Mvc.FromQuery(Name = "api-version")] string api_version, string name)
        {
            return _implementation.DeleteModuleAsync(api_version, name);
        }

        /// <summary>Prepare to update a module.</summary>
        /// <param name="api_version">The version of the API.</param>
        /// <param name="name">The name of the module to update. (urlencoded)</param>
        /// <returns>No Content</returns>
        [Microsoft.AspNetCore.Mvc.HttpPost, Microsoft.AspNetCore.Mvc.Route("modules/{name}/prepareupdate")]
        public System.Threading.Tasks.Task PrepareUpdateModule([Microsoft.AspNetCore.Mvc.FromQuery(Name = "api-version")] string api_version, string name, [Microsoft.AspNetCore.Mvc.FromBody] ModuleSpec module)
        {
            return _implementation.PrepareUpdateModuleAsync(api_version, name, module);
        }

        /// <summary>Start a module.</summary>
        /// <param name="api_version">The version of the API.</param>
        /// <param name="name">The name of the module to start. (urlencoded)</param>
        /// <returns>No Content</returns>
        [Microsoft.AspNetCore.Mvc.HttpPost, Microsoft.AspNetCore.Mvc.Route("modules/{name}/start")]
        public System.Threading.Tasks.Task StartModule([Microsoft.AspNetCore.Mvc.FromQuery(Name = "api-version")] string api_version, string name)
        {
            return _implementation.StartModuleAsync(api_version, name);
        }

        /// <summary>Stop a module.</summary>
        /// <param name="api_version">The version of the API.</param>
        /// <param name="name">The name of the module to stop. (urlencoded)</param>
        /// <returns>No Content</returns>
        [Microsoft.AspNetCore.Mvc.HttpPost, Microsoft.AspNetCore.Mvc.Route("modules/{name}/stop")]
        public System.Threading.Tasks.Task StopModule([Microsoft.AspNetCore.Mvc.FromQuery(Name = "api-version")] string api_version, string name)
        {
            return _implementation.StopModuleAsync(api_version, name);
        }

        /// <summary>Restart a module.</summary>
        /// <param name="api_version">The version of the API.</param>
        /// <param name="name">The name of the module to restart. (urlencoded)</param>
        /// <returns>No Content</returns>
        [Microsoft.AspNetCore.Mvc.HttpPost, Microsoft.AspNetCore.Mvc.Route("modules/{name}/restart")]
        public System.Threading.Tasks.Task RestartModule([Microsoft.AspNetCore.Mvc.FromQuery(Name = "api-version")] string api_version, string name)
        {
            return _implementation.RestartModuleAsync(api_version, name);
        }

        /// <summary>Get module logs.</summary>
        /// <param name="api_version">The version of the API.</param>
        /// <param name="name">The name of the module to obtain logs for. (urlencoded)</param>
        /// <param name="follow">Return the logs as a stream.</param>
        /// <param name="tail">Only return this number of lines from the end of the logs.</param>
        /// <param name="since">Only return logs since this time, as a UNIX timestamp.</param>
        /// <returns>Logs returned as a string in response body</returns>
        [Microsoft.AspNetCore.Mvc.HttpGet, Microsoft.AspNetCore.Mvc.Route("modules/{name}/logs")]
        public System.Threading.Tasks.Task ModuleLogs([Microsoft.AspNetCore.Mvc.FromQuery(Name = "api-version")] string api_version, string name, [Microsoft.AspNetCore.Mvc.FromQuery] bool? follow, [Microsoft.AspNetCore.Mvc.FromQuery] string tail, [Microsoft.AspNetCore.Mvc.FromQuery] int? since)
        {
            return _implementation.ModuleLogsAsync(api_version, name, follow ?? false, tail ?? "all", since);
        }

        /// <summary>List identities.</summary>
        /// <param name="api_version">The version of the API.</param>
        /// <returns>Ok</returns>
        [Microsoft.AspNetCore.Mvc.HttpGet, Microsoft.AspNetCore.Mvc.Route("identities/")]
        public System.Threading.Tasks.Task<IdentityList> ListIdentities([Microsoft.AspNetCore.Mvc.FromQuery(Name = "api-version")] string api_version)
        {
            return _implementation.ListIdentitiesAsync(api_version);
        }

        /// <summary>Create an identity.</summary>
        /// <param name="api_version">The version of the API.</param>
        /// <returns>Created</returns>
        [Microsoft.AspNetCore.Mvc.HttpPost, Microsoft.AspNetCore.Mvc.Route("identities/")]
        public System.Threading.Tasks.Task<Identity> CreateIdentity([Microsoft.AspNetCore.Mvc.FromQuery(Name = "api-version")] string api_version, [Microsoft.AspNetCore.Mvc.FromBody] IdentitySpec identity)
        {
            return _implementation.CreateIdentityAsync(api_version, identity);
        }

        /// <summary>Update an identity.</summary>
        /// <param name="api_version">The version of the API.</param>
        /// <param name="name">The name of the identity to update. (urlencoded)</param>
        /// <returns>Updated</returns>
        [Microsoft.AspNetCore.Mvc.HttpPut, Microsoft.AspNetCore.Mvc.Route("identities/{name}")]
        public System.Threading.Tasks.Task<Identity> UpdateIdentity([Microsoft.AspNetCore.Mvc.FromQuery(Name = "api-version")] string api_version, string name, [Microsoft.AspNetCore.Mvc.FromBody] UpdateIdentity updateinfo)
        {
            return _implementation.UpdateIdentityAsync(api_version, name, updateinfo);
        }

        /// <summary>Delete an identity.</summary>
        /// <param name="api_version">The version of the API.</param>
        /// <param name="name">The name of the identity to delete. (urlencoded)</param>
        /// <returns>Ok</returns>
        [Microsoft.AspNetCore.Mvc.HttpDelete, Microsoft.AspNetCore.Mvc.Route("identities/{name}")]
        public System.Threading.Tasks.Task DeleteIdentity([Microsoft.AspNetCore.Mvc.FromQuery(Name = "api-version")] string api_version, string name)
        {
            return _implementation.DeleteIdentityAsync(api_version, name);
        }

        /// <summary>Return host system information.</summary>
        /// <param name="api_version">The version of the API.</param>
        /// <returns>Ok</returns>
        [Microsoft.AspNetCore.Mvc.HttpGet, Microsoft.AspNetCore.Mvc.Route("systeminfo")]
        public System.Threading.Tasks.Task<SystemInfo> GetSystemInfo([Microsoft.AspNetCore.Mvc.FromQuery(Name = "api-version")] string api_version)
        {
            return _implementation.GetSystemInfoAsync(api_version);
        }

        /// <summary>Trigger a device reprovisioning flow.</summary>
        /// <param name="api_version">The version of the API.</param>
        /// <returns>Ok</returns>
        [Microsoft.AspNetCore.Mvc.HttpPost, Microsoft.AspNetCore.Mvc.Route("device/reprovision")]
        public System.Threading.Tasks.Task ReprovisionDevice([Microsoft.AspNetCore.Mvc.FromQuery(Name = "api-version")] string api_version)
        {
            return _implementation.ReprovisionDeviceAsync(api_version);
        }

    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.0.27.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class ModuleList
    {
        [Newtonsoft.Json.JsonProperty("modules", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        public System.Collections.Generic.List<ModuleDetails> Modules { get; set; } = new System.Collections.Generic.List<ModuleDetails>();


    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.0.27.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class ModuleDetails
    {
        /// <summary>System generated unique identitier.</summary>
        [Newtonsoft.Json.JsonProperty("id", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string Id { get; set; }

        /// <summary>The name of the module.</summary>
        [Newtonsoft.Json.JsonProperty("name", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string Name { get; set; }

        /// <summary>The type of a module.</summary>
        [Newtonsoft.Json.JsonProperty("type", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string Type { get; set; }

        [Newtonsoft.Json.JsonProperty("config", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        public Config Config { get; set; } = new Config();

        [Newtonsoft.Json.JsonProperty("status", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        public Status Status { get; set; } = new Status();


    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.0.27.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class ModuleSpec
    {
        /// <summary>The name of a the module.</summary>
        [Newtonsoft.Json.JsonProperty("name", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string Name { get; set; }

        [Newtonsoft.Json.JsonProperty("type", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string Type { get; set; }

        [Newtonsoft.Json.JsonProperty("imagePullPolicy", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public ModuleSpecImagePullPolicy? ImagePullPolicy { get; set; }

        [Newtonsoft.Json.JsonProperty("config", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        public Config Config { get; set; } = new Config();


    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.0.27.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class Config
    {
        [Newtonsoft.Json.JsonProperty("settings", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        public object Settings { get; set; } = new object();

        [Newtonsoft.Json.JsonProperty("env", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public System.Collections.Generic.List<EnvVar> Env { get; set; }


    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.0.27.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class Status
    {
        [Newtonsoft.Json.JsonProperty("startTime", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public System.DateTimeOffset? StartTime { get; set; }

        [Newtonsoft.Json.JsonProperty("exitStatus", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public ExitStatus ExitStatus { get; set; }

        [Newtonsoft.Json.JsonProperty("runtimeStatus", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        public RuntimeStatus RuntimeStatus { get; set; } = new RuntimeStatus();


    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.0.27.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class EnvVar
    {
        [Newtonsoft.Json.JsonProperty("key", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string Key { get; set; }

        [Newtonsoft.Json.JsonProperty("value", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string Value { get; set; }


    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.0.27.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class ExitStatus
    {
        [Newtonsoft.Json.JsonProperty("exitTime", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public System.DateTimeOffset ExitTime { get; set; }

        [Newtonsoft.Json.JsonProperty("statusCode", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string StatusCode { get; set; }


    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.0.27.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class RuntimeStatus
    {
        [Newtonsoft.Json.JsonProperty("status", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string Status { get; set; }

        [Newtonsoft.Json.JsonProperty("description", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Description { get; set; }


    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.0.27.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class SystemInfo
    {
        [Newtonsoft.Json.JsonProperty("osType", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string OsType { get; set; }

        [Newtonsoft.Json.JsonProperty("architecture", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string Architecture { get; set; }

        [Newtonsoft.Json.JsonProperty("version", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Version { get; set; }


    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.0.27.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class IdentityList
    {
        [Newtonsoft.Json.JsonProperty("identities", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        public System.Collections.Generic.List<Identity> Identities { get; set; } = new System.Collections.Generic.List<Identity>();


    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.0.27.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class IdentitySpec
    {
        [Newtonsoft.Json.JsonProperty("moduleId", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string ModuleId { get; set; }

        [Newtonsoft.Json.JsonProperty("managedBy", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string ManagedBy { get; set; }


    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.0.27.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class UpdateIdentity
    {
        [Newtonsoft.Json.JsonProperty("generationId", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string GenerationId { get; set; }

        [Newtonsoft.Json.JsonProperty("managedBy", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string ManagedBy { get; set; }


    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.0.27.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class Identity
    {
        [Newtonsoft.Json.JsonProperty("moduleId", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string ModuleId { get; set; }

        [Newtonsoft.Json.JsonProperty("managedBy", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string ManagedBy { get; set; }

        [Newtonsoft.Json.JsonProperty("generationId", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string GenerationId { get; set; }

        [Newtonsoft.Json.JsonProperty("authType", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public IdentityAuthType AuthType { get; set; }


    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.0.27.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class ErrorResponse
    {
        [Newtonsoft.Json.JsonProperty("message", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string Message { get; set; }


    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.0.27.0 (Newtonsoft.Json v11.0.0.0)")]
    public enum ModuleSpecImagePullPolicy
    {
        [System.Runtime.Serialization.EnumMember(Value = @"On-Create")]
        OnCreate = 0,

        [System.Runtime.Serialization.EnumMember(Value = @"Never")]
        Never = 1,

    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.0.27.0 (Newtonsoft.Json v11.0.0.0)")]
    public enum IdentityAuthType
    {
        [System.Runtime.Serialization.EnumMember(Value = @"None")]
        None = 0,

        [System.Runtime.Serialization.EnumMember(Value = @"Sas")]
        Sas = 1,

        [System.Runtime.Serialization.EnumMember(Value = @"X509")]
        X509 = 2,

    }

}
