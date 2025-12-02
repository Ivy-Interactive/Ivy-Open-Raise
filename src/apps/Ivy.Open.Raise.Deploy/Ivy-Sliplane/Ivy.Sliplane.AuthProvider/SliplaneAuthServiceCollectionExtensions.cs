using Ivy.Sliplane;
using Microsoft.Extensions.DependencyInjection;

namespace Ivy.Sliplane.Auth;

public static class SliplaneAuthServiceCollectionExtensions
{
    /// <summary>
    /// Adds Sliplane OAuth authentication provider to the service collection.
    /// Configuration is loaded from environment variables and user secrets.
    /// Also configures SliplaneService HTTP client to automatically include authentication tokens.
    /// </summary>
    public static IServiceCollection AddSliplaneAuth(this IServiceCollection services)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        // Register HttpContextAccessor for accessing request-scoped services (needed for auth handler)
        services.AddHttpContextAccessor();

        // Register Sliplane auth provider
        services.AddHttpClient<SliplaneAuthProvider>();
        services.AddSingleton<SliplaneAuthProvider>();

        // Register the auth handler that adds JWT tokens to outgoing requests
        services.AddTransient<SliplaneAuthHandler>();

        // Register SliplaneService options
        var options = new SliplaneServiceOptions();
        services.AddSingleton(options);

        // Register SliplaneService with HttpClient configured to use our auth handler
        // The handler will add the Authorization header from the JWT cookie per-request
        services.AddHttpClient<ISliplaneService, SliplaneService>()
            .AddHttpMessageHandler<SliplaneAuthHandler>();

        return services;
    }
}

