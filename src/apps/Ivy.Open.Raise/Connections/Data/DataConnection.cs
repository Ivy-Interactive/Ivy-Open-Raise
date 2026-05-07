using Ivy;

namespace Ivy.Open.Raise.Connections.Data;

public class DataConnection : IConnection, IHaveSecrets
{
    public string GetContext(string connectionPath)
    {
        var connectionFile = nameof(DataConnection) + ".cs";
        var contextFactoryFile = nameof(DataContextFactory) + ".cs";
        var files = Directory.GetFiles(connectionPath, "*.*", SearchOption.TopDirectoryOnly)
            .Where(f => !f.EndsWith(connectionFile) && !f.EndsWith(contextFactoryFile) && !f.EndsWith("EfmigrationsLock.cs"))
            .Select(File.ReadAllText)
            .ToArray();
        return string.Join(Environment.NewLine, files);
    }

    public string GetName() => nameof(Data);

    public string GetNamespace() => typeof(DataConnection).Namespace;

    public string GetConnectionType() => "EntityFramework.Postgres";

    public ConnectionEntity[] GetEntities()
    {
        return typeof(DataContext)
            .GetProperties()
            .Where(e => e.PropertyType.IsGenericType && e.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .Where(e => e.PropertyType.GenericTypeArguments[0].Name != "EfmigrationsLock")
            .Select(e => new ConnectionEntity(e.PropertyType.GenericTypeArguments[0].Name, e.Name))
            .ToArray();
    }

    public void RegisterServices(Server server)
    {
        server.Services.AddSingleton<DataContextFactory>();
    }

    public async Task<(bool ok, string? message)> TestConnection(IConfiguration config)
    {
        try
        {
            var connectionString = config.GetConnectionString("Data");
            if (string.IsNullOrWhiteSpace(connectionString))
                return (false, "Connection string 'Data' is not configured.");

            var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
            optionsBuilder.UseNpgsql(connectionString);
            await using var context = new DataContext(optionsBuilder.Options);
            await context.Database.CanConnectAsync();
            return (true, "Connection successful.");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public Secret[] GetSecrets()
    {
        return
        [
            new("ConnectionStrings:Data")
        ];
    }
}
