using Ivy.Sliplane.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

//https://ctrl.sliplane.io/llms.txt

namespace Ivy.Sliplane;

public class SliplaneService : ISliplaneService
{
    private readonly HttpClient _httpClient;
    private readonly SliplaneServiceOptions _options;
    private const string BaseUrl = "https://ctrl.sliplane.io/v0";

    public SliplaneService(HttpClient httpClient, SliplaneServiceOptions options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options ?? throw new ArgumentNullException(nameof(options));

        _httpClient.BaseAddress = new Uri(BaseUrl + "/");
        
        // Set up authentication - prefer OAuth token if available, otherwise use API token
        var token = GetAccessToken();
        if (!string.IsNullOrWhiteSpace(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        
        // X-Organization-ID is only needed for API token authentication
        if (!string.IsNullOrWhiteSpace(_options.OrganizationId) && !string.IsNullOrWhiteSpace(_options.ApiToken))
        {
            _httpClient.DefaultRequestHeaders.Add("X-Organization-ID", _options.OrganizationId);
        }
    }

    private string? GetAccessToken()
    {
        // Prefer OAuth access token if available
        if (!string.IsNullOrWhiteSpace(_options.AccessToken))
        {
            return _options.AccessToken;
        }
        
        // Fall back to API token for backward compatibility
        return _options.ApiToken;
    }

    private async Task<string> GetAccessTokenAsync()
    {
        // If there's a function to get the token, use it (for OAuth refresh scenarios)
        if (_options.GetAccessTokenAsync != null)
        {
            return await _options.GetAccessTokenAsync();
        }
        
        // Otherwise, use the synchronous method
        return GetAccessToken() ?? throw new InvalidOperationException("No access token available");
    }

    public async Task<Project> CreateProjectAsync(CreateProjectRequest request, CancellationToken cancellationToken = default)
    {
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("projects", content, cancellationToken);
        await EnsureSuccessStatusCodeAsync(response);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<Project>(json) ?? throw new InvalidOperationException("Deserialization returned null");
    }

    public async Task<List<Project>> ListProjectsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("projects", cancellationToken);
        await EnsureSuccessStatusCodeAsync(response);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<List<Project>>(json) ?? throw new InvalidOperationException("Deserialization returned null");
    }

    public async Task<Project> UpdateProjectAsync(string projectId, UpdateProjectRequest request, CancellationToken cancellationToken = default)
    {
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var response = await _httpClient.PatchAsync($"projects/{projectId}", content, cancellationToken);
        await EnsureSuccessStatusCodeAsync(response);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<Project>(json) ?? throw new InvalidOperationException("Deserialization returned null");
    }

    public async Task DeleteProjectAsync(string projectId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync($"projects/{projectId}", cancellationToken);
        await EnsureSuccessStatusCodeAsync(response);
    }

    public async Task<Service> CreateServiceAsync(string projectId, CreateServiceRequest request, CancellationToken cancellationToken = default)
    {
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"projects/{projectId}/services", content, cancellationToken);
        await EnsureSuccessStatusCodeAsync(response);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<Service>(json) ?? throw new InvalidOperationException("Deserialization returned null");
    }

    public async Task<List<Service>> ListServicesAsync(string projectId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(projectId))
            throw new ArgumentException("Project ID cannot be null or empty", nameof(projectId));
            
        var encodedProjectId = Uri.EscapeDataString(projectId);
        var response = await _httpClient.GetAsync($"projects/{encodedProjectId}/services", cancellationToken);
        await EnsureSuccessStatusCodeAsync(response);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<List<Service>>(json) ?? throw new InvalidOperationException("Deserialization returned null");
    }

    public async Task<Service> GetServiceAsync(string projectId, string serviceId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"projects/{projectId}/services/{serviceId}", cancellationToken);
        await EnsureSuccessStatusCodeAsync(response);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<Service>(json) ?? throw new InvalidOperationException("Deserialization returned null");
    }

    public async Task<Service> UpdateServiceAsync(string projectId, string serviceId, UpdateServiceRequest request, CancellationToken cancellationToken = default)
    {
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var response = await _httpClient.PatchAsync($"projects/{projectId}/services/{serviceId}", content, cancellationToken);
        await EnsureSuccessStatusCodeAsync(response);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<Service>(json) ?? throw new InvalidOperationException("Deserialization returned null");
    }

    public async Task DeleteServiceAsync(string projectId, string serviceId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync($"projects/{projectId}/services/{serviceId}", cancellationToken);
        await EnsureSuccessStatusCodeAsync(response);
    }

    public async Task DeployServiceAsync(string projectId, string serviceId, DeployServiceRequest? request = null, CancellationToken cancellationToken = default)
    {
        var content = request != null 
            ? new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
            : new StringContent("{}", Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"projects/{projectId}/services/{serviceId}/deploy", content, cancellationToken);
        await EnsureSuccessStatusCodeAsync(response);
    }

    public async Task<CustomDomain> AddCustomDomainAsync(string projectId, string serviceId, string domain, CancellationToken cancellationToken = default)
    {
        var request = new { domain };
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"projects/{projectId}/services/{serviceId}/custom-domains", content, cancellationToken);
        await EnsureSuccessStatusCodeAsync(response);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<CustomDomain>(json) ?? throw new InvalidOperationException("Deserialization returned null");
    }

    public async Task RemoveCustomDomainAsync(string projectId, string serviceId, string domainId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync($"projects/{projectId}/services/{serviceId}/custom-domains/{domainId}", cancellationToken);
        await EnsureSuccessStatusCodeAsync(response);
    }

    public async Task<ServiceLogsResponse> GetServiceLogsAsync(string projectId, string serviceId, int? limit = null, DateTime? since = null, CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        if (limit.HasValue)
            query.Add($"limit={limit.Value}");
        if (since.HasValue)
            query.Add($"since={since.Value:yyyy-MM-ddTHH:mm:ssZ}");
            
        var queryString = query.Count > 0 ? "?" + string.Join("&", query) : "";
        var response = await _httpClient.GetAsync($"projects/{projectId}/services/{serviceId}/logs{queryString}", cancellationToken);
        await EnsureSuccessStatusCodeAsync(response);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<ServiceLogsResponse>(json) ?? throw new InvalidOperationException("Deserialization returned null");
    }

    public async Task<Server> CreateServerAsync(CreateServerRequest request, CancellationToken cancellationToken = default)
    {
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("servers", content, cancellationToken);
        await EnsureSuccessStatusCodeAsync(response);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<Server>(json) ?? throw new InvalidOperationException("Deserialization returned null");
    }

    public async Task<List<Server>> ListServersAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("servers", cancellationToken);
        await EnsureSuccessStatusCodeAsync(response);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<List<Server>>(json) ?? throw new InvalidOperationException("Deserialization returned null");
    }

    public async Task<Server> GetServerAsync(string serverId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"servers/{serverId}", cancellationToken);
        await EnsureSuccessStatusCodeAsync(response);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<Server>(json) ?? throw new InvalidOperationException("Deserialization returned null");
    }

    public async Task DeleteServerAsync(string serverId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync($"servers/{serverId}", cancellationToken);
        await EnsureSuccessStatusCodeAsync(response);
    }

    public async Task<Server> RescaleServerAsync(string serverId, RescaleServerRequest request, CancellationToken cancellationToken = default)
    {
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"servers/{serverId}/rescale", content, cancellationToken);
        await EnsureSuccessStatusCodeAsync(response);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<Server>(json) ?? throw new InvalidOperationException("Deserialization returned null");
    }

    public async Task<RegistryCredentials> CreateRegistryCredentialsAsync(CreateRegistryCredentialsRequest request, CancellationToken cancellationToken = default)
    {
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("registry-credentials", content, cancellationToken);
        await EnsureSuccessStatusCodeAsync(response);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<RegistryCredentials>(json) ?? throw new InvalidOperationException("Deserialization returned null");
    }

    public async Task<List<RegistryCredentials>> ListRegistryCredentialsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("registry-credentials", cancellationToken);
        await EnsureSuccessStatusCodeAsync(response);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<List<RegistryCredentials>>(json) ?? throw new InvalidOperationException("Deserialization returned null");
    }

    public async Task<RegistryCredentials> GetRegistryCredentialsAsync(string credentialsId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"registry-credentials/{credentialsId}", cancellationToken);
        await EnsureSuccessStatusCodeAsync(response);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<RegistryCredentials>(json) ?? throw new InvalidOperationException("Deserialization returned null");
    }

    public async Task<RegistryCredentials> UpdateRegistryCredentialsAsync(string credentialsId, UpdateRegistryCredentialsRequest request, CancellationToken cancellationToken = default)
    {
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var response = await _httpClient.PatchAsync($"registry-credentials/{credentialsId}", content, cancellationToken);
        await EnsureSuccessStatusCodeAsync(response);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<RegistryCredentials>(json) ?? throw new InvalidOperationException("Deserialization returned null");
    }

    public async Task DeleteRegistryCredentialsAsync(string credentialsId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync($"registry-credentials/{credentialsId}", cancellationToken);
        await EnsureSuccessStatusCodeAsync(response);
    }

    private async Task EnsureSuccessStatusCodeAsync(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            // If we get a 401 and we're using OAuth, try to refresh the token
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && 
                _options.GetAccessTokenAsync != null)
            {
                // Clear the old authorization header
                _httpClient.DefaultRequestHeaders.Authorization = null;
                
                // Get a fresh token
                var newToken = await GetAccessTokenAsync();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
                
                // Retry the request (this is a simplified approach - in production you might want to retry the original request)
                // For now, we'll just throw the error
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            ApiError? error = null;
            try
            {
                error = JsonSerializer.Deserialize<ApiError>(errorContent);
            }
            catch { }

            // Include the URL in the error for debugging
            var requestUrl = response.RequestMessage?.RequestUri?.ToString() ?? "unknown";
            var message = error?.Message ?? $"Request to {requestUrl} failed with status code {response.StatusCode}. Response: {errorContent}";
            throw new SliplaneApiException(message, response.StatusCode, error?.Code);
        }
    }
}
