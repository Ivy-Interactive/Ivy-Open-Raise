using Ivy.Sliplane.Console.Infrastructure;
using Ivy.Sliplane.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Ivy.Sliplane.Console.Commands.Service
{
    public class ServiceDeployCommand : BaseCommand<ServiceDeployCommand.Settings>
    {
        public class Settings : BaseCommandSettings
        {
            [Description("Project ID")]
            [CommandOption("-p|--project")]
            public string ProjectId { get; set; } = string.Empty;

            [Description("Service ID")]
            [CommandOption("--id")]
            public string ServiceId { get; set; } = string.Empty;

            [Description("Image tag for image-based services")]
            [CommandOption("-t|--tag")]
            public string? Tag { get; set; }
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

                var service = GetSliplaneService(settings);

                DeployServiceRequest? request = null;
                if (!string.IsNullOrWhiteSpace(settings.Tag))
                {
                    request = new DeployServiceRequest { Tag = settings.Tag };
                }

                await service.DeployServiceAsync(settings.ProjectId, settings.ServiceId, request);

                ShowSuccess($"Deployment triggered for service {settings.ServiceId}");
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
