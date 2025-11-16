using Ivy.Sliplane.Console.Infrastructure;
using Ivy.Sliplane.Models;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Ivy.Sliplane.Console.Commands.Service
{
    public class ServiceCreateCommand : BaseCommand<ServiceCreateCommand.Settings>
    {
        public class Settings : BaseCommandSettings
        {
            [Description("Project ID")]
            [CommandOption("-p|--project")]
            public string ProjectId { get; set; } = string.Empty;

            [Description("Service name")]
            [CommandOption("-n|--name")]
            public string Name { get; set; } = string.Empty;

            [Description("Server ID")]
            [CommandOption("-s|--server")]
            public string ServerId { get; set; } = string.Empty;

            [Description("Repository URL (for repository deployment)")]
            [CommandOption("--repo")]
            public string? Repo { get; set; }

            [Description("Branch to deploy (default: main)")]
            [CommandOption("--branch")]
            public string Branch { get; set; } = "main";

            [Description("Path to Dockerfile (default: Dockerfile)")]
            [CommandOption("--dockerfile")]
            public string DockerfilePath { get; set; } = "Dockerfile";

            [Description("Docker build context (default: .)")]
            [CommandOption("--docker-context")]
            public string DockerContext { get; set; } = ".";

            [Description("Enable auto-deployment (default: true)")]
            [CommandOption("--auto-deploy")]
            [DefaultValue(true)]
            public bool AutoDeploy { get; set; } = true;

            [Description("Container image URL (for image deployment)")]
            [CommandOption("--image")]
            public string? Image { get; set; }

            [Description("Registry authentication ID")]
            [CommandOption("--registry-auth")]
            public string? RegistryAuth { get; set; }

            [Description("Make service publicly accessible")]
            [CommandOption("--public")]
            public bool Public { get; set; }

            [Description("Protocol (http, tcp, udp) when public")]
            [CommandOption("--protocol")]
            public string? Protocol { get; set; }

            [Description("Environment variable (format: KEY=VALUE)")]
            [CommandOption("--env")]
            public string[]? Env { get; set; }

            [Description("Secret environment variable (format: KEY=VALUE)")]
            [CommandOption("--secret-env")]
            public string[]? SecretEnv { get; set; }

            [Description("Health check path (default: /)")]
            [CommandOption("--healthcheck")]
            public string Healthcheck { get; set; } = "/";

            [Description("Override Docker CMD")]
            [CommandOption("--cmd")]
            public string? Cmd { get; set; }

            [Description("Volume mount (format: volume_id:mount_path or name:mount_path)")]
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

                if (string.IsNullOrWhiteSpace(settings.Name))
                {
                    ShowError("Service name is required");
                    return 1;
                }

                if (string.IsNullOrWhiteSpace(settings.ServerId))
                {
                    ShowError("Server ID is required");
                    return 1;
                }

                // Validate deployment type
                if (string.IsNullOrWhiteSpace(settings.Repo) && string.IsNullOrWhiteSpace(settings.Image))
                {
                    ShowError("Either --repo or --image must be specified");
                    return 1;
                }

                if (!string.IsNullOrWhiteSpace(settings.Repo) && !string.IsNullOrWhiteSpace(settings.Image))
                {
                    ShowError("Cannot specify both --repo and --image");
                    return 1;
                }

                var request = new CreateServiceRequest
                {
                    Name = settings.Name,
                    ServerId = settings.ServerId,
                    Network = new ServiceNetworkRequest
                    {
                        Public = settings.Public,
                        Protocol = settings.Protocol
                    },
                    Healthcheck = settings.Healthcheck,
                    Cmd = settings.Cmd
                };

                // Set deployment configuration
                if (!string.IsNullOrWhiteSpace(settings.Repo))
                {
                    request.Deployment = new RepositoryDeployment
                    {
                        Url = settings.Repo,
                        Branch = settings.Branch,
                        DockerfilePath = settings.DockerfilePath,
                        DockerContext = settings.DockerContext,
                        AutoDeploy = settings.AutoDeploy
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

                // Parse environment variables
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

                // Parse volumes
                request.Volumes = new List<VolumeRequest>();
                if (settings.Volume != null)
                {
                    foreach (var vol in settings.Volume)
                    {
                        var parts = vol.Split(':', 2);
                        if (parts.Length == 2)
                        {
                            var volumeRequest = new VolumeRequest
                            {
                                MountPath = parts[1]
                            };

                            // Check if it's an ID or name
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
                var createdService = await service.CreateServiceAsync(settings.ProjectId, request);

                ShowSuccess($"Service created successfully with ID: {createdService.Id}");
                
                if (settings.Output != OutputFormat.Table)
                {
                    WriteOutput(createdService, settings.Output);
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
