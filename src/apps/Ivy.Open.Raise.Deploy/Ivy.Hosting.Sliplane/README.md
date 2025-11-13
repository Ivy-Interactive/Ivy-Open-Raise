# Ivy.Hosting.Sliplane

A .NET client library and CLI for the Sliplane cloud hosting platform API.

## Projects

### Ivy.Hosting.Sliplane

A .NET 9 class library that provides a strongly-typed client for the Sliplane API.

**Features:**
- Full coverage of all Sliplane API endpoints
- Strongly-typed request and response models
- Async/await support with CancellationToken
- Dependency injection support
- Comprehensive error handling

**Installation:**
```bash
dotnet add package Ivy.Hosting.Sliplane
```

**Usage:**
```csharp
// Register in DI container
services.AddSliplaneService(options =>
{
    options.ApiKey = "your-api-key";
    options.OrganizationId = "your-org-id";
});

// Use the service
var sliplaneService = serviceProvider.GetRequiredService<ISliplaneService>();
var projects = await sliplaneService.ListProjectsAsync();
```

### Ivy.Hosting.Sliplane.Console (ivy-sliplane CLI)

A feature-rich command-line interface for managing Sliplane resources.

## CLI Installation

```bash
dotnet tool install -g Ivy.Hosting.Sliplane.Console
```

## Configuration

The CLI supports multiple configuration methods (in order of precedence):
1. Command-line arguments
2. Environment variables
3. User secrets (for development)
4. Configuration file

### Environment Variables
- `SLIPLANE_API_KEY` - Your Sliplane API key
- `SLIPLANE_ORGANIZATION_ID` - Your organization ID
- `SLIPLANE_API_URL` - API endpoint (optional, defaults to https://api.sliplane.io/v1)

### Configuration File
Create a configuration file at:
- Windows: `%APPDATA%\ivy-sliplane\config.json`
- macOS/Linux: `~/.ivy-sliplane/config.json`

```json
{
  "apiKey": "your-api-key",
  "organizationId": "your-org-id",
  "apiUrl": "https://api.sliplane.io/v1"
}
```

### User Secrets (Development)
```bash
dotnet user-secrets set "Sliplane:ApiKey" "your-api-key"
dotnet user-secrets set "Sliplane:OrganizationId" "your-org-id"
```

## CLI Commands

### Global Options
- `--api-key` - Override API key
- `--org-id` - Override organization ID
- `--api-url` - Override API URL
- `--output` - Output format (table, json, yaml)

### Project Management

#### List Projects
```bash
ivy-sliplane project list
```

#### Create Project
```bash
ivy-sliplane project create --name "my-project" --region "us-east-1"
```

#### Update Project
```bash
ivy-sliplane project update --id "project-id" --name "new-name"
```

#### Delete Project
```bash
ivy-sliplane project delete --id "project-id" --force
```

### Service Management

#### List Services
```bash
ivy-sliplane service list --project-id "project-id"
```

#### Get Service Details
```bash
ivy-sliplane service get --id "service-id"
```

#### Create Service
```bash
# From Docker image
ivy-sliplane service create --project-id "project-id" --name "my-service" \
  --image "nginx:latest" --port 80 --instance-type "standard"

# From GitHub repository
ivy-sliplane service create --project-id "project-id" --name "my-app" \
  --github-url "https://github.com/user/repo" --branch "main" \
  --port 3000 --instance-type "standard"
```

Options:
- `--project-id` - Project ID (required)
- `--name` - Service name (required)
- `--image` - Docker image
- `--github-url` - GitHub repository URL
- `--branch` - Git branch
- `--port` - Container port (required)
- `--instance-type` - Instance type (required)
- `--env` - Environment variables (can be specified multiple times)
- `--env-file` - Load environment variables from file
- `--registry-credentials-id` - Registry credentials ID for private images

#### Update Service
```bash
ivy-sliplane service update --id "service-id" --name "new-name" \
  --env "KEY1=value1" --env "KEY2=value2"
```

#### Delete Service
```bash
ivy-sliplane service delete --id "service-id" --force
```

#### Deploy Service
```bash
# Deploy latest
ivy-sliplane service deploy --id "service-id"

# Deploy specific tag
ivy-sliplane service deploy --id "service-id" --tag "v1.2.3"
```

#### View Service Logs
```bash
# View recent logs
ivy-sliplane service logs --id "service-id"

# Follow logs (live tail)
ivy-sliplane service logs --id "service-id" --follow
```

### Domain Management

#### Add Custom Domain
```bash
ivy-sliplane domain add --service-id "service-id" --domain "example.com"
```

#### Remove Custom Domain
```bash
ivy-sliplane domain remove --service-id "service-id" --domain "example.com" --force
```

### Server Management

#### List Servers
```bash
ivy-sliplane server list
```

#### Get Server Details
```bash
ivy-sliplane server get --id "server-id"
```

#### Create Server
```bash
ivy-sliplane server create --name "my-server" --region "us-east-1" \
  --instance-type "standard"
```

#### Delete Server
```bash
ivy-sliplane server delete --id "server-id" --force
```

#### Rescale Server
```bash
ivy-sliplane server rescale --id "server-id" --instance-type "performance"
```

### Registry Credentials Management

#### List Registry Credentials
```bash
ivy-sliplane registry list
```

#### Get Registry Credentials Details
```bash
ivy-sliplane registry get --id "credentials-id"
```

#### Create Registry Credentials
```bash
ivy-sliplane registry create --name "Docker Hub" \
  --url "https://registry.hub.docker.com" \
  --username "myuser" --password "mypass"
```

#### Update Registry Credentials
```bash
ivy-sliplane registry update --id "credentials-id" \
  --name "New Name" --username "newuser"
```

#### Delete Registry Credentials
```bash
ivy-sliplane registry delete --id "credentials-id" --force
```

## Output Formats

The CLI supports multiple output formats:

### Table (Default)
Human-readable table format with colors and formatting.

### JSON
```bash
ivy-sliplane project list --output json
```

### YAML
```bash
ivy-sliplane project list --output yaml
```

## Examples

### Deploy a Docker Container
```bash
# Create a project
ivy-sliplane project create --name "production" --region "us-east-1"

# Create a service from Docker Hub
ivy-sliplane service create --project-id "proj-123" --name "nginx-web" \
  --image "nginx:alpine" --port 80 --instance-type "standard"

# Add a custom domain
ivy-sliplane domain add --service-id "svc-456" --domain "www.example.com"

# View logs
ivy-sliplane service logs --id "svc-456" --follow
```

### Deploy from GitHub
```bash
# Create service from GitHub repository
ivy-sliplane service create --project-id "proj-123" --name "my-app" \
  --github-url "https://github.com/myuser/myapp" --branch "main" \
  --port 3000 --instance-type "standard" \
  --env "NODE_ENV=production" --env "API_KEY=secret"

# Deploy a specific version
ivy-sliplane service deploy --id "svc-789" --tag "v2.1.0"
```

### Use Private Registry
```bash
# Add registry credentials
ivy-sliplane registry create --name "My Registry" \
  --url "https://my-registry.com" --username "user" --password "pass"

# Create service with private image
ivy-sliplane service create --project-id "proj-123" --name "private-app" \
  --image "my-registry.com/app:latest" --port 8080 \
  --instance-type "performance" --registry-credentials-id "reg-001"
```

## Error Handling

The CLI provides clear error messages and exit codes:
- Exit code 0: Success
- Exit code 1: Error

Use `--force` flag to skip confirmation prompts for destructive operations.

## Development

### Building from Source
```bash
# Clone the repository
git clone https://github.com/your-org/Ivy.Hosting.Sliplane.git

# Build the library
dotnet build Ivy.Hosting.Sliplane

# Build the CLI
dotnet build Ivy.Hosting.Sliplane.Console

# Run the CLI locally
dotnet run --project Ivy.Hosting.Sliplane.Console -- project list
```

### Running Tests
```bash
dotnet test
```

## Requirements

- .NET 9.0 or later
- Sliplane API account with valid API key

## License

[Your License]

## Support

For issues or questions, please contact support or create an issue on GitHub.