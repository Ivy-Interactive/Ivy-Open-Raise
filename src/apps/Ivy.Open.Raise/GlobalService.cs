using Microsoft.Extensions.Hosting;

namespace Ivy.Open.Raise;

public static class GlobalServiceExtensions
{
    public static void UseGlobalService(this IServiceCollection services)
    {
        services.AddSingleton<GlobalService>();
        services.AddHostedService<GlobalServiceInitializer>();
    }
}

public sealed class GlobalServiceInitializer(GlobalService global) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
        => await global.InitializeAsync();

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}

public class GlobalService(DataContextFactory factory)
{
    public bool _initialize;

    public OrganizationSetting? Settings { get; set; }

    public async Task InitializeAsync()
    {
        if (_initialize) return;
        await RefreshAsync();
        _initialize = true;
    }
    
    public async Task RefreshAsync()
    {
        await using var db = factory.CreateDbContext();
        Settings = await db.OrganizationSettings.FirstOrDefaultAsync();
    }
}