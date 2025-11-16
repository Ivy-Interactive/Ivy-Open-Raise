namespace Ivy.Open.Raise.Deploy.Apps.Deploy;

public static class DeploymentValidator
{
    public static Dictionary<string, string> ValidateForm(DeploymentModel deployment)
    {
        var errors = new Dictionary<string, string>();

        // Project name validation
        if (string.IsNullOrWhiteSpace(deployment.ProjectName))
            errors["projectName"] = "Please enter a project name";
        else if (deployment.ProjectName.Length < 3)
            errors["projectName"] = "Project name must be at least 3 characters long";
        else if (deployment.ProjectName.Length > 50)
            errors["projectName"] = "Project name cannot be longer than 50 characters";
        else if (!System.Text.RegularExpressions.Regex.IsMatch(deployment.ProjectName, @"^[a-zA-Z0-9-_]+$"))
            errors["projectName"] = "Project name can only contain letters, numbers, hyphens (-), and underscores (_)";

        // Server location validation
        if (string.IsNullOrWhiteSpace(deployment.ServerLocation))
            errors["serverLocation"] = "Please select a server location";

        // Server type validation
        if (string.IsNullOrWhiteSpace(deployment.ServerType))
            errors["serverType"] = "Please select a server type";

        // LLM endpoint validation
        if (string.IsNullOrWhiteSpace(deployment.LlmEndpoint))
            errors["llmEndpoint"] = "Please enter your LLM endpoint URL";
        else if (!Uri.TryCreate(deployment.LlmEndpoint, UriKind.Absolute, out _))
            errors["llmEndpoint"] = "Please enter a valid URL (e.g., https://api.openai.com/v1)";

        // LLM API key validation
        if (string.IsNullOrWhiteSpace(deployment.LlmApiKey))
            errors["llmApiKey"] = "Please enter your API key";
        else if (deployment.LlmApiKey.Length < 10)
            errors["llmApiKey"] = "API key must be at least 10 characters long";

        // Email password validation (if provided)
        if (!string.IsNullOrWhiteSpace(deployment.EmailPassword) && deployment.EmailPassword.Length < 6)
            errors["emailPassword"] = "Email password must be at least 6 characters long";

        return errors;
    }

    public static string? ValidateField(string fieldName, DeploymentModel deployment)
    {
        return fieldName switch
        {
            "projectName" => ValidateProjectName(deployment.ProjectName),
            "serverLocation" => ValidateServerLocation(deployment.ServerLocation),
            "serverType" => ValidateServerType(deployment.ServerType),
            "llmEndpoint" => ValidateLlmEndpoint(deployment.LlmEndpoint),
            "llmApiKey" => ValidateLlmApiKey(deployment.LlmApiKey),
            "emailPassword" => ValidateEmailPassword(deployment.EmailPassword),
            _ => null
        };
    }

    private static string? ValidateProjectName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "Please enter a project name";
        if (value.Length < 3)
            return "Project name must be at least 3 characters long";
        if (value.Length > 50)
            return "Project name cannot be longer than 50 characters";
        if (!System.Text.RegularExpressions.Regex.IsMatch(value, @"^[a-zA-Z0-9-_]+$"))
            return "Project name can only contain letters, numbers, hyphens (-), and underscores (_)";
        return null;
    }

    private static string? ValidateServerLocation(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "Please select a server location";
        return null;
    }

    private static string? ValidateServerType(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "Please select a server type";
        return null;
    }

    private static string? ValidateLlmEndpoint(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "Please enter your LLM endpoint URL";
        if (!Uri.TryCreate(value, UriKind.Absolute, out _))
            return "Please enter a valid URL (e.g., https://api.openai.com/v1)";
        return null;
    }

    private static string? ValidateLlmApiKey(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "Please enter your API key";
        if (value.Length < 10)
            return "API key must be at least 10 characters long";
        return null;
    }

    private static string? ValidateEmailPassword(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value) && value.Length < 6)
            return "Email password must be at least 6 characters long";
        return null;
    }
}

