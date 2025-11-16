using Ivy.Sliplane.Console.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Ivy.Sliplane.Console.Commands.Service
{
    public class ServiceListCommand : BaseCommand<ServiceListCommand.Settings>
    {
        public class Settings : BaseCommandSettings
        {
            [Description("Project ID")]
            [CommandOption("-p|--project")]
            public string ProjectId { get; set; } = string.Empty;
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

                var service = GetSliplaneService(settings);
                var services = await service.ListServicesAsync(settings.ProjectId);

                if (settings.Output == OutputFormat.Table)
                {
                    var table = new Table();
                    table.AddColumn("ID");
                    table.AddColumn("Name");
                    table.AddColumn("Status");
                    table.AddColumn("Server ID");
                    table.AddColumn("Public");

                    foreach (var svc in services)
                    {
                        table.AddRow(
                            svc.Id ?? "",
                            svc.Name ?? "",
                            svc.Status.ToString(),
                            svc.ServerId ?? "",
                            svc.Network?.Public.ToString() ?? "false"
                        );
                    }

                    AnsiConsole.Write(table);
                }
                else
                {
                    WriteOutput(services, settings.Output);
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
