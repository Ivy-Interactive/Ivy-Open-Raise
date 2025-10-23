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
        // Individual state variables for each field
        var projectName = UseState("open-raise");
        var serverLocation = UseState("Frankfurt");
        var serverType = UseState("2 vCPU (Shared) 9€/month");
        var llmEndpoint = UseState("");
        var llmApiKey = UseState("");
        var emailHost = UseState("");
        var emailUser = UseState("");
        var emailPassword = UseState("");

        var client = UseService<IClientProvider>();

        // Handle form submission
        UseEffect(() =>
        {
            if (!string.IsNullOrEmpty(llmEndpoint.Value) &&
                !string.IsNullOrEmpty(llmApiKey.Value))
            {
                client.Toast($"Deployment created for {projectName.Value}!");
            }
        }, llmEndpoint, llmApiKey);

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

        return Layout.Vertical().Gap(6).Padding(4)
            | new Card(
                Layout.Vertical().Gap(8).Padding(6)
                | Text.H2("Deploy Open-Raise To Sliplane")
                | Text.Markdown("*Configure your new deployment*")

                // Project Details Section
                | Text.H3("Project Details")
                | new Field(
                    projectName.ToTextInput()
                        .Placeholder("Enter project name")
                )
                .Label("Project Name")
                .Required()

                | new Field(
                    serverLocation.ToSelectInput(locationOptions)
                        .Placeholder("Select server location")
                )
                .Label("Server Location")
                .Required()

                | new Field(
                    serverType.ToSelectInput(serverOptions)
                        .Placeholder("Select server type")
                )
                .Label("Server")
                .Required()

                | Text.Small("Minimum 3 vCPU is recommended for this deployment.")
                    .Color(Colors.Orange)

                // Services Section
                | new Separator()
                | Text.H3("Services Included in This Deployment")
                | Layout.Horizontal().Gap(8).Align(Align.Center)
                    | Layout.Vertical().Align(Align.Center)
                        | new Icon(Icons.Container, Colors.Blue)
                        | Text.Small("Open-Raise Container")
                    | Layout.Vertical().Align(Align.Center)
                        | new Icon(Icons.Database, Colors.Gray)
                        | Text.Small("Postgres Database")
                    | Layout.Vertical().Align(Align.Center)
                        | new Icon(Icons.Circle, Colors.Red)
                        | Text.Small("Redis")

                // LLM Provider Section
                | new Separator()
                | Text.H3("Settings")
                | Text.H4("LLM Provider")
                | Text.Markdown("*Use an OpenAI compatible endpoint.*")

                | new Field(
                    llmEndpoint.ToTextInput()
                        .Placeholder("https://api.openai.com/v1")
                )
                .Label("Endpoint")
                .Required()

                | new Field(
                    llmApiKey.ToPasswordInput()
                        .Placeholder("Enter your API key")
                )
                .Label("API Key")
                .Required()

                | new Button("Validate", _ => client.Toast("LLM endpoint validated!"))
                    .Variant(ButtonVariant.Link)

                // Email Provider Section
                | Text.H4("Email Provider (Optional)")
                | Text.Markdown("*How can we send email. Must be SMTP compatible. We recommend [Resend](https://resend.com).*")

                | new Field(
                    emailHost.ToTextInput()
                        .Placeholder("smtp.resend.com")
                )
                .Label("Host")

                | new Field(
                    emailUser.ToTextInput()
                        .Placeholder("user@example.com")
                )
                .Label("User")

                | new Field(
                    emailPassword.ToPasswordInput()
                        .Placeholder("Enter email password")
                )
                .Label("Password")

                | new Button("Validate", _ => client.Toast("Email provider validated!"))
                    .Variant(ButtonVariant.Link)

                // Submit Button
                | new Separator()
                | new Button("Create Account >", _ =>
                {
                    // Form submission is handled by UseEffect
                    client.Toast("Deployment created successfully!");
                })
                .Variant(ButtonVariant.Primary)
                .Large()
            )
            .Width(Size.Units(120).Max(600));
    }
}
