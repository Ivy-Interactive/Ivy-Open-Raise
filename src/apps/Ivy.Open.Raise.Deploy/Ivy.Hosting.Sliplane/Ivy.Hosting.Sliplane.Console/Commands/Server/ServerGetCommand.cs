using Ivy.Hosting.Sliplane.Console.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Ivy.Hosting.Sliplane.Console.Commands.Server
{
    public class ServerGetCommand : BaseCommand<ServerGetCommand.Settings>
    {
        public class Settings : BaseCommandSettings
        {
            [Description("Server ID")]
            [CommandOption("--id")]
            public string ServerId { get; set; } = string.Empty;
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

                var service = GetSliplaneService(settings);
                var server = await service.GetServerAsync(settings.ServerId);

                if (settings.Output == OutputFormat.Table)
                {
                    var table = new Table();
                    table.AddColumn("Property");
                    table.AddColumn("Value");

                    table.AddRow("ID", server.Id ?? "");
                    table.AddRow("Name", server.Name ?? "");
                    table.AddRow("Status", server.Status.ToString());
                    if (server.InstanceType != null)
                    {
                        table.AddRow("Instance Type", server.InstanceType.ToString());
                    }
                    
                    if (server.Location != null)
                    {
                        table.AddRow("Location", server.Location.ToString());
                    }
                    
                    if (server.IPv4 != null)
                    {
                        table.AddRow("IPv4", server.IPv4);
                    }
                    
                    if (server.IPv6 != null)
                    {
                        table.AddRow("IPv6", server.IPv6);
                    }
                    
                    table.AddRow("Created At", server.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));

                    AnsiConsole.Write(table);
                }
                else
                {
                    WriteOutput(server, settings.Output);
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