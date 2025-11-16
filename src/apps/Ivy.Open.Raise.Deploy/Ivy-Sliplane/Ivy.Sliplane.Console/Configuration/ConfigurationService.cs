using Microsoft.Extensions.Configuration;
using System.IO;
using System;

namespace Ivy.Sliplane.Console.Configuration
{
    public class ConfigurationService
    {
        private readonly IConfiguration _configuration;
        private static ConfigurationService? _instance;
        
        public static ConfigurationService Instance => _instance ??= new ConfigurationService();

        private ConfigurationService()
        {
            var builder = new ConfigurationBuilder();

            // Add configuration file from user's home directory if it exists
            var configPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".ivy-sliplane",
                "config.json"
            );
            
            if (File.Exists(configPath))
            {
                builder.AddJsonFile(configPath, optional: true, reloadOnChange: false);
            }

            // Add user secrets
            builder.AddUserSecrets<Program>();

            // Add environment variables
            builder.AddEnvironmentVariables();

            _configuration = builder.Build();
        }

        public string? GetApiKey(string? overrideValue = null)
        {
            return overrideValue 
                ?? _configuration["SLIPLANE_API_KEY"] 
                ?? throw new InvalidOperationException("SLIPLANE_API_KEY not configured. Set it via user secrets, environment variable, or --api-key option.");
        }

        public string? GetOrganizationId(string? overrideValue = null)
        {
            return overrideValue 
                ?? _configuration["SLIPLANE_ORG_ID"] 
                ?? throw new InvalidOperationException("SLIPLANE_ORG_ID not configured. Set it via user secrets, environment variable, or --org-id option.");
        }
    }
}
