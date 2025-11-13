using Ivy.Hosting.Sliplane.Console.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Ivy.Hosting.Sliplane.Console.Commands.Registry
{
    public class RegistryDeleteCommand : BaseCommand<RegistryDeleteCommand.Settings>
    {
        public class Settings : BaseCommandSettings
        {
            [Description("Credentials ID")]
            [CommandOption("--id")]
            public string CredentialsId { get; set; } = string.Empty;

            [Description("Skip confirmation prompt")]
            [CommandOption("-f|--force")]
            public bool Force { get; set; }
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

                if (!ConfirmAction($"Are you sure you want to delete registry credentials {settings.CredentialsId}?", settings.Force))
                {
                    ShowInfo("Operation cancelled");
                    return 0;
                }

                var service = GetSliplaneService(settings);
                await service.DeleteRegistryCredentialsAsync(settings.CredentialsId);

                ShowSuccess($"Registry credentials {settings.CredentialsId} deleted successfully");
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