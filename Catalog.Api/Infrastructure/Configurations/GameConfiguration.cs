using Catalog.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Api.Infrastructure.Configurations
{
    public class GameConfiguration : IEntityTypeConfiguration<Game>
    {
        public void Configure(EntityTypeBuilder<Game> builder)
        {
            builder.ToTable("games");

            builder.HasKey(g => g.Id);
            builder.Property(g => g.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");

            builder.Property(g => g.Name)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("name");

            builder.Property(g => g.Description)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("description");

            builder.Property(g => g.Genre)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnName("genre");

            // Armazenar DateOnly como coluna date no PostgreSQL
            builder.Property(g => g.Release)
                .HasColumnName("release_date")
                .HasColumnType("date")
                .HasConversion(
                    v => v.ToDateTime(TimeOnly.MinValue),
                    v => DateOnly.FromDateTime(v));

            builder.Property(g => g.Price)
                .HasPrecision(10, 2)
                .IsRequired()
                .HasColumnName("price");

            builder.Property(g => g.LogData)
                .HasColumnName("log_data")
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("now()")
                .ValueGeneratedOnAdd();
        }
    }
}
