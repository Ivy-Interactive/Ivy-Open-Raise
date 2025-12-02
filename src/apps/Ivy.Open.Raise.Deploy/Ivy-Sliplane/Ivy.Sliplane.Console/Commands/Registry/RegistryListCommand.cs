using Ivy.Sliplane.Console.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Ivy.Sliplane.Console.Commands.Registry
{
    public class RegistryListCommand : BaseCommand<RegistryListCommand.Settings>
    {
        public class Settings : BaseCommandSettings
        {
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            try
            {
                var service = GetSliplaneService(settings);
                var credentials = await service.ListRegistryCredentialsAsync();

                if (settings.Output == OutputFormat.Table)
                {
                    var table = new Table();
                    table.AddColumn("ID");
                    table.AddColumn("Name");
                    table.AddColumn("Registry URL");
                    table.AddColumn("Username");
                    table.AddColumn("Created At");

                    foreach (var cred in credentials)
                    {
                        table.AddRow(
                            cred.Id ?? "",
                            cred.Name ?? "",
                            cred.RegistryUrl ?? "",
                            cred.Username ?? "",
                            cred.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                        );
                    }

                    AnsiConsole.Write(table);
                }
                else
                {
                    WriteOutput(credentials, settings.Output);
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
