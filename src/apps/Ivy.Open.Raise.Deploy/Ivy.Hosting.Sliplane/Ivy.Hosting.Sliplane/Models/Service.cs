using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Ivy.Hosting.Sliplane.Models
{
    public class Service
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("serverId")]
        public string? ServerId { get; set; }

        [JsonPropertyName("projectId")]
        public string? ProjectId { get; set; }

        [JsonPropertyName("status")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ServiceStatus Status { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("network")]
        public ServiceNetworkResponse? Network { get; set; }

        [JsonPropertyName("volumes")]
        public List<VolumeResponse>? Volumes { get; set; }

        [JsonPropertyName("env")]
        public List<EnvironmentVariable>? Env { get; set; }

        [JsonPropertyName("deployment")]
        public object? Deployment { get; set; }

        [JsonPropertyName("healthcheck")]
        public string? Healthcheck { get; set; }

        [JsonPropertyName("cmd")]
        public string? Cmd { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ServiceStatus
    {
        Pending,
        Live,
        Failed,
        Suspended
    }

    public class CreateServiceRequest
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("serverId")]
        public string? ServerId { get; set; }

        [JsonPropertyName("deployment")]
        public object? Deployment { get; set; }

        [JsonPropertyName("network")]
        public ServiceNetworkRequest? Network { get; set; }

        [JsonPropertyName("cmd")]
        public string? Cmd { get; set; }

        [JsonPropertyName("env")]
        public List<EnvironmentVariable>? Env { get; set; }

        [JsonPropertyName("healthcheck")]
        public string? Healthcheck { get; set; }

        [JsonPropertyName("volumes")]
        public List<VolumeRequest>? Volumes { get; set; }
    }

    public class UpdateServiceRequest
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("deployment")]
        public object? Deployment { get; set; }

        [JsonPropertyName("network")]
        public ServiceNetworkRequest? Network { get; set; }

        [JsonPropertyName("cmd")]
        public string? Cmd { get; set; }

        [JsonPropertyName("env")]
        public List<EnvironmentVariable>? Env { get; set; }

        [JsonPropertyName("healthcheck")]
        public string? Healthcheck { get; set; }

        [JsonPropertyName("volumes")]
        public List<VolumeRequest>? Volumes { get; set; }
    }


    public class ServiceNetworkRequest
    {
        [JsonPropertyName("public")]
        public bool Public { get; set; }

        [JsonPropertyName("protocol")]
        public string? Protocol { get; set; }
    }

    public class ServiceNetworkResponse
    {
        [JsonPropertyName("public")]
        public bool Public { get; set; }

        [JsonPropertyName("protocol")]
        public string? Protocol { get; set; }

        [JsonPropertyName("managedDomain")]
        public string? ManagedDomain { get; set; }

        [JsonPropertyName("internalDomain")]
        public string? InternalDomain { get; set; }

        [JsonPropertyName("customDomains")]
        public List<CustomDomain>? CustomDomains { get; set; }
    }

    public class EnvironmentVariable
    {
        [JsonPropertyName("key")]
        public string? Key { get; set; }

        [JsonPropertyName("value")]
        public string? Value { get; set; }

        [JsonPropertyName("secret")]
        public bool Secret { get; set; }
    }

    public class VolumeRequest
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("mountPath")]
        public string? MountPath { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    public class VolumeResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("mountPath")]
        public string? MountPath { get; set; }
    }

    public class CustomDomain
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("domain")]
        public string? Domain { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }
    }
}