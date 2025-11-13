using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ivy.Hosting.Sliplane.Models;

namespace Ivy.Hosting.Sliplane.Converters
{
    public class ServerStatusConverter : JsonConverter<ServerStatus>
    {
        public override ServerStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            return value?.ToLowerInvariant() switch
            {
                "booting" => ServerStatus.Booting,
                "running" => ServerStatus.Running,
                "error" => ServerStatus.Error,
                "rescaling" => ServerStatus.Rescaling,
                "deleting" => ServerStatus.Deleting,
                _ => throw new JsonException($"Unknown server status: {value}")
            };
        }

        public override void Write(Utf8JsonWriter writer, ServerStatus value, JsonSerializerOptions options)
        {
            var stringValue = value switch
            {
                ServerStatus.Booting => "booting",
                ServerStatus.Running => "running",
                ServerStatus.Error => "error",
                ServerStatus.Rescaling => "rescaling",
                ServerStatus.Deleting => "deleting",
                _ => throw new JsonException($"Unknown server status enum value: {value}")
            };
            writer.WriteStringValue(stringValue);
        }
    }
}