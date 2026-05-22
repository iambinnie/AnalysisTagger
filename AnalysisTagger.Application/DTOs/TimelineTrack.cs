namespace AnalysisTagger.Application.DTOs;

public record TimelineTrack(
    Guid CategoryId,
    string CategoryName,
    string ColorHex,
    IReadOnlyList<TimelineSegmentData> Segments);
