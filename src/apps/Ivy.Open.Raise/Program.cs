using System.Reflection;
using Ivy.Filters;
using Ivy.Open.Raise;
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

server.UseChrome<ChromeApp>();

server.UseWebApplicationBuilder(builder =>
{
    builder.Services.AddControllers()
        .AddApplicationPart(Assembly.GetExecutingAssembly());
});

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

server.SetMetaTitle("Open Raise by Ivy.app");

await server.RunAsync();
