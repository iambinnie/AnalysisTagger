using AnalysisTagger.Application.Interfaces;
using AnalysisTagger.Domain.ValueObjects;

namespace AnalysisTagger.Tests.Application.Fakes;

public class FakeVideoExporter : IVideoExporter
{
    public List<(string Source, VideoSegment Segment, string Output)> ExportedSegments { get; } = new();
    public List<(string Source, List<VideoSegment> Segments, string Output)> ExportedPlaylists { get; } = new();

    public Task ExportSegmentAsync(string sourceFilePath, VideoSegment segment, string outputPath, CancellationToken cancellationToken = default)
    {
        ExportedSegments.Add((sourceFilePath, segment, outputPath));
        return Task.CompletedTask;
    }

    public Task ExportPlaylistAsync(string sourceFilePath, IEnumerable<VideoSegment> segments, string outputPath, CancellationToken cancellationToken = default)
    {
        ExportedPlaylists.Add((sourceFilePath, segments.ToList(), outputPath));
        return Task.CompletedTask;
    }
}
