using Ivy.Hosting.Sliplane.Console.Infrastructure;
using Ivy.Hosting.Sliplane.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Ivy.Hosting.Sliplane.Console.Commands.Project
{
    public class ProjectListCommand : BaseCommand<ProjectListCommand.Settings>
    {
        public class Settings : BaseCommandSettings
        {
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            try
            {
                var service = GetSliplaneService(settings);
                var projects = await service.ListProjectsAsync();

                if (settings.Output == OutputFormat.Table)
                {
                    var table = new Table();
                    table.AddColumn("ID");
                    table.AddColumn("Name");

                    foreach (var project in projects)
                    {
                        table.AddRow(project.Id ?? "", project.Name ?? "");
                    }

                    AnsiConsole.Write(table);
                }
                else
                {
                    WriteOutput(projects, settings.Output);
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