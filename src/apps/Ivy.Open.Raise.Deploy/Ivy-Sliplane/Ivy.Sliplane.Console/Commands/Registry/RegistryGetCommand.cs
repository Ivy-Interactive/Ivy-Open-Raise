using Ivy.Sliplane.Console.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Ivy.Sliplane.Console.Commands.Registry
{
    public class RegistryGetCommand : BaseCommand<RegistryGetCommand.Settings>
    {
        public class Settings : BaseCommandSettings
        {
            [Description("Credentials ID")]
            [CommandOption("--id")]
            public string CredentialsId { get; set; } = string.Empty;
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(settings.CredentialsId))
                {
                    ShowError("Credentials ID is required");
                    return 1;
                }

                var service = GetSliplaneService(settings);
                var credentials = await service.GetRegistryCredentialsAsync(settings.CredentialsId);

                if (settings.Output == OutputFormat.Table)
                {
                    var table = new Table();
                    table.AddColumn("Property");
                    table.AddColumn("Value");

                    table.AddRow("ID", credentials.Id ?? "");
                    table.AddRow("Name", credentials.Name ?? "");
                    table.AddRow("Registry URL", credentials.RegistryUrl ?? "");
                    table.AddRow("Username", credentials.Username ?? "");
                    table.AddRow("Created At", credentials.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));

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
