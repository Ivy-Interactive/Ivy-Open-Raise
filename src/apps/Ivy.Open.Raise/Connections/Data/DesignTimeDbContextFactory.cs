using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Ivy.Open.Raise.Connections.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DataContext>
{
    public DataContext CreateDbContext(string[] args)
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
            conf.MigrationsAssembly(typeof(DataContext).Assembly.FullName);
        });

        return new DataContext(optionsBuilder.Options);
    }
}
