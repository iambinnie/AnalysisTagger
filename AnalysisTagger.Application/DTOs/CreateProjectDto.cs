using AnalysisTagger.Domain.Enums;

namespace AnalysisTagger.Application.DTOs;

public class CreateProjectDto
{
    public string Title { get; set; } = string.Empty;
    public string Competition { get; set; } = string.Empty;
    public string Season { get; set; } = string.Empty;
    public DateTime MatchDate { get; set; } = DateTime.Today;
    public SportType Sport { get; set; } = SportType.Generic;
    public TaggingMode TaggingMode { get; set; } = TaggingMode.PostTagging;
    public string VideoFilePath { get; set; } = string.Empty;
}
