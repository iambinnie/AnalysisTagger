using System.Text.Json;
using AnalysisTagger.Domain.Models;
using AnalysisTagger.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnalysisTagger.Infrastructure.Data.Configurations;

public class EventTagConfiguration : IEntityTypeConfiguration<EventTag>
{
    public void Configure(EntityTypeBuilder<EventTag> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Notes).HasMaxLength(2000);
        builder.Property(e => e.SubCategory).HasMaxLength(100);

        builder.Property(e => e.CustomData)
            .HasConversion(
                dict => JsonSerializer.Serialize(dict, (JsonSerializerOptions?)null),
                json => JsonSerializer.Deserialize<Dictionary<string, string>>(json, (JsonSerializerOptions?)null) ?? new Dictionary<string, string>());

        builder.OwnsOne(e => e.Segment, seg =>
        {
            seg.Property(s => s.Start)
                .HasConversion(tc => tc.Value.Ticks, ticks => new Timecode(TimeSpan.FromTicks(ticks)))
                .HasColumnName("SegmentStartTicks");
            seg.Property(s => s.End)
                .HasConversion(tc => tc.Value.Ticks, ticks => new Timecode(TimeSpan.FromTicks(ticks)))
                .HasColumnName("SegmentEndTicks");
        });

        builder.HasOne(e => e.Category)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.TaggedPlayers)
            .WithMany()
            .UsingEntity("EventTagPlayers");

        builder.HasMany(e => e.Drawings)
            .WithOne()
            .HasForeignKey("EventTagId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
