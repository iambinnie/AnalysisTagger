using AnalysisTagger.Domain.ValueObjects;

namespace AnalysisTagger.Application.DTOs;

public class CreateEventTagDto
{
    public Guid CategoryId { get; set; }
    public Timecode Position { get; set; } = Timecode.Zero;
    public string? SubCategory { get; set; }
    public List<Guid> PlayerIds { get; set; } = new();
    public string Notes { get; set; } = string.Empty;
}
