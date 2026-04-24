using AnalysisTagger.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AnalysisTagger.Infrastructure.Data.Configurations;

public class TagTemplateConfiguration : IEntityTypeConfiguration<TagTemplate>
{
    public void Configure(EntityTypeBuilder<TagTemplate> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(100);

        builder.HasMany(t => t.Categories)
            .WithOne()
            .HasForeignKey("TagTemplateId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
