using Catalog.Worker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Worker.Infrastructure.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<UserCatalog> UsersCatalogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var postgreUser = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "catalog_user";
                var postgrePassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "catalog_pass";
                var postgreDb = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "catalog_db";
                var postgreHost = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";

                var connectionString = $"Host={postgreHost};Port=5432;Database={postgreDb};Username={postgreUser};Password={postgrePassword}";

                if (!string.IsNullOrWhiteSpace(connectionString))
                {
                    optionsBuilder.UseNpgsql(connectionString);
                }
            }

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("public");

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}