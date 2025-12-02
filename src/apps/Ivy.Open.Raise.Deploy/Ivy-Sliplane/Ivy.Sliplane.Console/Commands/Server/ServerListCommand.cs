using Ivy.Sliplane.Console.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Ivy.Sliplane.Console.Commands.Server
{
    public class ServerListCommand : BaseCommand<ServerListCommand.Settings>
    {
        public class Settings : BaseCommandSettings
        {
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            try
            {
                var service = GetSliplaneService(settings);
                var servers = await service.ListServersAsync();

                if (settings.Output == OutputFormat.Table)
                {
                    var table = new Table();
                    table.AddColumn("ID");
                    table.AddColumn("Name");
                    table.AddColumn("Status");
                    table.AddColumn("Instance Type");
                    table.AddColumn("Location");

                    foreach (var server in servers)
                    {
                        table.AddRow(
                            server.Id ?? "",
                            server.Name ?? "",
                            server.Status.ToString(),
                            server.InstanceType?.ToString() ?? "",
                            server.Location?.ToString() ?? ""
                        );
                    }

                    AnsiConsole.Write(table);
                }
                else
                {
                    WriteOutput(servers, settings.Output);
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
