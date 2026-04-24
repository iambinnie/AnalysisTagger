using AnalysisTagger.Domain.Enums;

namespace AnalysisTagger.Application.DTOs;

public class ProjectDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Competition { get; set; } = string.Empty;
    public string Season { get; set; } = string.Empty;
    public DateTime MatchDate { get; set; }
    public SportType Sport { get; set; }
    public TaggingMode TaggingMode { get; set; }
    public string VideoFilePath { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastModifiedAt { get; set; }
    public int EventCount { get; set; }
}
