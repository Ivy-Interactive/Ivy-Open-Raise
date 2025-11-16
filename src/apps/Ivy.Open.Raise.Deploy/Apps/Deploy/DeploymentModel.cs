namespace Ivy.Open.Raise.Deploy.Apps.Deploy;

public class DeploymentModel
{
    public string ProjectName { get; set; } = "";
    public string ServerLocation { get; set; } = "";
    public string ServerType { get; set; } = "";
    public string LlmEndpoint { get; set; } = "";
    public string LlmApiKey { get; set; } = "";
    public string? EmailHost { get; set; }
    public string? EmailUser { get; set; }
    public string? EmailPassword { get; set; }
}

