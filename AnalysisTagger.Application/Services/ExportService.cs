using AnalysisTagger.Application.Exceptions;
using AnalysisTagger.Application.Interfaces;

namespace AnalysisTagger.Application.Services;

public class ExportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IVideoExporter _exporter;

    public ExportService(IUnitOfWork unitOfWork, IVideoExporter exporter)
    {
        _unitOfWork = unitOfWork;
        _exporter = exporter;
    }

    public async Task ExportTagAsync(Guid projectId, Guid tagId, string outputPath, CancellationToken cancellationToken = default)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(projectId, cancellationToken)
            ?? throw new ProjectNotFoundException(projectId);

        if (!File.Exists(project.VideoFilePath))
            throw new VideoFileNotFoundException(project.VideoFilePath);

        var tag = project.Events.FirstOrDefault(e => e.Id == tagId)
            ?? throw new InvalidTagException($"Tag '{tagId}' not found in project '{projectId}'.");

        await _exporter.ExportSegmentAsync(project.VideoFilePath, tag.Segment, outputPath, cancellationToken);
    }

    public async Task ExportPlaylistAsync(Guid projectId, Guid playlistId, string outputPath, CancellationToken cancellationToken = default)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(projectId, cancellationToken)
            ?? throw new ProjectNotFoundException(projectId);

        if (!File.Exists(project.VideoFilePath))
            throw new VideoFileNotFoundException(project.VideoFilePath);

        var playlist = project.Playlists.FirstOrDefault(p => p.Id == playlistId)
            ?? throw new InvalidTagException($"Playlist '{playlistId}' not found in project '{projectId}'.");

        var segments = playlist.Entries
            .OrderBy(e => e.SortOrder)
            .Select(e => e.EffectiveSegment)
            .ToList();

        await _exporter.ExportPlaylistAsync(project.VideoFilePath, segments, outputPath, cancellationToken);
    }
}
