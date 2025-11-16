using Ivy.Open.Raise.Deploy;
using Ivy.Open.Raise.Deploy.Apps;
using Ivy.Sliplane;
using Ivy.Sliplane.Auth;
using Microsoft.AspNetCore.Http;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");
var server = new Server();
#if DEBUG
server.UseHotReload();
#endif

// Register Sliplane OAuth authentication provider
server.Services.AddSliplaneAuth();

// Use Sliplane auth provider
server.UseAuth<SliplaneAuthProvider>();

// Register HttpContextAccessor for accessing request-scoped services
server.Services.AddHttpContextAccessor();

// Register the auth handler - HttpClient factory will create instances as needed
server.Services.AddTransient<SliplaneAuthHandler>();

// Register SliplaneService options (empty since we're using the handler for auth)
var options = new SliplaneServiceOptions();
server.Services.AddSingleton(options);

// Register SliplaneService with HttpClient configured to use our auth handler
// The handler will add the Authorization header from the JWT cookie per-request
server.Services.AddHttpClient<ISliplaneService, SliplaneService>()
    .AddHttpMessageHandler<SliplaneAuthHandler>();

server.AddAppsFromAssembly();
server.AddConnectionsFromAssembly();

var chromeSettings = new ChromeSettings()
    .DefaultApp<DeployApp>()
    .UseTabs(preventDuplicates: true);
server.UseChrome(
    () => new DefaultSidebarChrome(chromeSettings)
);
await server.RunAsync();
