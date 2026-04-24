using AnalysisTagger.Domain.ValueObjects;

namespace AnalysisTagger.Application.Interfaces;

public interface IVideoExporter
{
    Task ExportSegmentAsync(
        string sourceFilePath,
        VideoSegment segment,
        string outputPath,
        CancellationToken cancellationToken = default);

    Task ExportPlaylistAsync(
        string sourceFilePath,
        IEnumerable<VideoSegment> segments,
        string outputPath,
        CancellationToken cancellationToken = default);
}
