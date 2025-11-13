using Ivy.Hosting.Sliplane.Console.Infrastructure;
using Ivy.Hosting.Sliplane.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Ivy.Hosting.Sliplane.Console.Commands.Project
{
    public class ProjectCreateCommand : BaseCommand<ProjectCreateCommand.Settings>
    {
        public class Settings : BaseCommandSettings
        {
            [Description("Name of the project")]
            [CommandOption("-n|--name")]
            public string Name { get; set; } = string.Empty;
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(settings.Name))
                {
                    ShowError("Project name is required");
                    return 1;
                }

                var service = GetSliplaneService(settings);
                var request = new CreateProjectRequest { Name = settings.Name };
                var project = await service.CreateProjectAsync(request);

                ShowSuccess($"Project created successfully with ID: {project.Id}");
                
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