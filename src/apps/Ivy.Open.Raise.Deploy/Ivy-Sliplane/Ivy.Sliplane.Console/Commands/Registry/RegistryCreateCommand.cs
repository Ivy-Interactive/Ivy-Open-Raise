using Ivy.Sliplane.Console.Infrastructure;
using Ivy.Sliplane.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Ivy.Sliplane.Console.Commands.Registry
{
    public class RegistryCreateCommand : BaseCommand<RegistryCreateCommand.Settings>
    {
        public class Settings : BaseCommandSettings
        {
            [Description("Credentials name")]
            [CommandOption("-n|--name")]
            public string Name { get; set; } = string.Empty;

            [Description("Registry URL")]
            [CommandOption("-u|--url")]
            public string RegistryUrl { get; set; } = string.Empty;

            [Description("Username")]
            [CommandOption("--username")]
            public string Username { get; set; } = string.Empty;

            [Description("Password")]
            [CommandOption("-p|--password")]
            public string Password { get; set; } = string.Empty;
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(settings.Name))
                {
                    ShowError("Credentials name is required");
                    return 1;
                }

                if (string.IsNullOrWhiteSpace(settings.RegistryUrl))
                {
                    ShowError("Registry URL is required");
                    return 1;
                }

                if (string.IsNullOrWhiteSpace(settings.Username))
                {
                    ShowError("Username is required");
                    return 1;
                }

                if (string.IsNullOrWhiteSpace(settings.Password))
                {
                    // Prompt for password if not provided
                    settings.Password = AnsiConsole.Prompt(
                        new TextPrompt<string>("Enter password:")
                            .PromptStyle("red")
                            .Secret());
                }

                var service = GetSliplaneService(settings);
                var request = new CreateRegistryCredentialsRequest
                {
                    Name = settings.Name,
                    RegistryUrl = settings.RegistryUrl,
                    Username = settings.Username,
                    Password = settings.Password
                };

                var credentials = await service.CreateRegistryCredentialsAsync(request);

                ShowSuccess($"Registry credentials created successfully with ID: {credentials.Id}");
                
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
