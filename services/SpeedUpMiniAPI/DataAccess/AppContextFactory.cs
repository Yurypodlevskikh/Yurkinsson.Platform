using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DataAccess;

// This factory is used by EF Core CLI tools to create an instance of the AppDbContext at design time
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Build configuration to access appsettings and user secrets
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()) // Set base path for config files
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true) // Add appsettings.json if exists
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true) // Add appsettings.Development.json if exists
            .AddUserSecrets<AppDbContextFactory>() // Add User Secrets configuration
            .Build();

        // Get the connection string from configuration
        var connectionString = configuration.GetConnectionString("MariaDbSpeedUp");

        // Setup options for DbContext
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        // optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 21)));
        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        // var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        // optionsBuilder.UseMySql("Server=localhost;Database=MariaDbSpeedUp;User=root;Password=your_password;",
        //     new MySqlServerVersion(new Version(8, 0, 21)));

        return new AppDbContext(optionsBuilder.Options);
    }
}