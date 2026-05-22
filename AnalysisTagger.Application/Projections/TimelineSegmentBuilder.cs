using AnalysisTagger.Application.DTOs;

namespace AnalysisTagger.Application.Projections;

public static class TimelineSegmentBuilder
{
    public static IReadOnlyList<TimelineSegmentData> Build(IEnumerable<EventTagDto> events) =>
        events
            .Select(e => new TimelineSegmentData(
                e.StartTime.Value.TotalSeconds,
                e.EndTime.Value.TotalSeconds,
                e.CategoryColor))
            .ToList();
}
