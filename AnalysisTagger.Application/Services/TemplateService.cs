using AnalysisTagger.Application.DTOs;
using AnalysisTagger.Application.Exceptions;
using AnalysisTagger.Application.Interfaces;
using AnalysisTagger.Domain.Models;

namespace AnalysisTagger.Application.Services;

public class TemplateService
{
    private readonly IUnitOfWork _unitOfWork;

    public TemplateService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<IEnumerable<TemplateDto>> GetAllTemplatesAsync(CancellationToken cancellationToken = default)
    {
        var templates = await _unitOfWork.Templates.GetAllAsync(cancellationToken);
        return templates.Select(MapToDto);
    }

    public async Task<(TemplateDto Template, IEnumerable<CategoryEditorDto> Categories)> GetTemplateDetailAsync(
        Guid templateId, CancellationToken cancellationToken = default)
    {
        var template = await _unitOfWork.Templates.GetByIdAsync(templateId, cancellationToken)
            ?? throw new TemplateNotFoundException(templateId);
        return (MapToDto(template), template.Categories.OrderBy(c => c.SortOrder).Select(MapCategoryToDto));
    }

    public async Task<TemplateDto> CreateTemplateAsync(CreateTemplateDto dto, CancellationToken cancellationToken = default)
    {
        var template = new TagTemplate { Name = dto.Name.Trim(), Sport = dto.Sport };
        await _unitOfWork.Templates.AddAsync(template, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(template);
    }

    public async Task UpdateTemplateAsync(Guid templateId, UpdateTemplateDto dto, CancellationToken cancellationToken = default)
    {
        var template = await _unitOfWork.Templates.GetByIdAsync(templateId, cancellationToken)
            ?? throw new TemplateNotFoundException(templateId);
        template.Name = dto.Name.Trim();
        template.Sport = dto.Sport;
        await _unitOfWork.Templates.UpdateAsync(template, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteTemplateAsync(Guid templateId, CancellationToken cancellationToken = default)
    {
        var template = await _unitOfWork.Templates.GetByIdAsync(templateId, cancellationToken)
            ?? throw new TemplateNotFoundException(templateId);
        if (template.IsBuiltIn)
            throw new InvalidOperationException("Built-in templates cannot be deleted.");
        await _unitOfWork.Templates.DeleteAsync(templateId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<CategoryEditorDto> AddCategoryAsync(Guid templateId, CreateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var template = await _unitOfWork.Templates.GetByIdAsync(templateId, cancellationToken)
            ?? throw new TemplateNotFoundException(templateId);

        var category = new Category
        {
            Name = dto.Name.Trim(),
            Color = dto.Color,
            DefaultLeadTime = TimeSpan.FromSeconds(dto.LeadTimeSeconds),
            DefaultLagTime = TimeSpan.FromSeconds(dto.LagTimeSeconds),
            SubCategories = dto.SubCategories,
            SortOrder = template.Categories.Count + 1
        };
        template.Categories.Add(category);
        _unitOfWork.Templates.TrackNewCategory(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapCategoryToDto(category);
    }

    public async Task UpdateCategoryAsync(Guid templateId, Guid categoryId, UpdateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var template = await _unitOfWork.Templates.GetByIdAsync(templateId, cancellationToken)
            ?? throw new TemplateNotFoundException(templateId);

        var category = template.Categories.FirstOrDefault(c => c.Id == categoryId)
            ?? throw new InvalidOperationException($"Category '{categoryId}' not found in template '{templateId}'.");

        category.Name = dto.Name.Trim();
        category.Color = dto.Color;
        category.DefaultLeadTime = TimeSpan.FromSeconds(dto.LeadTimeSeconds);
        category.DefaultLagTime = TimeSpan.FromSeconds(dto.LagTimeSeconds);
        category.SubCategories = dto.SubCategories;
        await _unitOfWork.Templates.UpdateAsync(template, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveCategoryAsync(Guid templateId, Guid categoryId, CancellationToken cancellationToken = default)
    {
        var template = await _unitOfWork.Templates.GetByIdAsync(templateId, cancellationToken)
            ?? throw new TemplateNotFoundException(templateId);

        var category = template.Categories.FirstOrDefault(c => c.Id == categoryId)
            ?? throw new InvalidOperationException($"Category '{categoryId}' not found in template '{templateId}'.");

        template.Categories.Remove(category);
        await _unitOfWork.Templates.UpdateAsync(template, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    internal static TemplateDto MapToDto(TagTemplate template) => new()
    {
        Id = template.Id,
        Name = template.Name,
        Sport = template.Sport,
        CategoryCount = template.Categories.Count,
        IsBuiltIn = template.IsBuiltIn
    };

    internal static CategoryEditorDto MapCategoryToDto(Category category) => new()
    {
        Id = category.Id,
        Name = category.Name,
        Color = category.Color,
        LeadTimeSeconds = (int)category.DefaultLeadTime.TotalSeconds,
        LagTimeSeconds = (int)category.DefaultLagTime.TotalSeconds,
        SubCategories = category.SubCategories,
        SortOrder = category.SortOrder
    };
}
