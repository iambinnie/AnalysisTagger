using AnalysisTagger.Application.DTOs;

namespace AnalysisTagger.Application.Projections;

public static class TimelineSegmentBuilder
{
    public static IReadOnlyList<TimelineTrack> Build(
        IEnumerable<CategoryDto> categories,
        IEnumerable<EventTagDto> events)
    {
        var grouped = events
            .GroupBy(e => e.CategoryId)
            .ToDictionary(g => g.Key, g => g.ToList());

        return categories
            .Select(cat =>
            {
                var segments = grouped.TryGetValue(cat.Id, out var catEvents)
                    ? catEvents
                        .Select(e => new TimelineSegmentData(
                            e.StartTime.Value.TotalSeconds,
                            e.EndTime.Value.TotalSeconds))
                        .ToList()
                    : [];

                return new TimelineTrack(cat.Id, cat.Name, cat.Color, segments);
            })
            .ToList();
    }
}
