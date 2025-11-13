using Spectre.Console;
using Spectre.Console.Cli;
using System.Net.Http;
using System.Text.Json;
using Ivy.Sliplane;
using Ivy.Sliplane.Console.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ivy.Sliplane.Console.Infrastructure
{
    public abstract class BaseCommand<TSettings> : AsyncCommand<TSettings> where TSettings : BaseCommandSettings
    {
        protected ISliplaneService GetSliplaneService(TSettings settings)
        {
            var apiKey = ConfigurationService.Instance.GetApiKey(settings.ApiKey);
            var orgId = ConfigurationService.Instance.GetOrganizationId(settings.OrganizationId);

            var services = new ServiceCollection();
            services.AddSliplaneService(options =>
            {
                options.ApiToken = apiKey;
                options.OrganizationId = orgId;
            });

            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider.GetRequiredService<ISliplaneService>();
        }

        protected void WriteOutput<T>(T data, OutputFormat format)
        {
            switch (format)
            {
                case OutputFormat.Json:
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    var json = JsonSerializer.Serialize(data, options);
                    AnsiConsole.WriteLine(json);
                    break;

                case OutputFormat.Table:
                    WriteTable(data);
                    break;

                case OutputFormat.Yaml:
                    AnsiConsole.WriteLine("[yellow]YAML output not yet implemented. Showing JSON instead:[/]");
                    var yamlOptions = new JsonSerializerOptions { WriteIndented = true };
                    var yamlJson = JsonSerializer.Serialize(data, yamlOptions);
                    AnsiConsole.WriteLine(yamlJson);
                    break;
            }
        }

        protected virtual void WriteTable<T>(T data)
        {
            // Default implementation, can be overridden by specific commands
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(data, options);
            AnsiConsole.WriteLine(json);
        }

        protected bool ConfirmAction(string message, bool force)
        {
            if (force) return true;
            return AnsiConsole.Confirm(message);
        }

        protected void ShowError(string message)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {message}");
        }

        protected void ShowSuccess(string message)
        {
            AnsiConsole.MarkupLine($"[green]Success:[/] {message}");
        }

        protected void ShowInfo(string message)
        {
            AnsiConsole.MarkupLine($"[blue]Info:[/] {message}");
        }
    }
}
