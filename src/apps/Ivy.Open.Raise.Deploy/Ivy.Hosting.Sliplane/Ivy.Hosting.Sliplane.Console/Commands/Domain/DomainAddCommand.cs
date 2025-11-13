using Ivy.Hosting.Sliplane.Console.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Ivy.Hosting.Sliplane.Console.Commands.Domain
{
    public class DomainAddCommand : BaseCommand<DomainAddCommand.Settings>
    {
        public class Settings : BaseCommandSettings
        {
            [Description("Project ID")]
            [CommandOption("-p|--project")]
            public string ProjectId { get; set; } = string.Empty;

            [Description("Service ID")]
            [CommandOption("-s|--service")]
            public string ServiceId { get; set; } = string.Empty;

            [Description("Domain name")]
            [CommandOption("-d|--domain")]
            public string Domain { get; set; } = string.Empty;
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

                if (string.IsNullOrWhiteSpace(settings.Domain))
                {
                    ShowError("Domain name is required");
                    return 1;
                }

                var service = GetSliplaneService(settings);
                var customDomain = await service.AddCustomDomainAsync(
                    settings.ProjectId, 
                    settings.ServiceId, 
                    settings.Domain);

                ShowSuccess($"Custom domain '{settings.Domain}' added successfully with ID: {customDomain.Id}");
                ShowInfo($"Domain status: {customDomain.Status}");
                
                if (customDomain.Status == "pending")
                {
                    ShowInfo("Please configure your DNS records to point to the managed domain.");
                }
                
                if (settings.Output != OutputFormat.Table)
                {
                    WriteOutput(customDomain, settings.Output);
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