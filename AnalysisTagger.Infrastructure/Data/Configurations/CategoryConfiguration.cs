using System.Text.Json;
using AnalysisTagger.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnalysisTagger.Infrastructure.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Color).HasMaxLength(20);

        builder.Property(c => c.DefaultLeadTime)
            .HasConversion(ts => ts.Ticks, ticks => TimeSpan.FromTicks(ticks));

        builder.Property(c => c.DefaultLagTime)
            .HasConversion(ts => ts.Ticks, ticks => TimeSpan.FromTicks(ticks));

        builder.Property(c => c.SubCategories)
            .HasConversion(
                list => JsonSerializer.Serialize(list, (JsonSerializerOptions?)null),
                json => JsonSerializer.Deserialize<List<string>>(json, (JsonSerializerOptions?)null) ?? new List<string>());
    }
}
