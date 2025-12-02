using System.Reflection;
using Ivy.Open.Raise.Apps;
using Ivy.Open.Raise.Apps.Settings;
using Ivy.Sliplane.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.AI;
using OpenAI;

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

server.AddAppsFromAssembly();
server.AddConnectionsFromAssembly();

server.UseChrome<ChromeApp>();

server.UseBuilder(builder =>
{
    // Add our own application controllers
    // Note: Ivy Framework automatically adds its controllers (including WebhookController) in Server.cs after this
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
