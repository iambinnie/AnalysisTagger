using System.Text.Json;
using AnalysisTagger.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnalysisTagger.Infrastructure.Data.Configurations;

public class PlayerConfiguration : IEntityTypeConfiguration<Player>
{
    public void Configure(EntityTypeBuilder<Player> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Position).HasMaxLength(50);
        builder.Property(p => p.PhotoPath).HasMaxLength(500);

        builder.Property(p => p.CustomAttributes)
            .HasConversion(
                dict => JsonSerializer.Serialize(dict, (JsonSerializerOptions?)null),
                json => JsonSerializer.Deserialize<Dictionary<string, string>>(json, (JsonSerializerOptions?)null) ?? new Dictionary<string, string>());
    }
}
