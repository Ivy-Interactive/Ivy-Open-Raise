using Ivy.Open.Raise.Apps;
using Microsoft.Extensions.AI;
using OpenAI;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");
var server = new Server();
#if DEBUG
server.UseHotReload();
#endif
server.AddAppsFromAssembly();
server.AddConnectionsFromAssembly();
var chromeSettings = new ChromeSettings()
    .UseTabs(preventDuplicates: true)
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
