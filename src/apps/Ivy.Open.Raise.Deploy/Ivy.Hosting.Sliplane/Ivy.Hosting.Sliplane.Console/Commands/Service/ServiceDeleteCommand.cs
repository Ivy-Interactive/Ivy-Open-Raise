using Ivy.Hosting.Sliplane.Console.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Ivy.Hosting.Sliplane.Console.Commands.Service
{
    public class ServiceDeleteCommand : BaseCommand<ServiceDeleteCommand.Settings>
    {
        public class Settings : BaseCommandSettings
        {
            [Description("Project ID")]
            [CommandOption("-p|--project")]
            public string ProjectId { get; set; } = string.Empty;

            [Description("Service ID")]
            [CommandOption("--id")]
            public string ServiceId { get; set; } = string.Empty;

            [Description("Skip confirmation prompt")]
            [CommandOption("-f|--force")]
            public bool Force { get; set; }
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(settings.ProjectId))
                {
                    ShowError("Project ID is required");
                    return 1;
                }

                if (string.IsNullOrWhiteSpace(settings.ServiceId))
                {
                    ShowError("Service ID is required");
                    return 1;
                }

                if (!ConfirmAction($"Are you sure you want to delete service {settings.ServiceId}?", settings.Force))
                {
                    ShowInfo("Operation cancelled");
                    return 0;
                }

                var service = GetSliplaneService(settings);
                await service.DeleteServiceAsync(settings.ProjectId, settings.ServiceId);

                ShowSuccess($"Service {settings.ServiceId} deleted successfully");
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