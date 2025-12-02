using Ivy.Open.Raise.Deploy.Apps;
using Ivy.Sliplane.Auth;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");
var server = new Server();
#if DEBUG
server.UseHotReload();
#endif

// Register Sliplane OAuth authentication provider
// This also configures SliplaneService HTTP client with authentication
server.Services.AddSliplaneAuth();

// Use Sliplane auth provider
server.UseAuth<SliplaneAuthProvider>();

server.AddAppsFromAssembly();
server.AddConnectionsFromAssembly();

var chromeSettings = new ChromeSettings()
    .DefaultApp<DeployApp>()
    .UseTabs(preventDuplicates: true);
server.UseChrome(
    () => new DefaultSidebarChrome(chromeSettings)
);
await server.RunAsync();
