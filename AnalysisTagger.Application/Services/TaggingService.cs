using AnalysisTagger.Application.DTOs;
using AnalysisTagger.Application.Exceptions;
using AnalysisTagger.Application.Interfaces;
using AnalysisTagger.Domain.Models;
using AnalysisTagger.Domain.ValueObjects;

namespace AnalysisTagger.Application.Services;

public class TaggingService
{
    private readonly IUnitOfWork _unitOfWork;

    public TaggingService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<EventTagDto> TagEventAsync(Guid projectId, CreateEventTagDto dto, CancellationToken cancellationToken = default)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(projectId, cancellationToken)
            ?? throw new ProjectNotFoundException(projectId);

        var category = project.Template.Categories.FirstOrDefault(c => c.Id == dto.CategoryId)
            ?? throw new InvalidTagException($"Category '{dto.CategoryId}' not found in the project template.");

        // Clamp start to zero so tags near the beginning of the video don't produce negative timecodes
        var start = dto.Position.Add(-category.DefaultLeadTime);
        if (start.IsBefore(Timecode.Zero)) start = Timecode.Zero;
        var end = dto.Position.Add(category.DefaultLagTime);

        var players = project.HomeTeam.Players
            .Concat(project.AwayTeam.Players)
            .Where(p => dto.PlayerIds.Contains(p.Id))
            .ToList();

        var tag = new EventTag
        {
            Segment = new VideoSegment(start, end),
            Category = category,
            SubCategory = dto.SubCategory,
            Notes = dto.Notes,
            TaggedPlayers = players
        };

        project.AddEvent(tag);
        await _unitOfWork.Projects.UpdateAsync(project, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(tag);
    }

    public async Task DeleteTagAsync(Guid projectId, Guid tagId, CancellationToken cancellationToken = default)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(projectId, cancellationToken)
            ?? throw new ProjectNotFoundException(projectId);

        var tag = project.Events.FirstOrDefault(e => e.Id == tagId)
            ?? throw new InvalidTagException($"Tag '{tagId}' not found in project '{projectId}'.");

        project.Events.Remove(tag);
        await _unitOfWork.Projects.UpdateAsync(project, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    internal static EventTagDto MapToDto(EventTag tag) => new()
    {
        Id = tag.Id,
        CategoryId = tag.Category.Id,
        CategoryName = tag.Category.Name,
        CategoryColor = tag.Category.Color,
        StartTime = tag.StartTime,
        EndTime = tag.EndTime,
        Duration = tag.Duration,
        SubCategory = tag.SubCategory,
        Notes = tag.Notes,
        TaggedPlayerIds = tag.TaggedPlayers.Select(p => p.Id).ToList(),
        CreatedAt = tag.CreatedAt
    };
}
