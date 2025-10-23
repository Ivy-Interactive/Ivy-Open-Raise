using System.ComponentModel.DataAnnotations;

namespace Ivy.Open.Raise.Deploy.Apps;

[App(icon: Icons.Rocket, title: "Deploy Open-Raise")]
public class DeployApp : ViewBase
{
    public record DeploymentModel(
        string ProjectName,
        string ServerLocation,
        string ServerType,
        string LlmEndpoint,
        string LlmApiKey,
        string? EmailHost,
        string? EmailUser,
        string? EmailPassword
    );

    public override object? Build()
    {
        var deployment = UseState(() => new DeploymentModel(
            "open-raise",
            "Frankfurt",
            "2 vCPU (Shared) 9€/month",
            "",
            "",
            null,
            null,
            null
        ));

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

                // Single comprehensive form with grouping
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
                    .Required(m => m.ProjectName, m => m.ServerLocation, m => m.ServerType, m => m.LlmEndpoint, m => m.LlmApiKey)
            )
            .Width(Size.Units(120).Max(600));
    }
}
