using Ivy.Sliplane.Console.Infrastructure;
using Ivy.Sliplane.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Ivy.Sliplane.Console.Commands.Server
{
    public class ServerRescaleCommand : BaseCommand<ServerRescaleCommand.Settings>
    {
        public class Settings : BaseCommandSettings
        {
            [Description("Server ID")]
            [CommandOption("--id")]
            public string ServerId { get; set; } = string.Empty;

            [Description("New instance type (base, medium, large, x-large, xx-large, dedicated-base, etc.)")]
            [CommandOption("-t|--instance-type")]
            public string InstanceType { get; set; } = string.Empty;
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

                if (string.IsNullOrWhiteSpace(settings.InstanceType))
                {
                    ShowError("Instance type is required");
                    return 1;
                }

                // Parse the instance type string to enum
                if (!Enum.TryParse<InstanceTypeEnum>(settings.InstanceType.Replace("-", ""), true, out var instanceType))
                {
                    ShowError($"Invalid instance type: {settings.InstanceType}");
                    ShowInfo("Valid types: base, medium, large, x-large, xx-large, dedicated-base, dedicated-medium, dedicated-large, dedicated-x-large, dedicated-xx-large, dedicated-xxx-large");
                    return 1;
                }

                ShowInfo($"Rescaling server {settings.ServerId} to instance type {settings.InstanceType}...");

                var service = GetSliplaneService(settings);
                var request = new RescaleServerRequest 
                { 
                    InstanceType = instanceType 
                };
                
                var server = await service.RescaleServerAsync(settings.ServerId, request);

                ShowSuccess($"Server {server.Id} rescaled successfully");
                ShowInfo($"New instance type: {server.InstanceType?.ToString() ?? settings.InstanceType}");
                
                if (settings.Output != OutputFormat.Table)
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
