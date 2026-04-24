using AnalysisTagger.Application.DTOs;
using AnalysisTagger.Application.Exceptions;
using AnalysisTagger.Application.Interfaces;
using AnalysisTagger.Domain.Models;

namespace AnalysisTagger.Application.Services;

public class PlaylistService
{
    private readonly IUnitOfWork _unitOfWork;

    public PlaylistService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<PlaylistDto> CreatePlaylistAsync(Guid projectId, string name, CancellationToken cancellationToken = default)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(projectId, cancellationToken)
            ?? throw new ProjectNotFoundException(projectId);

        var playlist = new Playlist { Name = name };
        project.Playlists.Add(playlist);

        await _unitOfWork.Projects.UpdateAsync(project, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(playlist);
    }

    public async Task AddTagToPlaylistAsync(Guid projectId, Guid playlistId, Guid tagId, CancellationToken cancellationToken = default)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(projectId, cancellationToken)
            ?? throw new ProjectNotFoundException(projectId);

        var playlist = project.Playlists.FirstOrDefault(p => p.Id == playlistId)
            ?? throw new InvalidTagException($"Playlist '{playlistId}' not found in project '{projectId}'.");

        var tag = project.Events.FirstOrDefault(e => e.Id == tagId)
            ?? throw new InvalidTagException($"Tag '{tagId}' not found in project '{projectId}'.");

        playlist.AddEntry(tag);

        await _unitOfWork.Projects.UpdateAsync(project, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveEntryFromPlaylistAsync(Guid projectId, Guid playlistId, Guid entryId, CancellationToken cancellationToken = default)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(projectId, cancellationToken)
            ?? throw new ProjectNotFoundException(projectId);

        var playlist = project.Playlists.FirstOrDefault(p => p.Id == playlistId)
            ?? throw new InvalidTagException($"Playlist '{playlistId}' not found in project '{projectId}'.");

        var entry = playlist.Entries.FirstOrDefault(e => e.Id == entryId)
            ?? throw new InvalidTagException($"Entry '{entryId}' not found in playlist '{playlistId}'.");

        playlist.Entries.Remove(entry);

        await _unitOfWork.Projects.UpdateAsync(project, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ReorderEntryAsync(Guid projectId, Guid playlistId, Guid entryId, int newPosition, CancellationToken cancellationToken = default)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(projectId, cancellationToken)
            ?? throw new ProjectNotFoundException(projectId);

        var playlist = project.Playlists.FirstOrDefault(p => p.Id == playlistId)
            ?? throw new InvalidTagException($"Playlist '{playlistId}' not found in project '{projectId}'.");

        playlist.Reorder(entryId, newPosition);

        await _unitOfWork.Projects.UpdateAsync(project, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static PlaylistDto MapToDto(Playlist playlist) => new()
    {
        Id = playlist.Id,
        Name = playlist.Name,
        TotalDuration = playlist.TotalDuration,
        Entries = playlist.Entries.Select(e => new PlaylistEntryDto
        {
            Id = e.Id,
            SortOrder = e.SortOrder,
            HasSegmentOverride = e.SegmentOverride is not null,
            Tag = TaggingService.MapToDto(e.Tag)
        }).ToList()
    };
}
