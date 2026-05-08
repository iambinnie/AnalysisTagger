using AnalysisTagger.Application.DTOs;
using AnalysisTagger.Application.Exceptions;
using AnalysisTagger.Application.Interfaces;
using AnalysisTagger.Domain.Models;

namespace AnalysisTagger.Application.Services;

public class ProjectService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProjectService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<ProjectDto> CreateProjectAsync(CreateProjectDto dto, CancellationToken cancellationToken = default)
    {
        var project = new Project
        {
            Title = dto.Title,
            Competition = dto.Competition,
            Season = dto.Season,
            MatchDate = dto.MatchDate,
            Sport = dto.Sport,
            TaggingMode = dto.TaggingMode,
            VideoFilePath = dto.VideoFilePath,
            Template = TagTemplate.CreateDefault(dto.Sport)
        };

        await _unitOfWork.Projects.AddAsync(project, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(project);
    }

    public async Task<ProjectDto> GetProjectAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(id, cancellationToken)
            ?? throw new ProjectNotFoundException(id);
        return MapToDto(project);
    }

    public async Task<IEnumerable<ProjectDto>> GetAllProjectsAsync(CancellationToken cancellationToken = default)
    {
        var projects = await _unitOfWork.Projects.GetAllAsync(cancellationToken);
        return projects.Select(MapToDto);
    }

    public async Task UpdateProjectAsync(ProjectDto dto, CancellationToken cancellationToken = default)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(dto.Id, cancellationToken)
            ?? throw new ProjectNotFoundException(dto.Id);

        project.Title = dto.Title;
        project.Competition = dto.Competition;
        project.Season = dto.Season;
        project.MatchDate = dto.MatchDate;
        project.VideoFilePath = dto.VideoFilePath;

        await _unitOfWork.Projects.UpdateAsync(project, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task AssignTemplateAsync(Guid projectId, Guid libraryTemplateId, CancellationToken cancellationToken = default)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(projectId, cancellationToken)
            ?? throw new ProjectNotFoundException(projectId);
        var library = await _unitOfWork.Templates.GetByIdAsync(libraryTemplateId, cancellationToken)
            ?? throw new TemplateNotFoundException(libraryTemplateId);

        project.Template.Name = library.Name;
        project.Template.Sport = library.Sport;
        project.Template.Categories.Clear();

        foreach (var cat in library.Categories.OrderBy(c => c.SortOrder))
        {
            var copy = new Category
            {
                Name = cat.Name,
                Color = cat.Color,
                DefaultLeadTime = cat.DefaultLeadTime,
                DefaultLagTime = cat.DefaultLagTime,
                SubCategories = cat.SubCategories.ToList(),
                SortOrder = cat.SortOrder
            };
            project.Template.Categories.Add(copy);
            _unitOfWork.Projects.TrackNewCategory(copy);
        }

        project.LastModifiedAt = DateTime.UtcNow;
        await _unitOfWork.Projects.UpdateAsync(project, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteProjectAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _ = await _unitOfWork.Projects.GetByIdAsync(id, cancellationToken)
            ?? throw new ProjectNotFoundException(id);

        await _unitOfWork.Projects.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static ProjectDto MapToDto(Project project) => new()
    {
        Id = project.Id,
        Title = project.Title,
        Competition = project.Competition,
        Season = project.Season,
        MatchDate = project.MatchDate,
        Sport = project.Sport,
        TaggingMode = project.TaggingMode,
        VideoFilePath = project.VideoFilePath,
        CreatedAt = project.CreatedAt,
        LastModifiedAt = project.LastModifiedAt,
        TemplateId = project.Template?.Id ?? Guid.Empty,
        TemplateName = project.Template?.Name ?? string.Empty,
        EventCount = project.Events.Count
    };
}
