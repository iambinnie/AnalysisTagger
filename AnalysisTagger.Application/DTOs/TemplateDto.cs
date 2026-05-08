using AnalysisTagger.Domain.Enums;

namespace AnalysisTagger.Application.DTOs;

public class TemplateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public SportType Sport { get; set; }
    public int CategoryCount { get; set; }
    public bool IsBuiltIn { get; set; }
}

public class CreateTemplateDto
{
    public string Name { get; set; } = string.Empty;
    public SportType Sport { get; set; } = SportType.Generic;
}

public class UpdateTemplateDto
{
    public string Name { get; set; } = string.Empty;
    public SportType Sport { get; set; }
}

public class CategoryEditorDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#3498DB";
    public int LeadTimeSeconds { get; set; } = 2;
    public int LagTimeSeconds { get; set; } = 3;
    public List<string> SubCategories { get; set; } = new();
    public int SortOrder { get; set; }
}

public class CreateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#3498DB";
    public int LeadTimeSeconds { get; set; } = 2;
    public int LagTimeSeconds { get; set; } = 3;
    public List<string> SubCategories { get; set; } = new();
}

public class UpdateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#3498DB";
    public int LeadTimeSeconds { get; set; } = 2;
    public int LagTimeSeconds { get; set; } = 3;
    public List<string> SubCategories { get; set; } = new();
}
