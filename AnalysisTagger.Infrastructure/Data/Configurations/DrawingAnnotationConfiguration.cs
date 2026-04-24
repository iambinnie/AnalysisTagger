using AnalysisTagger.Domain.Models;
using AnalysisTagger.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnalysisTagger.Infrastructure.Data.Configurations;

public class DrawingAnnotationConfiguration : IEntityTypeConfiguration<DrawingAnnotation>
{
    public void Configure(EntityTypeBuilder<DrawingAnnotation> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.ThumbnailPath).HasMaxLength(500);

        builder.Property(d => d.FrameTimecode)
            .HasConversion(
                tc => tc.Value.Ticks,
                ticks => new Timecode(TimeSpan.FromTicks(ticks)));
    }
}
