using CommunityToolkit.Mvvm.Input;

namespace AnalysisTagger.UI.ViewModels;

public class SubCategoryItem
{
    public string Name { get; }
    public IAsyncRelayCommand TagCommand { get; }

    public SubCategoryItem(string name, IAsyncRelayCommand tagCommand)
    {
        Name = name;
        TagCommand = tagCommand;
    }
}

public class SubCategoryRow
{
    public SubCategoryItem? Left { get; }
    public SubCategoryItem? Right { get; }
    public bool HasRight => Right is not null;

    public SubCategoryRow(SubCategoryItem? left, SubCategoryItem? right)
    {
        Left = left;
        Right = right;
    }
}

public class CategoryButtonViewModel
{
    public string Name { get; }
    public string Color { get; }
    public IAsyncRelayCommand TagCommand { get; }
    public List<SubCategoryRow> SubCategoryRows { get; }
    public bool HasSubCategories => SubCategoryRows.Count > 0;

    public CategoryButtonViewModel(
        string name,
        string color,
        IAsyncRelayCommand tagCommand,
        List<SubCategoryRow> subCategoryRows)
    {
        Name = name;
        Color = color;
        TagCommand = tagCommand;
        SubCategoryRows = subCategoryRows;
    }
}
