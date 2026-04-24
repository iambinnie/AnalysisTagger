namespace AnalysisTagger.Application.DTOs;

public class PlaylistDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<PlaylistEntryDto> Entries { get; set; } = new();
    public TimeSpan TotalDuration { get; set; }
}
