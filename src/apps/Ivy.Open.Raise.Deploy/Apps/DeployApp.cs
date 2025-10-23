using System.ComponentModel.DataAnnotations;

namespace Ivy.Open.Raise.Deploy.Apps;

[App(icon: Icons.Rocket, title: "Deploy Open-Raise")]
public class DeployApp : ViewBase
{
    public class DeploymentModel
    {
        [Required(ErrorMessage = "Project name is required")]
        [MinLength(3, ErrorMessage = "Project name must be at least 3 characters")]
        [MaxLength(50, ErrorMessage = "Project name cannot exceed 50 characters")]
        [RegularExpression(@"^[a-zA-Z0-9-_]+$", ErrorMessage = "Project name can only contain letters, numbers, hyphens, and underscores")]
        public string ProjectName { get; set; } = "";

        [Required(ErrorMessage = "Server location is required")]
        public string ServerLocation { get; set; } = "";

        [Required(ErrorMessage = "Server type is required")]
        public string ServerType { get; set; } = "";

        [Required(ErrorMessage = "LLM endpoint is required")]
        [Url(ErrorMessage = "LLM endpoint must be a valid URL")]
        public string LlmEndpoint { get; set; } = "";

        [Required(ErrorMessage = "LLM API key is required")]
        [MinLength(10, ErrorMessage = "API key must be at least 10 characters")]
        public string LlmApiKey { get; set; } = "";

        public string? EmailHost { get; set; }

        public string? EmailUser { get; set; }

        [MinLength(6, ErrorMessage = "Email password must be at least 6 characters")]
        public string? EmailPassword { get; set; }
    }

    public override object? Build()
    {
        var deployment = UseState(() => new DeploymentModel
        {
            ProjectName = "open-raise",
            ServerLocation = "Frankfurt",
            ServerType = "2 vCPU (Shared) 9€/month",
            LlmEndpoint = "",
            LlmApiKey = "",
            EmailHost = null,
            EmailUser = null,
            EmailPassword = null
        });

        var client = UseService<IClientProvider>();

        // Handle form submission
        UseEffect(() =>
        {
            if (!string.IsNullOrEmpty(deployment.Value.LlmEndpoint) &&
                !string.IsNullOrEmpty(deployment.Value.LlmApiKey))
            {
                client.Toast($"Deployment created for {deployment.Value.ProjectName}!");
            }
        }, deployment);

        // Server location options
        var locationOptions = new[]
        {
            "Frankfurt",
            "London",
            "New York",
            "Singapore",
            "Tokyo"
        }.ToOptions();

        // Server type options
        var serverOptions = new[]
        {
            "2 vCPU (Shared) 9€/month",
            "3 vCPU (Shared) 12€/month",
            "4 vCPU (Dedicated) 18€/month",
            "8 vCPU (Dedicated) 35€/month"
        }.ToOptions();

        return Layout.Vertical().Gap(4).Padding(4)
            | new Card(
                Layout.Vertical().Gap(8).Padding(4)
                | Text.H2("Deploy Open-Raise To Sliplane")
                | Text.Block("Configure your new deployment")
                | deployment.ToForm("Create Account")
                    .Group("Project Details", m => m.ProjectName, m => m.ServerLocation, m => m.ServerType)
                    .Group("LLM Provider", m => m.LlmEndpoint, m => m.LlmApiKey)
                    .Group("Email Provider (Optional)", m => m.EmailHost, m => m.EmailUser, m => m.EmailPassword)
                    .Builder(m => m.ServerLocation, s => s.ToSelectInput(locationOptions))
                    .Builder(m => m.ServerType, s => s.ToSelectInput(serverOptions))
                    .Builder(m => m.LlmApiKey, s => s.ToPasswordInput())
                    .Builder(m => m.EmailPassword, s => s.ToPasswordInput())
                    .Label(m => m.ProjectName, "Project Name")
                    .Label(m => m.ServerLocation, "Server Location")
                    .Label(m => m.ServerType, "Server")
                    .Label(m => m.LlmEndpoint, "Endpoint")
                    .Label(m => m.LlmApiKey, "API Key")
                    .Label(m => m.EmailHost, "Host")
                    .Label(m => m.EmailUser, "User")
                    .Label(m => m.EmailPassword, "Password")
                    // Required fields are automatically detected from DataAnnotations
            )
            .Width(Size.Units(120).Max(600));
    }
}
