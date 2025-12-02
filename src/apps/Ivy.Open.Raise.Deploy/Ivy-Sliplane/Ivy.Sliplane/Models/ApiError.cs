using System.Text.Json.Serialization;

namespace Ivy.Sliplane.Models
{
    public class ApiError
    {
        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
}
