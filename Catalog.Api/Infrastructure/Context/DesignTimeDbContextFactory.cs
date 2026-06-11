using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Catalog.Api.Infrastructure.Context
{
    // For EF Core tools at design time (migrations)
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var postgreUser = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "catalog_user";
            var postgrePassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "catalog_pass";
            var postgreDb = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "catalog_db";
            var postgreHost = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";

            var connectionString = $"Host={postgreHost};Port=5432;Database={postgreDb};Username={postgreUser};Password={postgrePassword}";

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
