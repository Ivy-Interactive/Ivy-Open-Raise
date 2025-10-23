using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Ivy.Open.Raise.Connections.Data;

public class DataContextFactory(ServerArgs args) : IDbContextFactory<DataContext>
{
    public DataContext CreateDbContext()
    {
        var configuration = new ConfigurationBuilder()
           .AddEnvironmentVariables()
           .AddUserSecrets(Assembly.GetExecutingAssembly())
           .Build();

        var optionsBuilder = new DbContextOptionsBuilder<DataContext>();

        var connectionString = configuration.GetConnectionString("Data");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Database connection string 'Data' is not set.");
        }

        optionsBuilder.UseNpgsql(connectionString, conf =>
        {
            conf.EnableRetryOnFailure(5, TimeSpan.FromSeconds(2), null);
        });

        if (args.Verbose)
        {
            optionsBuilder
                .EnableSensitiveDataLogging()
                .LogTo(Console.WriteLine, LogLevel.Information);
        }

        return new DataContext(optionsBuilder.Options);
    }
}
