using Catalog.Worker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Worker.Infrastructure.Configurations
{
    public class UserCatalogConfiguration : IEntityTypeConfiguration<UserCatalog>
    {
        public void Configure(EntityTypeBuilder<UserCatalog> builder)
        {
            builder.ToTable("user_catalogs");

            builder.HasKey(uc => uc.Id);
            builder.Property(uc => uc.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");

            builder.Property(uc => uc.GameId)
                .IsRequired()
                .HasColumnName("game_id");

            builder.Property(uc => uc.UserId)
                .IsRequired()
                .HasColumnName("user_id");

            builder.Property(uc => uc.BuyDate)
                .HasColumnName("buy_date")
                .HasColumnType("date")
                .HasConversion(
                    v => v.ToDateTime(TimeOnly.MinValue),
                    v => DateOnly.FromDateTime(v));

            builder.Property(uc => uc.LogData)
                .HasColumnName("log_data")
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("now()")
                .ValueGeneratedOnAdd();

            // prevenir duplicatas (mesmo usuário comprando mesmo jogo várias vezes)
            builder.HasIndex(uc => new { uc.UserId, uc.GameId })
                .IsUnique()
                .HasDatabaseName("ux_user_catalog_user_game");
        }
    }
}