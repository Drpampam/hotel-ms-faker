using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace hotelier_core_app.Migrations
{
    /// <summary>
    /// Factory for creating <see cref="AppDbContext"/> instances at design time for migrations.
    /// </summary>
    internal class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        /// <summary>
        /// Creates a new <see cref="AppDbContext"/> instance for design-time operations.
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        /// <returns>A new <see cref="AppDbContext"/> instance.</returns>
        public AppDbContext CreateDbContext(string[] args)
        {
            string jsonPath = "appsettings.json";
            var basePath = Directory.GetCurrentDirectory();

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile(Path.Combine(basePath, jsonPath), optional: false, reloadOnChange: true)
                .Build();

            string? connectionString = configuration.GetConnectionString("DbConnectionString");
            Console.WriteLine($"Loaded Connection String: {connectionString}");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("⚠️ Database connection string is null or empty! Check appsettings.json.");
            }

            var builder = new DbContextOptionsBuilder<AppDbContext>();
                 builder.UseNpgsql(connectionString)
                     .ReplaceService<IModelCacheKeyFactory, TenantModelCacheKeyFactory>();
            var tenantProvider = new TenantProvider();
            tenantProvider.SetSchema("public");

            return new AppDbContext(builder.Options, tenantProvider);
        }
    }

}