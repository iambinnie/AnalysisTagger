namespace AnalysisTagger.Application.DTOs;

public class PlaylistEntryDto
{
    public Guid Id { get; set; }
    public EventTagDto Tag { get; set; } = null!;
    public int SortOrder { get; set; }
    public bool HasSegmentOverride { get; set; }
}
