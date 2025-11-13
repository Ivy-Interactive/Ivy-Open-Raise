using Ivy.Hosting.Sliplane.Console.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Ivy.Hosting.Sliplane.Console.Commands.Project
{
    public class ProjectDeleteCommand : BaseCommand<ProjectDeleteCommand.Settings>
    {
        public class Settings : BaseCommandSettings
        {
            [Description("Project ID")]
            [CommandOption("--id")]
            public string Id { get; set; } = string.Empty;

            [Description("Skip confirmation prompt")]
            [CommandOption("-f|--force")]
            public bool Force { get; set; }
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(settings.Id))
                {
                    ShowError("Project ID is required");
                    return 1;
                }

                if (!ConfirmAction($"Are you sure you want to delete project {settings.Id}?", settings.Force))
                {
                    ShowInfo("Operation cancelled");
                    return 0;
                }

                var service = GetSliplaneService(settings);
                await service.DeleteProjectAsync(settings.Id);

                ShowSuccess($"Project {settings.Id} deleted successfully");
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