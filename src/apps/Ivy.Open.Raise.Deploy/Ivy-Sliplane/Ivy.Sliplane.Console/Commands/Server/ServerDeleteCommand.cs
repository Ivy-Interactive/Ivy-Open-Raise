using Ivy.Sliplane.Console.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Ivy.Sliplane.Console.Commands.Server
{
    public class ServerDeleteCommand : BaseCommand<ServerDeleteCommand.Settings>
    {
        public class Settings : BaseCommandSettings
        {
            [Description("Server ID")]
            [CommandOption("--id")]
            public string ServerId { get; set; } = string.Empty;

            [Description("Skip confirmation prompt")]
            [CommandOption("-f|--force")]
            public bool Force { get; set; }
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(settings.ServerId))
                {
                    ShowError("Server ID is required");
                    return 1;
                }

                if (!ConfirmAction($"Are you sure you want to delete server {settings.ServerId}? This action cannot be undone.", settings.Force))
                {
                    ShowInfo("Operation cancelled");
                    return 0;
                }

                var service = GetSliplaneService(settings);
                await service.DeleteServerAsync(settings.ServerId);

                ShowSuccess($"Server {settings.ServerId} deleted successfully");
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
