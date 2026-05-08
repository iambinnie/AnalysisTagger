using AnalysisTagger.Application.DTOs;
using AnalysisTagger.Application.Interfaces;
using AnalysisTagger.Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AnalysisTagger.UI.ViewModels;

[QueryProperty(nameof(TemplateId), "templateId")]
[QueryProperty(nameof(CategoryId), "categoryId")]
public partial class AddEditCategoryViewModel : ObservableObject
{
    private readonly TemplateService _templateService;
    private readonly INavigationService _navigation;

    [ObservableProperty] private string _templateId = string.Empty;
    [ObservableProperty] private string _categoryId = string.Empty;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _color = "#3498DB";
    [ObservableProperty] private string _leadTimeText = "2";
    [ObservableProperty] private string _lagTimeText = "3";
    [ObservableProperty] private string _subCategoriesText = string.Empty;

    public bool IsEditing => !string.IsNullOrEmpty(CategoryId);

    public AddEditCategoryViewModel(TemplateService templateService, INavigationService navigation)
    {
        _templateService = templateService;
        _navigation = navigation;
    }

    partial void OnCategoryIdChanged(string value)
    {
        if (Guid.TryParse(value, out _))
            _ = LoadCategoryAsync();
    }

    [RelayCommand]
    private async Task LoadCategoryAsync()
    {
        var (_, cats) = await _templateService.GetTemplateDetailAsync(Guid.Parse(TemplateId));
        var cat = cats.FirstOrDefault(c => c.Id == Guid.Parse(CategoryId));
        if (cat is null) return;

        Name = cat.Name;
        Color = cat.Color;
        LeadTimeText = cat.LeadTimeSeconds.ToString();
        LagTimeText = cat.LagTimeSeconds.ToString();
        SubCategoriesText = string.Join(", ", cat.SubCategories);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name)) return;

        var leadSecs = int.TryParse(LeadTimeText, out var l) ? l : 2;
        var lagSecs = int.TryParse(LagTimeText, out var g) ? g : 3;
        var subCats = SubCategoriesText
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        if (IsEditing)
        {
            await _templateService.UpdateCategoryAsync(
                Guid.Parse(TemplateId),
                Guid.Parse(CategoryId),
                new UpdateCategoryDto
                {
                    Name = Name.Trim(),
                    Color = Color.Trim(),
                    LeadTimeSeconds = leadSecs,
                    LagTimeSeconds = lagSecs,
                    SubCategories = subCats
                });
        }
        else
        {
            await _templateService.AddCategoryAsync(
                Guid.Parse(TemplateId),
                new CreateCategoryDto
                {
                    Name = Name.Trim(),
                    Color = Color.Trim(),
                    LeadTimeSeconds = leadSecs,
                    LagTimeSeconds = lagSecs,
                    SubCategories = subCats
                });
        }

        await _navigation.GoBackAsync();
    }

    [RelayCommand]
    private Task CancelAsync() => _navigation.GoBackAsync();
}
