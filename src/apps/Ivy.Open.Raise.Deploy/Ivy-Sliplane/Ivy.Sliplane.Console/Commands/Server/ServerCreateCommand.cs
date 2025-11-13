using Ivy.Sliplane.Console.Infrastructure;
using Ivy.Sliplane.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Ivy.Sliplane.Console.Commands.Server
{
    public class ServerCreateCommand : BaseCommand<ServerCreateCommand.Settings>
    {
        public class Settings : BaseCommandSettings
        {
            [Description("Server name")]
            [CommandOption("-n|--name")]
            public string Name { get; set; } = string.Empty;

            [Description("Instance type (base, medium, large, x-large, xx-large, dedicated-base, etc.)")]
            [CommandOption("-t|--instance-type")]
            public string InstanceType { get; set; } = string.Empty;
            
            [Description("Location (sin, fsn, nbg, ash, hel, hil)")]
            [CommandOption("-l|--location")]
            public string Location { get; set; } = string.Empty;
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(settings.Name))
                {
                    ShowError("Server name is required");
                    return 1;
                }

                if (string.IsNullOrWhiteSpace(settings.InstanceType))
                {
                    ShowError("Instance type is required");
                    return 1;
                }
                
                if (string.IsNullOrWhiteSpace(settings.Location))
                {
                    ShowError("Location is required");
                    return 1;
                }

                // Parse the instance type string to enum
                if (!Enum.TryParse<InstanceTypeEnum>(settings.InstanceType.Replace("-", ""), true, out var instanceType))
                {
                    ShowError($"Invalid instance type: {settings.InstanceType}");
                    ShowInfo("Valid types: base, medium, large, x-large, xx-large, dedicated-base, dedicated-medium, dedicated-large, dedicated-x-large, dedicated-xx-large, dedicated-xxx-large");
                    return 1;
                }
                
                // Parse the location string to enum
                if (!Enum.TryParse<LocationEnum>(settings.Location, true, out var location))
                {
                    ShowError($"Invalid location: {settings.Location}");
                    ShowInfo("Valid locations: sin (Singapore), fsn (Falkenstein), nbg (Nuremberg), ash (Ashburn), hel (Helsinki), hil (Hillsboro)");
                    return 1;
                }

                var service = GetSliplaneService(settings);
                var request = new CreateServerRequest 
                { 
                    Name = settings.Name,
                    InstanceType = instanceType,
                    Location = location
                };
                
                var server = await service.CreateServerAsync(request);

                ShowSuccess($"Server created successfully with ID: {server.Id}");
                
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
