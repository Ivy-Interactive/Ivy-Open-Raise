using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ivy.Auth;
using Ivy.Hooks;
using Ivy.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Ivy.Sliplane.Auth;

public class SliplaneAuthProvider : IAuthProvider
{
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _authorizationUrl;
    private readonly string _tokenUrl;
    private readonly string _scope;
    private readonly HttpClient _httpClient;
    private readonly ILogger<SliplaneAuthProvider>? _logger;
    private readonly List<AuthOption> _authOptions = new();

    public SliplaneAuthProvider(HttpClient httpClient, ILogger<SliplaneAuthProvider>? logger = null)
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets(System.Reflection.Assembly.GetEntryAssembly()!)
            .Build();

        _clientId = configuration.GetValue<string>("Sliplane:ClientId") 
            ?? throw new Exception("Sliplane:ClientId is required");
        _clientSecret = configuration.GetValue<string>("Sliplane:ClientSecret") 
            ?? throw new Exception("Sliplane:ClientSecret is required");
        _authorizationUrl = configuration.GetValue<string>("Sliplane:AuthorizationUrl") 
            ?? "https://api.sliplane.io/web/oauth/authorize";
        _tokenUrl = configuration.GetValue<string>("Sliplane:TokenUrl") 
            ?? "https://api.sliplane.io/web/oauth/token";
        _scope = configuration.GetValue<string>("Sliplane:Scope") ?? "full";

        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger;

        // Sliplane only supports OAuth flow
        _authOptions.Add(new AuthOption(AuthFlow.OAuth, "Sliplane", "sliplane", Icons.Rocket));
    }

    public Task<AuthToken?> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        // Sliplane doesn't support email/password authentication, only OAuth
        return Task.FromResult<AuthToken?>(null);
    }

    public Task LogoutAsync(string token, CancellationToken cancellationToken = default)
    {
        // Sliplane doesn't have a logout endpoint, tokens are stateless
        return Task.CompletedTask;
    }

    public async Task<AuthToken?> RefreshAccessTokenAsync(AuthToken token, CancellationToken cancellationToken = default)
    {
        if (token.RefreshToken == null)
        {
            return null;
        }

        try
        {
            var requestData = new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "refresh_token", token.RefreshToken },
                { "client_id", _clientId },
                { "client_secret", _clientSecret }
            };

            var content = new FormUrlEncodedContent(requestData);
            var response = await _httpClient.PostAsync(_tokenUrl, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger?.LogError("Failed to refresh token. Status: {StatusCode}, Response: {Response}", 
                    response.StatusCode, errorContent);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var tokenResponse = JsonSerializer.Deserialize<SliplaneTokenResponse>(json);

            if (tokenResponse == null || string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
            {
                return null;
            }

            return new AuthToken(
                tokenResponse.AccessToken,
                tokenResponse.RefreshToken ?? token.RefreshToken
            );
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to refresh token");
            return null;
        }
    }

    public async Task<bool> ValidateAccessTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://ctrl.sliplane.io/v0/projects");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<UserInfo?> GetUserInfoAsync(string token, CancellationToken cancellationToken = default)
    {
        // Sliplane OAuth doesn't provide a standard user info endpoint
        // We verify the token by calling the projects API
        // Since we don't have user info, we return a basic structure
        try
        {
            var isValid = await ValidateAccessTokenAsync(token, cancellationToken);
            if (!isValid)
            {
                return null;
            }

            // Sliplane doesn't provide user email/name in OAuth tokens
            // Return a placeholder user info
            return new UserInfo(
                Id: "sliplane-user",
                Email: "user@sliplane.io",
                FullName: "Sliplane User",
                AvatarUrl: null
            );
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to get user info");
            return null;
        }
    }

    public AuthOption[] GetAuthOptions()
    {
        return _authOptions.ToArray();
    }

    public Task<Uri> GetOAuthUriAsync(AuthOption option, WebhookEndpoint callback, CancellationToken cancellationToken = default)
    {
        if (option.Id != "sliplane")
        {
            throw new ArgumentException($"Unknown auth option: {option.Id}", nameof(option));
        }

        var callbackUri = callback.GetUri(includeIdInPath: false);
        var state = callback.Id;

        var parameters = new Dictionary<string, string>
        {
            { "client_id", _clientId },
            { "redirect_uri", callbackUri.ToString() },
            { "response_type", "code" },
            { "scope", _scope },
            { "state", state }
        };

        var queryString = string.Join("&", parameters.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
        var authUrl = new Uri($"{_authorizationUrl}?{queryString}");

        return Task.FromResult(authUrl);
    }

    public async Task<AuthToken?> HandleOAuthCallbackAsync(HttpRequest request, CancellationToken cancellationToken = default)
    {
        var code = request.Query["code"].ToString();
        var error = request.Query["error"].ToString();
        var errorDescription = request.Query["error_description"].ToString();

        if (!string.IsNullOrEmpty(error) || !string.IsNullOrEmpty(errorDescription))
        {
            throw new Exception($"Sliplane OAuth error: {error} - {errorDescription}");
        }

        if (string.IsNullOrEmpty(code))
        {
            throw new Exception("Received no authorization code from Sliplane.");
        }

        try
        {
            var redirectUri = $"{request.Scheme}://{request.Host}{request.Path}";
            var requestData = new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "code", code },
                { "client_id", _clientId },
                { "client_secret", _clientSecret },
                { "redirect_uri", redirectUri }
            };

            var content = new FormUrlEncodedContent(requestData);
            var response = await _httpClient.PostAsync(_tokenUrl, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger?.LogError("Failed to exchange code for token. Status: {StatusCode}, Response: {Response}", 
                    response.StatusCode, errorContent);
                throw new Exception($"Failed to exchange authorization code for tokens: {errorContent}");
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var tokenResponse = JsonSerializer.Deserialize<SliplaneTokenResponse>(json);

            if (tokenResponse == null || string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
            {
                throw new Exception("Invalid token response from Sliplane");
            }

            return new AuthToken(
                tokenResponse.AccessToken,
                tokenResponse.RefreshToken
            );
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to handle OAuth callback");
            throw new Exception($"Failed to exchange authorization code for tokens: {ex.Message}", ex);
        }
    }

    public Task<DateTimeOffset?> GetTokenExpiration(AuthToken token, CancellationToken cancellationToken = default)
    {
        // Sliplane tokens don't include expiration in a standard way
        // We could parse JWT if they're JWTs, but for now return null
        // The token validation will handle expiration checking
        return Task.FromResult<DateTimeOffset?>(null);
    }

    private class SliplaneTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        [JsonPropertyName("scope")]
        public string? Scope { get; set; }
    }
}

