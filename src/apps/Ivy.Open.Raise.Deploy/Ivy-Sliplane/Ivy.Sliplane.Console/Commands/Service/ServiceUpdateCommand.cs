using Ivy.Sliplane.Console.Infrastructure;
using Ivy.Sliplane.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Ivy.Sliplane.Console.Commands.Service
{
    public class ServiceUpdateCommand : BaseCommand<ServiceUpdateCommand.Settings>
    {
        public class Settings : BaseCommandSettings
        {
            [Description("Project ID")]
            [CommandOption("-p|--project")]
            public string ProjectId { get; set; } = string.Empty;

            [Description("Service ID")]
            [CommandOption("--id")]
            public string ServiceId { get; set; } = string.Empty;

            [Description("New service name")]
            [CommandOption("-n|--name")]
            public string? Name { get; set; }

            [Description("Repository URL")]
            [CommandOption("--repo")]
            public string? Repo { get; set; }

            [Description("Branch to deploy")]
            [CommandOption("--branch")]
            public string? Branch { get; set; }

            [Description("Path to Dockerfile")]
            [CommandOption("--dockerfile")]
            public string? DockerfilePath { get; set; }

            [Description("Docker build context")]
            [CommandOption("--docker-context")]
            public string? DockerContext { get; set; }

            [Description("Enable/disable auto-deployment")]
            [CommandOption("--auto-deploy")]
            public bool? AutoDeploy { get; set; }

            [Description("Container image URL")]
            [CommandOption("--image")]
            public string? Image { get; set; }

            [Description("Registry authentication ID")]
            [CommandOption("--registry-auth")]
            public string? RegistryAuth { get; set; }

            [Description("Make service publicly accessible")]
            [CommandOption("--public")]
            public bool? Public { get; set; }

            [Description("Protocol (http, tcp, udp)")]
            [CommandOption("--protocol")]
            public string? Protocol { get; set; }

            [Description("Environment variable (format: KEY=VALUE)")]
            [CommandOption("--env")]
            public string[]? Env { get; set; }

            [Description("Secret environment variable (format: KEY=VALUE)")]
            [CommandOption("--secret-env")]
            public string[]? SecretEnv { get; set; }

            [Description("Health check path")]
            [CommandOption("--healthcheck")]
            public string? Healthcheck { get; set; }

            [Description("Override Docker CMD")]
            [CommandOption("--cmd")]
            public string? Cmd { get; set; }

            [Description("Volume mount (format: volume_id:mount_path)")]
            [CommandOption("--volume")]
            public string[]? Volume { get; set; }
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

                var request = new UpdateServiceRequest
                {
                    Name = settings.Name,
                    Healthcheck = settings.Healthcheck,
                    Cmd = settings.Cmd
                };

                // Set network configuration if provided
                if (settings.Public.HasValue || !string.IsNullOrWhiteSpace(settings.Protocol))
                {
                    request.Network = new ServiceNetworkRequest
                    {
                        Public = settings.Public ?? false,
                        Protocol = settings.Protocol
                    };
                }

                // Set deployment configuration if provided
                if (!string.IsNullOrWhiteSpace(settings.Repo))
                {
                    request.Deployment = new RepositoryDeployment
                    {
                        Url = settings.Repo,
                        Branch = settings.Branch ?? "main",
                        DockerfilePath = settings.DockerfilePath ?? "Dockerfile",
                        DockerContext = settings.DockerContext ?? ".",
                        AutoDeploy = settings.AutoDeploy ?? true
                    };
                }
                else if (!string.IsNullOrWhiteSpace(settings.Image))
                {
                    request.Deployment = new ImageDeployment
                    {
                        Url = settings.Image,
                        RegistryAuthenticationId = settings.RegistryAuth
                    };
                }

                // Parse environment variables if provided
                if (settings.Env != null || settings.SecretEnv != null)
                {
                    request.Env = new List<EnvironmentVariable>();
                    
                    if (settings.Env != null)
                    {
                        foreach (var env in settings.Env)
                        {
                            var parts = env.Split('=', 2);
                            if (parts.Length == 2)
                            {
                                request.Env.Add(new EnvironmentVariable
                                {
                                    Key = parts[0],
                                    Value = parts[1],
                                    Secret = false
                                });
                            }
                        }
                    }

                    if (settings.SecretEnv != null)
                    {
                        foreach (var env in settings.SecretEnv)
                        {
                            var parts = env.Split('=', 2);
                            if (parts.Length == 2)
                            {
                                request.Env.Add(new EnvironmentVariable
                                {
                                    Key = parts[0],
                                    Value = parts[1],
                                    Secret = true
                                });
                            }
                        }
                    }
                }

                // Parse volumes if provided
                if (settings.Volume != null)
                {
                    request.Volumes = new List<VolumeRequest>();
                    foreach (var vol in settings.Volume)
                    {
                        var parts = vol.Split(':', 2);
                        if (parts.Length == 2)
                        {
                            var volumeRequest = new VolumeRequest
                            {
                                MountPath = parts[1]
                            };

                            if (parts[0].StartsWith("volume_"))
                            {
                                volumeRequest.Id = parts[0];
                            }
                            else
                            {
                                volumeRequest.Name = parts[0];
                            }

                            request.Volumes.Add(volumeRequest);
                        }
                    }
                }

                var service = GetSliplaneService(settings);
                var updatedService = await service.UpdateServiceAsync(settings.ProjectId, settings.ServiceId, request);

                ShowSuccess($"Service {updatedService.Id} updated successfully");
                
                if (settings.Output != OutputFormat.Table)
                {
                    WriteOutput(updatedService, settings.Output);
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
