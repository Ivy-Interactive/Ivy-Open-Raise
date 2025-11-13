using Ivy.Hosting.Sliplane.Console.Infrastructure;
using Ivy.Hosting.Sliplane.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Ivy.Hosting.Sliplane.Console.Commands.Project
{
    public class ProjectUpdateCommand : BaseCommand<ProjectUpdateCommand.Settings>
    {
        public class Settings : BaseCommandSettings
        {
            [Description("Project ID")]
            [CommandOption("--id")]
            public string Id { get; set; } = string.Empty;

            [Description("New name for the project")]
            [CommandOption("-n|--name")]
            public string Name { get; set; } = string.Empty;
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

                if (string.IsNullOrWhiteSpace(settings.Name))
                {
                    ShowError("New project name is required");
                    return 1;
                }

                var service = GetSliplaneService(settings);
                var request = new UpdateProjectRequest { Name = settings.Name };
                var project = await service.UpdateProjectAsync(settings.Id, request);

                ShowSuccess($"Project {project.Id} updated successfully");
                
                if (settings.Output != OutputFormat.Table)
                {
                    WriteOutput(project, settings.Output);
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