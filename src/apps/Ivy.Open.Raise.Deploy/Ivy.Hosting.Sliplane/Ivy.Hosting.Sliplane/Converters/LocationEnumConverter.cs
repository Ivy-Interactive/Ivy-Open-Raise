using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ivy.Hosting.Sliplane.Models;

namespace Ivy.Hosting.Sliplane.Converters
{
    public class LocationEnumConverter : JsonConverter<LocationEnum>
    {
        public override LocationEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            return value switch
            {
                "sin" => LocationEnum.Singapore,
                "fsn" => LocationEnum.Falkenstein,
                "nbg" => LocationEnum.Nuremberg,
                "ash" => LocationEnum.Ashburn,
                "hel" => LocationEnum.Helsinki,
                "hil" => LocationEnum.Hillsboro,
                _ => throw new JsonException($"Unknown location: {value}")
            };
        }

        public override void Write(Utf8JsonWriter writer, LocationEnum value, JsonSerializerOptions options)
        {
            var stringValue = value switch
            {
                LocationEnum.Singapore => "sin",
                LocationEnum.Falkenstein => "fsn",
                LocationEnum.Nuremberg => "nbg",
                LocationEnum.Ashburn => "ash",
                LocationEnum.Helsinki => "hel",
                LocationEnum.Hillsboro => "hil",
                _ => throw new JsonException($"Unknown location enum value: {value}")
            };
            writer.WriteStringValue(stringValue);
        }
    }

    public class NullableLocationEnumConverter : JsonConverter<LocationEnum?>
    {
        public override LocationEnum? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            var value = reader.GetString();
            if (string.IsNullOrEmpty(value))
                return null;

            return value switch
            {
                "sin" => LocationEnum.Singapore,
                "fsn" => LocationEnum.Falkenstein,
                "nbg" => LocationEnum.Nuremberg,
                "ash" => LocationEnum.Ashburn,
                "hel" => LocationEnum.Helsinki,
                "hil" => LocationEnum.Hillsboro,
                _ => throw new JsonException($"Unknown location: {value}")
            };
        }

        public override void Write(Utf8JsonWriter writer, LocationEnum? value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            var stringValue = value switch
            {
                LocationEnum.Singapore => "sin",
                LocationEnum.Falkenstein => "fsn",
                LocationEnum.Nuremberg => "nbg",
                LocationEnum.Ashburn => "ash",
                LocationEnum.Helsinki => "hel",
                LocationEnum.Hillsboro => "hil",
                _ => throw new JsonException($"Unknown location enum value: {value}")
            };
            writer.WriteStringValue(stringValue);
        }
    }
}