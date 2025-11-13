using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ivy.Sliplane.Converters;

namespace Ivy.Sliplane.Models
{
    public class Server
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("status")]
        [JsonConverter(typeof(ServerStatusConverter))]
        public ServerStatus Status { get; set; }

        [JsonPropertyName("location")]
        [JsonConverter(typeof(NullableLocationEnumConverter))]
        public LocationEnum? Location { get; set; }

        [JsonPropertyName("instanceType")]
        [JsonConverter(typeof(NullableInstanceTypeEnumConverter))]
        public InstanceTypeEnum? InstanceType { get; set; }

        [JsonPropertyName("ipv4")]
        public string? IPv4 { get; set; }

        [JsonPropertyName("ipv6")]
        public string? IPv6 { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
    }

    public enum ServerStatus
    {
        Booting,
        Running,
        Error,
        Rescaling,
        Deleting
    }

    public enum InstanceTypeEnum
    {
        Base,
        Medium,
        Large,
        XLarge,
        XXLarge,
        DedicatedBase,
        DedicatedMedium,
        DedicatedLarge,
        DedicatedXLarge,
        DedicatedXXLarge,
        DedicatedXXXLarge
    }

    public class CreateServerRequest
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("instanceType")]
        [JsonConverter(typeof(NullableInstanceTypeEnumConverter))]
        public InstanceTypeEnum? InstanceType { get; set; }

        [JsonPropertyName("location")]
        [JsonConverter(typeof(NullableLocationEnumConverter))]
        public LocationEnum? Location { get; set; }
    }

    public class RescaleServerRequest
    {
        [JsonPropertyName("instanceType")]
        [JsonConverter(typeof(NullableInstanceTypeEnumConverter))]
        public InstanceTypeEnum? InstanceType { get; set; }
    }

    public enum LocationEnum
    {
        Singapore,
        Falkenstein,
        Nuremberg,
        Ashburn,
        Helsinki,
        Hillsboro
    }

    // This class is kept for future use if detailed instance type info is needed
    public class InstanceType
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("cpu")]
        public int Cpu { get; set; }

        [JsonPropertyName("memory")]
        public int Memory { get; set; }

        [JsonPropertyName("disk")]
        public int Disk { get; set; }

        [JsonPropertyName("pricePerMonth")]
        public decimal PricePerMonth { get; set; }
    }

    // This class is kept for future use if detailed location info is needed
    public class Location
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("region")]
        public string? Region { get; set; }
    }
}
