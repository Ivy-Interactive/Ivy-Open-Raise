using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using Ivy.Auth;
using Microsoft.AspNetCore.Http;

namespace Ivy.Open.Raise.Deploy;

public class SliplaneAuthHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SliplaneAuthHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[SliplaneAuthHandler] SendAsync called for: {request.RequestUri}");
        
        string? jwt = null;
        var httpContext = _httpContextAccessor.HttpContext;
        
        if (httpContext != null)
        {
            Console.WriteLine("[SliplaneAuthHandler] HttpContext found");
            Console.WriteLine($"[SliplaneAuthHandler] Request path: {httpContext.Request.Path}");
            Console.WriteLine($"[SliplaneAuthHandler] All cookies: {string.Join(", ", httpContext.Request.Cookies.Keys)}");
            
            // Try to get from cookie - check both "auth_token" and "jwt" for compatibility
            var jwtCookie = httpContext.Request.Cookies["auth_token"] ?? httpContext.Request.Cookies["jwt"];
            Console.WriteLine($"[SliplaneAuthHandler] auth_token cookie exists: {!string.IsNullOrWhiteSpace(httpContext.Request.Cookies["auth_token"])}");
            Console.WriteLine($"[SliplaneAuthHandler] jwt cookie exists: {!string.IsNullOrWhiteSpace(httpContext.Request.Cookies["jwt"])}");
            
            if (!string.IsNullOrWhiteSpace(jwtCookie))
            {
                Console.WriteLine($"[SliplaneAuthHandler] Cookie length: {jwtCookie.Length}");
                
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
                            Console.WriteLine($"[SliplaneAuthHandler] Extracted JWT from AccessToken, length: {jwt?.Length ?? 0}");
                        }
                        else if (authTokenJson.TryGetProperty("Jwt", out var jwtElement) &&
                                 jwtElement.ValueKind == JsonValueKind.String)
                        {
                            jwt = jwtElement.GetString();
                            Console.WriteLine($"[SliplaneAuthHandler] Extracted JWT from Jwt property, length: {jwt?.Length ?? 0}");
                        }
                        else
                        {
                            Console.WriteLine("[SliplaneAuthHandler] AuthToken JSON doesn't have AccessToken or Jwt property");
                            Console.WriteLine($"[SliplaneAuthHandler] Available properties: {string.Join(", ", authTokenJson.EnumerateObject().Select(p => p.Name))}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SliplaneAuthHandler] EXCEPTION parsing cookie: {ex.Message}");
                    Console.WriteLine($"[SliplaneAuthHandler] StackTrace: {ex.StackTrace}");
                }
            }
        }
        else
        {
            Console.WriteLine("[SliplaneAuthHandler] ERROR: HttpContext is null!");
        }
        
        if (!string.IsNullOrWhiteSpace(jwt))
        {
            if (request.Headers.Contains("Authorization"))
            {
                request.Headers.Remove("Authorization");
            }
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
            Console.WriteLine($"[SliplaneAuthHandler] SUCCESS: Set Authorization header with Bearer token");
        }
        else
        {
            Console.WriteLine("[SliplaneAuthHandler] ERROR: No token available!");
        }

        return await base.SendAsync(request, cancellationToken);
    }
}

