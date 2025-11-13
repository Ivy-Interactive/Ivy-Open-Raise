using Ivy.Hosting.Sliplane.Console.Infrastructure;
using Ivy.Hosting.Sliplane.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Ivy.Hosting.Sliplane.Console.Commands.Registry
{
    public class RegistryUpdateCommand : BaseCommand<RegistryUpdateCommand.Settings>
    {
        public class Settings : BaseCommandSettings
        {
            [Description("Credentials ID")]
            [CommandOption("--id")]
            public string CredentialsId { get; set; } = string.Empty;

            [Description("New name")]
            [CommandOption("-n|--name")]
            public string? Name { get; set; }

            [Description("New registry URL")]
            [CommandOption("-u|--url")]
            public string? RegistryUrl { get; set; }

            [Description("New username")]
            [CommandOption("--username")]
            public string? Username { get; set; }

            [Description("New password")]
            [CommandOption("-p|--password")]
            public string? Password { get; set; }
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(settings.CredentialsId))
                {
                    ShowError("Credentials ID is required");
                    return 1;
                }

                // Check if at least one field is being updated
                if (string.IsNullOrWhiteSpace(settings.Name) &&
                    string.IsNullOrWhiteSpace(settings.RegistryUrl) &&
                    string.IsNullOrWhiteSpace(settings.Username) &&
                    string.IsNullOrWhiteSpace(settings.Password))
                {
                    ShowError("At least one field must be specified to update");
                    return 1;
                }

                var service = GetSliplaneService(settings);
                var request = new UpdateRegistryCredentialsRequest
                {
                    Name = settings.Name,
                    RegistryUrl = settings.RegistryUrl,
                    Username = settings.Username,
                    Password = settings.Password
                };

                var credentials = await service.UpdateRegistryCredentialsAsync(settings.CredentialsId, request);

                ShowSuccess($"Registry credentials {credentials.Id} updated successfully");
                
                if (settings.Output != OutputFormat.Table)
                {
                    WriteOutput(credentials, settings.Output);
                }

                return 0;
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                return 1;
            }
        }
    }
}