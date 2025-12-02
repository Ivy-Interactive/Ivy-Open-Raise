using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using Ivy.Auth;
using Microsoft.AspNetCore.Http;

namespace Ivy.Sliplane.Auth;

/// <summary>
/// HTTP message handler that automatically adds Sliplane OAuth tokens to outgoing HTTP requests.
/// Extracts the JWT token from the auth_token cookie and adds it as a Bearer token in the Authorization header.
/// </summary>
public class SliplaneAuthHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SliplaneAuthHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string? jwt = null;
        var httpContext = _httpContextAccessor.HttpContext;
        
        if (httpContext != null)
        {
            // Try to get from cookie - check both "auth_token" and "jwt" for compatibility
            var jwtCookie = httpContext.Request.Cookies["auth_token"] ?? httpContext.Request.Cookies["jwt"];
            
            if (!string.IsNullOrWhiteSpace(jwtCookie))
            {
                try
                {
                    // Deserialize the cookie as AuthToken
                    // The cookie contains AccessToken, RefreshToken, and Tag properties
                    var authTokenJson = JsonSerializer.Deserialize<JsonElement>(jwtCookie);
                    
                    // Try AccessToken first (Sliplane auth), then fall back to Jwt (for compatibility)
                    if (authTokenJson.ValueKind == JsonValueKind.Object)
                    {
                        if (authTokenJson.TryGetProperty("AccessToken", out var accessTokenElement) &&
                            accessTokenElement.ValueKind == JsonValueKind.String)
                        {
                            jwt = accessTokenElement.GetString();
                        }
                        else if (authTokenJson.TryGetProperty("Jwt", out var jwtElement) &&
                                 jwtElement.ValueKind == JsonValueKind.String)
                        {
                            jwt = jwtElement.GetString();
                        }
                    }
                }
                catch (Exception)
                {
                    // Silently fail - token extraction errors shouldn't break the request
                }
            }
        }
        
        if (!string.IsNullOrWhiteSpace(jwt))
        {
            if (request.Headers.Contains("Authorization"))
            {
                request.Headers.Remove("Authorization");
            }
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}

