using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;

namespace Ivy.Sliplane.Models
{
    public class ServiceLog
    {
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("level")]
        public string? Level { get; set; }
    }

    public class ServiceLogsResponse
    {
        [JsonPropertyName("logs")]
        public List<ServiceLog>? Logs { get; set; }
    }
}
