# Sliplane OAuth Auth Provider for Ivy

This package provides an OAuth2 authentication provider for Sliplane that can be used with Ivy applications.

## Overview

The Sliplane OAuth provider implements the OAuth2 authorization code flow, allowing users to authenticate with Sliplane and use their access tokens to make API calls.

## Configuration

### OAuth Endpoints

- **Authorization URL**: `https://api.sliplane.io/web/oauth/authorize`
- **Token URL**: `https://api.sliplane.io/web/oauth/token`
- **API URL**: `https://ctrl.sliplane.io`

### Required Configuration

Configuration is loaded from environment variables and .NET user secrets. You need to set:

- `Sliplane:ClientId` - Your OAuth client ID
- `Sliplane:ClientSecret` - Your OAuth client secret
- `Sliplane:AuthorizationUrl` (optional) - Defaults to `https://api.sliplane.io/web/oauth/authorize`
- `Sliplane:TokenUrl` (optional) - Defaults to `https://api.sliplane.io/web/oauth/token`
- `Sliplane:Scope` (optional) - Defaults to `"full"`

## Usage

### 1. Register the Auth Provider

In your `Program.cs` or service configuration:

```csharp
using Ivy.Sliplane.Auth;

// Add Sliplane OAuth provider
services.AddSliplaneAuth();
```

The provider will automatically load configuration from:
1. Environment variables (e.g., `Sliplane__ClientId`)
2. .NET user secrets (recommended for development)
3. appsettings.json (not recommended for secrets)

### 2. Use with Ivy Server

```csharp
var server = new Server();
server.UseAuth<SliplaneAuthProvider>();
```

The provider implements `IAuthProvider` and integrates seamlessly with Ivy's authentication system. Users will see a "Sliplane" login option in the Ivy auth UI.

### 3. Using OAuth Tokens with SliplaneService

After authentication, you can use the access token with `SliplaneService`:

```csharp
// Get the token from the authenticated user's session
var authService = serviceProvider.GetRequiredService<IAuthService>();
var token = await authService.GetTokenAsync(); // Get current auth token

var serviceOptions = new SliplaneServiceOptions
{
    AccessToken = token?.AccessToken,
    // Or use a function for token refresh:
    GetAccessTokenAsync = async () =>
    {
        var authProvider = serviceProvider.GetRequiredService<SliplaneAuthProvider>();
        if (token != null)
        {
            var refreshed = await authProvider.RefreshAccessTokenAsync(token);
            return refreshed?.AccessToken ?? token.AccessToken;
        }
        throw new Exception("No token available");
    }
};

services.AddSliplaneService(serviceOptions);
```

## Configuration

### Using .NET User Secrets (Recommended for Development)

```bash
dotnet user-secrets set "Sliplane:ClientId" "your_client_id"
dotnet user-secrets set "Sliplane:ClientSecret" "your_client_secret"
```

### Using Environment Variables

**Windows (PowerShell):**
```powershell
$env:Sliplane__ClientId="your_client_id"
$env:Sliplane__ClientSecret="your_client_secret"
```

**Linux/Mac:**
```bash
export Sliplane__ClientId="your_client_id"
export Sliplane__ClientSecret="your_client_secret"
```

**Note:** The double underscore (`__`) is used to represent nested configuration keys in environment variables.

## OAuth Flow

1. User is redirected to Sliplane authorization URL
2. User authenticates with Sliplane
3. Sliplane redirects back to your callback URL with an authorization code
4. Exchange the authorization code for an access token and refresh token
5. Use the access token to make API calls to Sliplane
6. Refresh the token when it expires using the refresh token

## Integration with Ivy

The auth provider is designed to work with Ivy's authentication system. When integrated, users can:

- Click a login button to be redirected to Sliplane
- Authenticate with Sliplane
- Be redirected back to your application with an authenticated session
- Use `IAuthService` in your views to access user information

## Example: Complete Integration

```csharp
// Program.cs
using Ivy.Sliplane.Auth;

var server = new Server();

// Register Sliplane OAuth provider
server.Services.AddSliplaneAuth();

// Use Sliplane auth provider
server.UseAuth<SliplaneAuthProvider>();

await server.RunAsync();
```

Make sure you've configured your OAuth credentials using user secrets or environment variables before running the application.

## Security Notes

- Always use HTTPS in production
- Store client secrets securely (use .NET user secrets or environment variables)
- Validate the state parameter to prevent CSRF attacks
- Handle token refresh automatically when tokens expire
- Never expose client secrets in client-side code

## Troubleshooting

### Invalid Client Credentials
- Verify your Client ID and Client Secret are correct
- Check that your OAuth application is properly configured in Sliplane

### Callback URL Mismatch
- Ensure your RedirectUri matches exactly what's configured in Sliplane
- For Ivy apps, the callback URL is typically `{your-app-url}/webhook`

### Token Exchange Failed
- Verify the authorization code hasn't expired (codes are typically short-lived)
- Check that the redirect URI matches exactly
- Ensure your client credentials are correct

