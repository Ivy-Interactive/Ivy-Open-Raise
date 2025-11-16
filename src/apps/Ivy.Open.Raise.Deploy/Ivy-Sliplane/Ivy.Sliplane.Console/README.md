# Ivy Sliplane CLI

A command-line interface for managing Sliplane services, built with .NET 9.0 and Spectre.Console.

## Installation

### Prerequisites
- .NET 9.0 SDK or later
- Sliplane API access (get your API key from the Sliplane dashboard)

## Configuration

The CLI requires two key pieces of information:
- **API Key**: Your Sliplane API authentication token
- **Organization ID**: Your Sliplane organization identifier

### Configuration Methods

The CLI reads configuration from multiple sources (in order of precedence):

1. **Command-line options**
   ```bash
   ivy-sliplane project list --api-key YOUR_KEY --org-id YOUR_ORG
   ```

2. **Environment variables**
   ```bash
   export SLIPLANE_API_KEY=your_api_key
   export SLIPLANE_ORG_ID=your_org_id
   ```

3. **.NET User Secrets** (recommended for development)
   ```bash
   dotnet user-secrets set "SLIPLANE_API_KEY" "your_api_key"
   dotnet user-secrets set "SLIPLANE_ORG_ID" "your_org_id"
   ```

4. **Configuration file**
   Create `~/.ivy-sliplane/config.json`:
   ```json
   {
     "SLIPLANE_API_KEY": "your_api_key",
     "SLIPLANE_ORG_ID": "your_org_id"
   }
   ```

## Usage

### Global Options

Available for all commands:

- `--api-key` - Override the API key from configuration
- `--org-id` - Override the organization ID from configuration
- `-o, --output` - Output format: `table` (default), `json`, or `yaml`
- `-v, --verbose` - Enable verbose output
- `-h, --help` - Show help information

### Project Management

#### List Projects
```bash
ivy-sliplane project list
```

#### Create Project
```bash
ivy-sliplane project create --name "My Project"
```

#### Update Project
```bash
ivy-sliplane project update --id project_123456 --name "Updated Name"
```

#### Delete Project
```bash
ivy-sliplane project delete --id project_123456
ivy-sliplane project delete --id project_123456 --force  # Skip confirmation
```

### Service Management

#### List Services
```bash
ivy-sliplane service list --project project_123456
```

#### Create Service

**Repository-based deployment:**
```bash
ivy-sliplane service create \
  --project project_123456 \
  --name "API Service" \
  --server server_789 \
  --repo https://github.com/username/repo \
  --branch main \
  --dockerfile Dockerfile \
  --public \
  --protocol http \
  --env NODE_ENV=production \
  --secret-env DATABASE_URL=postgresql://...
```

**Image-based deployment:**
```bash
ivy-sliplane service create \
  --project project_123456 \
  --name "Redis Cache" \
  --server server_789 \
  --image redis:7-alpine \
  --volume redis-data:/data
```

Options:
- `-p, --project` - Project ID (required)
- `-n, --name` - Service name (required)
- `-s, --server` - Server ID (required)
- `--repo` - Repository URL (for repository deployment)
- `--branch` - Branch to deploy (default: main)
- `--dockerfile` - Path to Dockerfile (default: Dockerfile)
- `--docker-context` - Docker build context (default: .)
- `--auto-deploy` - Enable auto-deployment (default: true)
- `--image` - Container image URL (for image deployment)
- `--registry-auth` - Registry authentication ID
- `--public` - Make service publicly accessible
- `--protocol` - Protocol: http, tcp, or udp (required when public)
- `--env` - Environment variable (format: KEY=VALUE)
- `--secret-env` - Secret environment variable (format: KEY=VALUE)
- `--healthcheck` - Health check path (default: /)
- `--cmd` - Override Docker CMD
- `--volume` - Volume mount (format: volume_id:mount_path or name:mount_path)

#### Deploy Service
```bash
ivy-sliplane service deploy \
  --project project_123456 \
  --id service_789 \
  --tag v1.2.3  # Optional: for image-based services
```

### Server Management

#### List Servers
```bash
ivy-sliplane server list
```

#### Create Server
```bash
ivy-sliplane server create \
  --name "Production Server" \
  --instance-type instance_type_123
```

Options:
- `-n, --name` - Server name (required)
- `-t, --instance-type` - Instance type ID (required)

## Output Formats

The CLI supports multiple output formats:

### Table Format (default)
```bash
ivy-sliplane project list
```
```
┌──────────────┬─────────────┐
│ ID           │ Name        │
├──────────────┼─────────────┤
│ project_123  │ My Project  │
│ project_456  │ Another One │
└──────────────┴─────────────┘
```

### JSON Format
```bash
ivy-sliplane project list --output json
```
```json
[
  {
    "id": "project_123",
    "name": "My Project"
  },
  {
    "id": "project_456",
    "name": "Another One"
  }
]
```

### YAML Format
```bash
ivy-sliplane project list --output yaml
```

## Examples

### Complete Workflow Example

```bash
# 1. Create a project
ivy-sliplane project create --name "E-Commerce Platform"

# 2. Create a server
ivy-sliplane server create --name "Production" --instance-type small

# 3. Deploy a GitHub repository
ivy-sliplane service create \
  --project project_abc123 \
  --name "API Backend" \
  --server server_xyz789 \
  --repo https://github.com/mycompany/api \
  --branch main \
  --public \
  --protocol http \
  --env NODE_ENV=production \
  --env PORT=3000 \
  --secret-env DATABASE_URL=postgresql://user:pass@host/db

# 4. Deploy a Docker image
ivy-sliplane service create \
  --project project_abc123 \
  --name "PostgreSQL Database" \
  --server server_xyz789 \
  --image postgres:15 \
  --env POSTGRES_DB=myapp \
  --secret-env POSTGRES_PASSWORD=secretpass \
  --volume postgres-data:/var/lib/postgresql/data

# 5. Trigger a manual deployment
ivy-sliplane service deploy \
  --project project_abc123 \
  --id service_api123 \
  --tag v2.0.0

# 6. Check service status
ivy-sliplane service list --project project_abc123 --output json
```

### Development Setup

```bash
# Set up user secrets for development
dotnet user-secrets set "SLIPLANE_API_KEY" "your_dev_api_key"
dotnet user-secrets set "SLIPLANE_ORG_ID" "your_dev_org_id"

# Test the configuration
ivy-sliplane project list
```

## Error Handling

The CLI provides clear error messages:

```bash
$ ivy-sliplane project create
Error: Project name is required

$ ivy-sliplane project list
Error: SLIPLANE_API_KEY not configured. Set it via user secrets, environment variable, or --api-key option.
```