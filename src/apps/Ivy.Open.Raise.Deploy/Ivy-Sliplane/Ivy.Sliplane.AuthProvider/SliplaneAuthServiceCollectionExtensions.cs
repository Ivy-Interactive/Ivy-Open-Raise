using Microsoft.Extensions.DependencyInjection;

namespace Ivy.Sliplane.Auth;

public static class SliplaneAuthServiceCollectionExtensions
{
    /// <summary>
    /// Adds Sliplane OAuth authentication provider to the service collection.
    /// Configuration is loaded from environment variables and user secrets.
    /// </summary>
    public static IServiceCollection AddSliplaneAuth(this IServiceCollection services)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        services.AddHttpClient<SliplaneAuthProvider>();
        services.AddSingleton<SliplaneAuthProvider>();

        return services;
    }
}

