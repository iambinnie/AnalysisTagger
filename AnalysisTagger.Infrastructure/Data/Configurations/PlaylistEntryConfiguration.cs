using AnalysisTagger.Domain.Models;
using AnalysisTagger.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnalysisTagger.Infrastructure.Data.Configurations;

public class PlaylistEntryConfiguration : IEntityTypeConfiguration<PlaylistEntry>
{
    public void Configure(EntityTypeBuilder<PlaylistEntry> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasOne(e => e.Tag)
            .WithMany()
            .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsOne(e => e.SegmentOverride, seg =>
        {
            seg.Property(s => s.Start)
                .HasConversion(tc => tc.Value.Ticks, ticks => new Timecode(TimeSpan.FromTicks(ticks)))
                .HasColumnName("SegmentOverrideStartTicks");
            seg.Property(s => s.End)
                .HasConversion(tc => tc.Value.Ticks, ticks => new Timecode(TimeSpan.FromTicks(ticks)))
                .HasColumnName("SegmentOverrideEndTicks");
        });
    }
}
