using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ivy.Hosting.Sliplane.Models;

namespace Ivy.Hosting.Sliplane.Converters
{
    public class InstanceTypeEnumConverter : JsonConverter<InstanceTypeEnum>
    {
        public override InstanceTypeEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            return value switch
            {
                "base" => InstanceTypeEnum.Base,
                "medium" => InstanceTypeEnum.Medium,
                "large" => InstanceTypeEnum.Large,
                "x-large" => InstanceTypeEnum.XLarge,
                "xx-large" => InstanceTypeEnum.XXLarge,
                "dedicated-base" => InstanceTypeEnum.DedicatedBase,
                "dedicated-medium" => InstanceTypeEnum.DedicatedMedium,
                "dedicated-large" => InstanceTypeEnum.DedicatedLarge,
                "dedicated-x-large" => InstanceTypeEnum.DedicatedXLarge,
                "dedicated-xx-large" => InstanceTypeEnum.DedicatedXXLarge,
                "dedicated-xxx-large" => InstanceTypeEnum.DedicatedXXXLarge,
                _ => throw new JsonException($"Unknown instance type: {value}")
            };
        }

        public override void Write(Utf8JsonWriter writer, InstanceTypeEnum value, JsonSerializerOptions options)
        {
            var stringValue = value switch
            {
                InstanceTypeEnum.Base => "base",
                InstanceTypeEnum.Medium => "medium",
                InstanceTypeEnum.Large => "large",
                InstanceTypeEnum.XLarge => "x-large",
                InstanceTypeEnum.XXLarge => "xx-large",
                InstanceTypeEnum.DedicatedBase => "dedicated-base",
                InstanceTypeEnum.DedicatedMedium => "dedicated-medium",
                InstanceTypeEnum.DedicatedLarge => "dedicated-large",
                InstanceTypeEnum.DedicatedXLarge => "dedicated-x-large",
                InstanceTypeEnum.DedicatedXXLarge => "dedicated-xx-large",
                InstanceTypeEnum.DedicatedXXXLarge => "dedicated-xxx-large",
                _ => throw new JsonException($"Unknown instance type enum value: {value}")
            };
            writer.WriteStringValue(stringValue);
        }
    }

    public class NullableInstanceTypeEnumConverter : JsonConverter<InstanceTypeEnum?>
    {
        public override InstanceTypeEnum? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            var value = reader.GetString();
            if (string.IsNullOrEmpty(value))
                return null;

            return value switch
            {
                "base" => InstanceTypeEnum.Base,
                "medium" => InstanceTypeEnum.Medium,
                "large" => InstanceTypeEnum.Large,
                "x-large" => InstanceTypeEnum.XLarge,
                "xx-large" => InstanceTypeEnum.XXLarge,
                "dedicated-base" => InstanceTypeEnum.DedicatedBase,
                "dedicated-medium" => InstanceTypeEnum.DedicatedMedium,
                "dedicated-large" => InstanceTypeEnum.DedicatedLarge,
                "dedicated-x-large" => InstanceTypeEnum.DedicatedXLarge,
                "dedicated-xx-large" => InstanceTypeEnum.DedicatedXXLarge,
                "dedicated-xxx-large" => InstanceTypeEnum.DedicatedXXXLarge,
                _ => throw new JsonException($"Unknown instance type: {value}")
            };
        }

        public override void Write(Utf8JsonWriter writer, InstanceTypeEnum? value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            var stringValue = value switch
            {
                InstanceTypeEnum.Base => "base",
                InstanceTypeEnum.Medium => "medium",
                InstanceTypeEnum.Large => "large",
                InstanceTypeEnum.XLarge => "x-large",
                InstanceTypeEnum.XXLarge => "xx-large",
                InstanceTypeEnum.DedicatedBase => "dedicated-base",
                InstanceTypeEnum.DedicatedMedium => "dedicated-medium",
                InstanceTypeEnum.DedicatedLarge => "dedicated-large",
                InstanceTypeEnum.DedicatedXLarge => "dedicated-x-large",
                InstanceTypeEnum.DedicatedXXLarge => "dedicated-xx-large",
                InstanceTypeEnum.DedicatedXXXLarge => "dedicated-xxx-large",
                _ => throw new JsonException($"Unknown instance type enum value: {value}")
            };
            writer.WriteStringValue(stringValue);
        }
    }
}