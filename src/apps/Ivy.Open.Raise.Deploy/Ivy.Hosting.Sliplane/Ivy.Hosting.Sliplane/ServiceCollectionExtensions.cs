using Microsoft.Extensions.DependencyInjection;
using System;

namespace Ivy.Hosting.Sliplane
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSliplaneService(this IServiceCollection services, Action<SliplaneServiceOptions> configureOptions)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (configureOptions == null)
                throw new ArgumentNullException(nameof(configureOptions));

            var options = new SliplaneServiceOptions();
            configureOptions(options);
            services.AddSingleton(options);

            services.AddHttpClient<ISliplaneService, SliplaneService>();

            return services;
        }

        public static IServiceCollection AddSliplaneService(this IServiceCollection services, SliplaneServiceOptions options)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            services.AddSingleton(options);
            services.AddHttpClient<ISliplaneService, SliplaneService>();

            return services;
        }
    }
}