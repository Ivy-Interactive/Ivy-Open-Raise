using System.Text.Json.Serialization;

namespace Ivy.Sliplane.Models
{
    public class Project
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    public class CreateProjectRequest
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    public class UpdateProjectRequest
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}
