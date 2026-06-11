using Catalog.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Api.Infrastructure.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Game> Games { get; set; }
        public DbSet<UserCatalog> UsersCatalogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var conn = "Host=localhost;Port=5432;Database=catalog_db;Username=catalog_user;Password=catalog_pass";

                if (!string.IsNullOrWhiteSpace(conn))
                {
                    optionsBuilder.UseNpgsql(conn);
                }
            }

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // usar schema público por padrão no PostgreSQL
            modelBuilder.HasDefaultSchema("public");

            // aplica todas as configurações de IEntityTypeConfiguration no assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}