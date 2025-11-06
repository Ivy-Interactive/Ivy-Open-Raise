using Ivy.Open.Raise.Apps;
using Ivy.Open.Raise.Apps.Settings;
using Microsoft.Extensions.AI;
using OpenAI;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");
var server = new Server();
#if DEBUG
server.UseHotReload();
#endif

server.AddApp<DashboardApp>();
server.AddApp<DealsPipelineApp>();
server.AddApp<InvestorsApp>();
server.AddApp<DecksApp>();
server.AddApp<WallpaperApp>();

server.AddConnectionsFromAssembly();

var chromeSettings = new ChromeSettings()
    .UseTabs(preventDuplicates: true)
    .UseFooterMenuItemsTransformer((menuItems, navigator) =>
    {
        return [ 
            MenuItem.Default("Users").Icon(Icons.User).HandleSelect(() => navigator.Navigate<UsersApp>()),
            MenuItem.Default("Settings").Icon(Icons.Settings) 
                | MenuItem.Default("General").HandleSelect(() => navigator.Navigate<OrganizationSettingsApp>())
                | MenuItem.Default("Deal Approaches").HandleSelect(() => navigator.Navigate<DealApproachesApp>())
                | MenuItem.Default("Deal States").HandleSelect(() => navigator.Navigate<DealStatesApp>())
                | MenuItem.Default("Interaction Types").HandleSelect(() => navigator.Navigate<InteractionTypesApp>())
                | MenuItem.Default("Investor Types").HandleSelect(() => navigator.Navigate<InvestorTypesApp>())
                | MenuItem.Default("Startup Stages").HandleSelect(() => navigator.Navigate<StartupStagesApp>())
                | MenuItem.Default("Verticals").HandleSelect(() => navigator.Navigate<StartupVerticalsApp>()),
            ..menuItems
        ];
    })
    .WallpaperApp<WallpaperApp>();
server.UseChrome(chromeSettings);

server.Services.UseSmtp();
server.Services.UseBlobs();

if (server.Configuration.GetValue<string>("OpenAi:ApiKey") is { } openAiApiKey &&
    server.Configuration.GetValue<string>("OpenAi:Endpoint") is { } openAiEndpoint)
{
    var openAiClient = new OpenAIClient(new System.ClientModel.ApiKeyCredential(openAiApiKey), new OpenAIClientOptions
    {
        Endpoint = new Uri(openAiEndpoint)
    });

    var openAiChatClient = openAiClient.GetChatClient("gpt-4o");
    var chatClient = openAiChatClient.AsIChatClient();
    server.Services.AddSingleton(chatClient);
}

await server.RunAsync();
