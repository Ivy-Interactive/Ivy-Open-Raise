namespace Ivy.Open.Raise.Deploy.Apps;

[App(icon: Icons.Rocket, title: "Deploy Open-Raise")]
public class DeployApp : ViewBase
{
    public class DeploymentModel
    {
        public string ProjectName { get; set; } = "open-raise";
        public string ServerLocation { get; set; } = "Frankfurt";
        public string ServerType { get; set; } = "2 vCPU (Shared) 9€/month";
        public string LlmEndpoint { get; set; } = "";
        public string LlmApiKey { get; set; } = "";
        public string? EmailHost { get; set; }
        public string? EmailUser { get; set; }
        public string? EmailPassword { get; set; }
    }

    public override object? Build()
    {
        // Single model state
        var deployment = UseState(() => new DeploymentModel());

        // Individual field states for form binding
        var projectName = UseState(() => deployment.Value.ProjectName);
        var serverLocation = UseState(() => deployment.Value.ServerLocation);
        var serverType = UseState(() => deployment.Value.ServerType);
        var llmEndpoint = UseState(() => deployment.Value.LlmEndpoint);
        var llmApiKey = UseState(() => deployment.Value.LlmApiKey);
        var emailHost = UseState(() => deployment.Value.EmailHost ?? "");
        var emailUser = UseState(() => deployment.Value.EmailUser ?? "");
        var emailPassword = UseState(() => deployment.Value.EmailPassword ?? "");

        // Sync individual states with model
        UseEffect(() =>
        {
            deployment.Set(new DeploymentModel
            {
                ProjectName = projectName.Value,
                ServerLocation = serverLocation.Value,
                ServerType = serverType.Value,
                LlmEndpoint = llmEndpoint.Value,
                LlmApiKey = llmApiKey.Value,
                EmailHost = string.IsNullOrEmpty(emailHost.Value) ? null : emailHost.Value,
                EmailUser = string.IsNullOrEmpty(emailUser.Value) ? null : emailUser.Value,
                EmailPassword = string.IsNullOrEmpty(emailPassword.Value) ? null : emailPassword.Value
            });
        }, projectName, serverLocation, serverType, llmEndpoint, llmApiKey, emailHost, emailUser, emailPassword);

        // Form validation state
        var formErrors = UseState<Dictionary<string, string>>(() => []);
        var isSubmitting = UseState(false);

        // Validate fields when they change (for real-time validation)
        UseEffect(() => ValidateField("projectName"), projectName);
        UseEffect(() => ValidateField("serverLocation"), serverLocation);
        UseEffect(() => ValidateField("serverType"), serverType);
        UseEffect(() => ValidateField("llmEndpoint"), llmEndpoint);
        UseEffect(() => ValidateField("llmApiKey"), llmApiKey);
        UseEffect(() => ValidateField("emailPassword"), emailPassword);

        var client = UseService<IClientProvider>();

        // Validation function
        bool ValidateForm()
        {
            var errors = new Dictionary<string, string>();

            // Project name validation
            if (string.IsNullOrWhiteSpace(deployment.Value.ProjectName))
                errors["projectName"] = "Please enter a project name";
            else if (deployment.Value.ProjectName.Length < 3)
                errors["projectName"] = "Project name must be at least 3 characters long";
            else if (deployment.Value.ProjectName.Length > 50)
                errors["projectName"] = "Project name cannot be longer than 50 characters";
            else if (!System.Text.RegularExpressions.Regex.IsMatch(deployment.Value.ProjectName, @"^[a-zA-Z0-9-_]+$"))
                errors["projectName"] = "Project name can only contain letters, numbers, hyphens (-), and underscores (_)";

            // Server location validation
            if (string.IsNullOrWhiteSpace(deployment.Value.ServerLocation))
                errors["serverLocation"] = "Please select a server location";

            // Server type validation
            if (string.IsNullOrWhiteSpace(deployment.Value.ServerType))
                errors["serverType"] = "Please select a server type";

            // LLM endpoint validation
            if (string.IsNullOrWhiteSpace(deployment.Value.LlmEndpoint))
                errors["llmEndpoint"] = "Please enter your LLM endpoint URL";
            else if (!Uri.TryCreate(deployment.Value.LlmEndpoint, UriKind.Absolute, out _))
                errors["llmEndpoint"] = "Please enter a valid URL (e.g., https://api.openai.com/v1)";

            // LLM API key validation
            if (string.IsNullOrWhiteSpace(deployment.Value.LlmApiKey))
                errors["llmApiKey"] = "Please enter your API key";
            else if (deployment.Value.LlmApiKey.Length < 10)
                errors["llmApiKey"] = "API key must be at least 10 characters long";

            // Email password validation (if provided)
            if (!string.IsNullOrWhiteSpace(deployment.Value.EmailPassword) && deployment.Value.EmailPassword.Length < 6)
                errors["emailPassword"] = "Email password must be at least 6 characters long";

            formErrors.Set(errors);
            return errors.Count == 0;
        }

        // Validate individual field
        void ValidateField(string fieldName)
        {
            var errors = new Dictionary<string, string>(formErrors.Value);
            errors.Remove(fieldName); // Remove existing error for this field

            switch (fieldName)
            {
                case "projectName":
                    if (string.IsNullOrWhiteSpace(deployment.Value.ProjectName))
                        errors["projectName"] = "Please enter a project name";
                    else if (deployment.Value.ProjectName.Length < 3)
                        errors["projectName"] = "Project name must be at least 3 characters long";
                    else if (deployment.Value.ProjectName.Length > 50)
                        errors["projectName"] = "Project name cannot be longer than 50 characters";
                    else if (!System.Text.RegularExpressions.Regex.IsMatch(deployment.Value.ProjectName, @"^[a-zA-Z0-9-_]+$"))
                        errors["projectName"] = "Project name can only contain letters, numbers, hyphens (-), and underscores (_)";
                    break;

                case "serverLocation":
                    if (string.IsNullOrWhiteSpace(deployment.Value.ServerLocation))
                        errors["serverLocation"] = "Please select a server location";
                    break;

                case "serverType":
                    if (string.IsNullOrWhiteSpace(deployment.Value.ServerType))
                        errors["serverType"] = "Please select a server type";
                    break;

                case "llmEndpoint":
                    if (string.IsNullOrWhiteSpace(deployment.Value.LlmEndpoint))
                        errors["llmEndpoint"] = "Please enter your LLM endpoint URL";
                    else if (!Uri.TryCreate(deployment.Value.LlmEndpoint, UriKind.Absolute, out _))
                        errors["llmEndpoint"] = "Please enter a valid URL (e.g., https://api.openai.com/v1)";
                    break;

                case "llmApiKey":
                    if (string.IsNullOrWhiteSpace(deployment.Value.LlmApiKey))
                        errors["llmApiKey"] = "Please enter your API key";
                    else if (deployment.Value.LlmApiKey.Length < 10)
                        errors["llmApiKey"] = "API key must be at least 10 characters long";
                    break;

                case "emailPassword":
                    if (!string.IsNullOrWhiteSpace(deployment.Value.EmailPassword) && deployment.Value.EmailPassword.Length < 6)
                        errors["emailPassword"] = "Email password must be at least 6 characters long";
                    break;
            }

            formErrors.Set(errors);
        }

        // Handle form submission
        void HandleSubmit()
        {
            if (isSubmitting.Value) return;

            if (!ValidateForm())
            {
                client.Toast("Please fix the validation errors before submitting.");
                return;
            }

            isSubmitting.Set(true);

            try
            {
                // Simulate form submission
                client.Toast($"Deployment created for {projectName.Value}!");

                // Reset form
                projectName.Set("open-raise");
                serverLocation.Set("Frankfurt");
                serverType.Set("2 vCPU (Shared) 9€/month");
                llmEndpoint.Set("");
                llmApiKey.Set("");
                emailHost.Set("");
                emailUser.Set("");
                emailPassword.Set("");
                formErrors.Set(new Dictionary<string, string>());
            }
            finally
            {
                isSubmitting.Set(false);
            }
        }

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

        return Layout.Vertical()
            | new Card(
                Layout.Vertical()
                | Text.H2("Deploy Open-Raise To Sliplane")
                | Text.Block("Configure your new deployment")

                // Project Details Section
                | Text.H3("Project Details")
                | new Field(
                    projectName.ToTextInput()
                        .Placeholder("Enter project name")
                        .Invalid(formErrors.Value.ContainsKey("projectName") ? formErrors.Value["projectName"] : null)
                )
                .Label("Project Name")
                .Required()

                | new Field(
                    serverLocation.ToSelectInput(locationOptions)
                        .Placeholder("Select server location")
                        .Invalid(formErrors.Value.ContainsKey("serverLocation") ? formErrors.Value["serverLocation"] : null)
                )
                .Label("Server Location")
                .Required()

                | new Field(
                    serverType.ToSelectInput(serverOptions)
                        .Placeholder("Select server type")
                        .Invalid(formErrors.Value.ContainsKey("serverType") ? formErrors.Value["serverType"] : null)
                )
                .Label("Server")
                .Required()

                | Text.Small("Minimum 3 vCPU is recommended for this deployment.")
                    .Color(Colors.Orange)

                // Services Section
                | new Separator()
                | Text.H3("Services Included in This Deployment")
                | (Layout.Horizontal().Gap(8).Align(Align.Center)
                    | (Layout.Vertical().Align(Align.Center)
                        | new Icon(Icons.Container, Colors.Blue)
                        | Text.Small("Open-Raise Container"))
                    | (Layout.Vertical().Align(Align.Center)
                        | new Icon(Icons.Database, Colors.Gray)
                        | Text.Small("Postgres Database"))
                    | (Layout.Vertical().Align(Align.Center)
                        | new Icon(Icons.Circle, Colors.Red)
                        | Text.Small("Redis")))

                // LLM Provider Section 
                | new Separator()
                | Text.H3("Settings")
                | Text.H4("LLM Provider")
                | Text.Block("Use an OpenAI compatible endpoint.")

                | new Field(
                    llmEndpoint.ToTextInput()
                        .Placeholder("https://api.openai.com/v1")
                        .Invalid(formErrors.Value.ContainsKey("llmEndpoint") ? formErrors.Value["llmEndpoint"] : null)
                )
                .Label("Endpoint")
                .Required()

                | new Field(
                    llmApiKey.ToPasswordInput()
                        .Placeholder("Enter your API key")
                        .Invalid(formErrors.Value.ContainsKey("llmApiKey") ? formErrors.Value["llmApiKey"] : null)
                )
                .Label("API Key")
                .Required()

                // Email Provider Section
                | Text.H4("Email Provider (Optional)")
                | Text.Block("How can we send email. Must be SMTP compatible. We recommend Resend.")

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
                        .Invalid(formErrors.Value.ContainsKey("emailPassword") ? formErrors.Value["emailPassword"] : null)
                )
                .Label("Password")

                // Submit Button
                | new Separator()
                | new Button("Create Account >", HandleSubmit)
                    .Variant(ButtonVariant.Primary)
                    .Large()
                    .Disabled(isSubmitting.Value)
            )
            .Width(Size.Units(140).Max(600));
    }
}
