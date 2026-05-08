namespace AnalysisTagger.Application.DTOs;

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#3498DB";
    public int SortOrder { get; set; }
    public List<string> SubCategories { get; set; } = new();
}
