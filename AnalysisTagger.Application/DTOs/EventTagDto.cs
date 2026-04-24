using AnalysisTagger.Domain.ValueObjects;

namespace AnalysisTagger.Application.DTOs;

public class EventTagDto
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryColor { get; set; } = string.Empty;
    public Timecode StartTime { get; set; } = Timecode.Zero;
    public Timecode EndTime { get; set; } = Timecode.Zero;
    public TimeSpan Duration { get; set; }
    public string? SubCategory { get; set; }
    public string Notes { get; set; } = string.Empty;
    public List<Guid> TaggedPlayerIds { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}
