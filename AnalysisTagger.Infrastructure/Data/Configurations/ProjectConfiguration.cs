using AnalysisTagger.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnalysisTagger.Infrastructure.Data.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Title).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Competition).HasMaxLength(100);
        builder.Property(p => p.Season).HasMaxLength(50);
        builder.Property(p => p.VideoFilePath).HasMaxLength(500);

        builder.HasOne(p => p.HomeTeam)
            .WithMany()
            .HasForeignKey("HomeTeamId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.AwayTeam)
            .WithMany()
            .HasForeignKey("AwayTeamId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Template)
            .WithOne()
            .HasForeignKey<TagTemplate>("ProjectId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Events)
            .WithOne()
            .HasForeignKey("ProjectId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Playlists)
            .WithOne()
            .HasForeignKey("ProjectId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
