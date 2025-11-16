using Ivy.Sliplane.Console.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Ivy.Sliplane.Console.Commands.Service
{
    public class ServiceGetCommand : BaseCommand<ServiceGetCommand.Settings>
    {
        public class Settings : BaseCommandSettings
        {
            [Description("Project ID")]
            [CommandOption("-p|--project")]
            public string ProjectId { get; set; } = string.Empty;

            [Description("Service ID")]
            [CommandOption("--id")]
            public string ServiceId { get; set; } = string.Empty;
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
                var svc = await service.GetServiceAsync(settings.ProjectId, settings.ServiceId);

                if (settings.Output == OutputFormat.Table)
                {
                    var table = new Table();
                    table.AddColumn("Property");
                    table.AddColumn("Value");

                    table.AddRow("ID", svc.Id ?? "");
                    table.AddRow("Name", svc.Name ?? "");
                    table.AddRow("Status", svc.Status.ToString());
                    table.AddRow("Server ID", svc.ServerId ?? "");
                    table.AddRow("Project ID", svc.ProjectId ?? "");
                    table.AddRow("Created At", svc.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
                    table.AddRow("Public", svc.Network?.Public.ToString() ?? "false");
                    
                    if (svc.Network?.Public == true)
                    {
                        table.AddRow("Protocol", svc.Network?.Protocol ?? "");
                        table.AddRow("Managed Domain", svc.Network?.ManagedDomain ?? "");
                    }
                    
                    table.AddRow("Internal Domain", svc.Network?.InternalDomain ?? "");
                    table.AddRow("Health Check", svc.Healthcheck ?? "/");

                    AnsiConsole.Write(table);
                }
                else
                {
                    WriteOutput(svc, settings.Output);
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
