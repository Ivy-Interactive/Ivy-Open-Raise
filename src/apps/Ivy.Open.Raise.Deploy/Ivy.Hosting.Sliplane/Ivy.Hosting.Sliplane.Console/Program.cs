using Spectre.Console.Cli;
using Ivy.Hosting.Sliplane.Console.Commands.Project;
using Ivy.Hosting.Sliplane.Console.Commands.Service;
using Ivy.Hosting.Sliplane.Console.Commands.Server;
using Ivy.Hosting.Sliplane.Console.Commands.Domain;
using Ivy.Hosting.Sliplane.Console.Commands.Registry;

var app = new CommandApp();

app.Configure(config =>
{
    config.SetApplicationName("ivy-sliplane");
    config.UseStrictParsing();

    // Project commands
    config.AddBranch("project", project =>
    {
        project.AddCommand<ProjectListCommand>("list")
            .WithDescription("List all projects");
        
        project.AddCommand<ProjectCreateCommand>("create")
            .WithDescription("Create a new project");
        
        project.AddCommand<ProjectUpdateCommand>("update")
            .WithDescription("Update a project");
        
        project.AddCommand<ProjectDeleteCommand>("delete")
            .WithDescription("Delete a project");
    });

    // Service commands
    config.AddBranch("service", service =>
    {
        service.AddCommand<ServiceListCommand>("list")
            .WithDescription("List all services in a project");
        
        service.AddCommand<ServiceGetCommand>("get")
            .WithDescription("Get details of a specific service");
        
        service.AddCommand<ServiceCreateCommand>("create")
            .WithDescription("Create a new service");
        
        service.AddCommand<ServiceUpdateCommand>("update")
            .WithDescription("Update a service");
        
        service.AddCommand<ServiceDeleteCommand>("delete")
            .WithDescription("Delete a service");
        
        service.AddCommand<ServiceDeployCommand>("deploy")
            .WithDescription("Deploy a service");
        
        service.AddCommand<ServiceLogsCommand>("logs")
            .WithDescription("Get service logs");
    });

    // Domain commands
    config.AddBranch("domain", domain =>
    {
        domain.AddCommand<DomainAddCommand>("add")
            .WithDescription("Add a custom domain to a service");
        
        domain.AddCommand<DomainRemoveCommand>("remove")
            .WithDescription("Remove a custom domain from a service");
    });

    // Server commands
    config.AddBranch("server", server =>
    {
        server.AddCommand<ServerListCommand>("list")
            .WithDescription("List all servers");
        
        server.AddCommand<ServerGetCommand>("get")
            .WithDescription("Get details of a specific server");
        
        server.AddCommand<ServerCreateCommand>("create")
            .WithDescription("Create a new server");
        
        server.AddCommand<ServerDeleteCommand>("delete")
            .WithDescription("Delete a server");
        
        server.AddCommand<ServerRescaleCommand>("rescale")
            .WithDescription("Rescale a server to a different instance type");
    });

    // Registry commands
    config.AddBranch("registry", registry =>
    {
        registry.AddCommand<RegistryListCommand>("list")
            .WithDescription("List all registry credentials");
        
        registry.AddCommand<RegistryGetCommand>("get")
            .WithDescription("Get details of specific registry credentials");
        
        registry.AddCommand<RegistryCreateCommand>("create")
            .WithDescription("Create new registry credentials");
        
        registry.AddCommand<RegistryUpdateCommand>("update")
            .WithDescription("Update existing registry credentials");
        
        registry.AddCommand<RegistryDeleteCommand>("delete")
            .WithDescription("Delete registry credentials");
    });
});

return await app.RunAsync(args);
