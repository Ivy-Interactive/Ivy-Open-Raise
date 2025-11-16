using System.Text.Json.Serialization;
using System;

namespace Ivy.Sliplane.Models
{
    public class RepositoryDeployment
    {
        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("autoDeploy")]
        public bool AutoDeploy { get; set; } = true;

        [JsonPropertyName("branch")]
        public string? Branch { get; set; } = "main";

        [JsonPropertyName("dockerContext")]
        public string? DockerContext { get; set; } = ".";

        [JsonPropertyName("dockerfilePath")]
        public string? DockerfilePath { get; set; } = "Dockerfile";
    }

    public class ImageDeployment
    {
        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("registryAuthenticationId")]
        public string? RegistryAuthenticationId { get; set; }
    }

    public class DeployServiceRequest
    {
        [JsonPropertyName("tag")]
        public string? Tag { get; set; }
    }

    public class DeploymentResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("serviceId")]
        public string? ServiceId { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("branch")]
        public string? Branch { get; set; }

        [JsonPropertyName("commitSha")]
        public string? CommitSha { get; set; }
    }
}
