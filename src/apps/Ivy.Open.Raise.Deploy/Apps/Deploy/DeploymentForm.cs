using Ivy.Open.Raise.Deploy.Apps.Deploy;

namespace Ivy.Open.Raise.Deploy.Apps.Deploy;

public class DeploymentForm : ViewBase
{
    private readonly IState<DeploymentModel> _deployment;
    private readonly IState<Dictionary<string, string>> _formErrors;
    private readonly IState<bool> _isSubmitting;
    private readonly Action _onSubmit;

    public DeploymentForm(
        IState<DeploymentModel> deployment,
        IState<Dictionary<string, string>> formErrors,
        IState<bool> isSubmitting,
        Action onSubmit)
    {
        _deployment = deployment;
        _formErrors = formErrors;
        _isSubmitting = isSubmitting;
        _onSubmit = onSubmit;
    }

    public override object? Build()
    {
        // Individual field states for form binding
        var projectName = UseState(() => _deployment.Value.ProjectName);
        var serverLocation = UseState(() => _deployment.Value.ServerLocation);
        var serverType = UseState(() => _deployment.Value.ServerType);
        var llmEndpoint = UseState(() => _deployment.Value.LlmEndpoint);
        var llmApiKey = UseState(() => _deployment.Value.LlmApiKey);
        var emailHost = UseState(() => _deployment.Value.EmailHost ?? "");
        var emailUser = UseState(() => _deployment.Value.EmailUser ?? "");
        var emailPassword = UseState(() => _deployment.Value.EmailPassword ?? "");

        // Sync individual states with model
        UseEffect(() =>
        {
            _deployment.Set(new DeploymentModel
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

        // Validate fields when they change (for real-time validation)
        UseEffect(() =>
        {
            var error = DeploymentValidator.ValidateField("projectName", _deployment.Value);
            if (error != null)
            {
                var errors = new Dictionary<string, string>(_formErrors.Value) { ["projectName"] = error };
                _formErrors.Set(errors);
            }
            else
            {
                var errors = new Dictionary<string, string>(_formErrors.Value);
                errors.Remove("projectName");
                _formErrors.Set(errors);
            }
        }, projectName);

        UseEffect(() =>
        {
            var error = DeploymentValidator.ValidateField("serverLocation", _deployment.Value);
            if (error != null)
            {
                var errors = new Dictionary<string, string>(_formErrors.Value) { ["serverLocation"] = error };
                _formErrors.Set(errors);
            }
            else
            {
                var errors = new Dictionary<string, string>(_formErrors.Value);
                errors.Remove("serverLocation");
                _formErrors.Set(errors);
            }
        }, serverLocation);

        UseEffect(() =>
        {
            var error = DeploymentValidator.ValidateField("serverType", _deployment.Value);
            if (error != null)
            {
                var errors = new Dictionary<string, string>(_formErrors.Value) { ["serverType"] = error };
                _formErrors.Set(errors);
            }
            else
            {
                var errors = new Dictionary<string, string>(_formErrors.Value);
                errors.Remove("serverType");
                _formErrors.Set(errors);
            }
        }, serverType);

        UseEffect(() =>
        {
            var error = DeploymentValidator.ValidateField("llmEndpoint", _deployment.Value);
            if (error != null)
            {
                var errors = new Dictionary<string, string>(_formErrors.Value) { ["llmEndpoint"] = error };
                _formErrors.Set(errors);
            }
            else
            {
                var errors = new Dictionary<string, string>(_formErrors.Value);
                errors.Remove("llmEndpoint");
                _formErrors.Set(errors);
            }
        }, llmEndpoint);

        UseEffect(() =>
        {
            var error = DeploymentValidator.ValidateField("llmApiKey", _deployment.Value);
            if (error != null)
            {
                var errors = new Dictionary<string, string>(_formErrors.Value) { ["llmApiKey"] = error };
                _formErrors.Set(errors);
            }
            else
            {
                var errors = new Dictionary<string, string>(_formErrors.Value);
                errors.Remove("llmApiKey");
                _formErrors.Set(errors);
            }
        }, llmApiKey);

        UseEffect(() =>
        {
            var error = DeploymentValidator.ValidateField("emailPassword", _deployment.Value);
            if (error != null)
            {
                var errors = new Dictionary<string, string>(_formErrors.Value) { ["emailPassword"] = error };
                _formErrors.Set(errors);
            }
            else
            {
                var errors = new Dictionary<string, string>(_formErrors.Value);
                errors.Remove("emailPassword");
                _formErrors.Set(errors);
            }
        }, emailPassword);

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
            | Text.H2("Deploy Open-Raise To Sliplane")
            | Text.Block("Configure your new deployment")

            // Project Details Section
            | Text.H3("Project Details")
            | new Field(
                projectName.ToTextInput()
                    .Placeholder("Enter project name")
                    .Invalid(_formErrors.Value.ContainsKey("projectName") ? _formErrors.Value["projectName"] : null)
            )
            .Label("Project Name")
            .Required()

            | new Field(
                serverLocation.ToSelectInput(locationOptions)
                    .Placeholder("Select server location")
                    .Invalid(_formErrors.Value.ContainsKey("serverLocation") ? _formErrors.Value["serverLocation"] : null)
            )
            .Label("Server Location")
            .Required()

            | new Field(
                serverType.ToSelectInput(serverOptions)
                    .Placeholder("Select server type")
                    .Invalid(_formErrors.Value.ContainsKey("serverType") ? _formErrors.Value["serverType"] : null)
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
                    .Invalid(_formErrors.Value.ContainsKey("llmEndpoint") ? _formErrors.Value["llmEndpoint"] : null)
            )
            .Label("Endpoint")
            .Required()

            | new Field(
                llmApiKey.ToPasswordInput()
                    .Placeholder("Enter your API key")
                    .Invalid(_formErrors.Value.ContainsKey("llmApiKey") ? _formErrors.Value["llmApiKey"] : null)
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
                    .Invalid(_formErrors.Value.ContainsKey("emailPassword") ? _formErrors.Value["emailPassword"] : null)
            )
            .Label("Password")

            // Submit Button
            | new Separator()
            | new Button("Create Account >", _onSubmit)
                .Variant(ButtonVariant.Primary)
                .Large()
                .Disabled(_isSubmitting.Value);
    }
}

