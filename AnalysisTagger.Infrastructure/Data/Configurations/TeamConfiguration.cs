using AnalysisTagger.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnalysisTagger.Infrastructure.Data.Configurations;

public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(100);
        builder.Property(t => t.ShieldImagePath).HasMaxLength(500);

        builder.HasMany(t => t.Players)
            .WithOne()
            .HasForeignKey("TeamId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
