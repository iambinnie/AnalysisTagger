using AnalysisTagger.Application.DTOs;
using AnalysisTagger.Application.Interfaces;
using AnalysisTagger.Application.Services;
using AnalysisTagger.Domain.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AnalysisTagger.UI.ViewModels;

[QueryProperty(nameof(TemplateId), "templateId")]
public partial class TemplateEditorViewModel : ObservableObject
{
    private readonly TemplateService _templateService;
    private readonly INavigationService _navigation;
    private readonly IDialogService _dialog;

    [ObservableProperty] private string _templateId = string.Empty;
    [ObservableProperty] private string _templateName = string.Empty;
    [ObservableProperty] private SportType _selectedSport = SportType.Generic;
    [ObservableProperty] private ObservableCollection<CategoryEditorDto> _categories = [];
    [ObservableProperty] private bool _isBuiltIn;

    public List<SportType> SportTypes { get; } = Enum.GetValues<SportType>().ToList();

    public TemplateEditorViewModel(TemplateService templateService, INavigationService navigation, IDialogService dialog)
    {
        _templateService = templateService;
        _navigation = navigation;
        _dialog = dialog;
    }

    partial void OnTemplateIdChanged(string value)
    {
        if (Guid.TryParse(value, out _))
            _ = LoadTemplateAsync();
    }

    [RelayCommand]
    private async Task LoadTemplateAsync()
    {
        var (template, cats) = await _templateService.GetTemplateDetailAsync(Guid.Parse(TemplateId));
        TemplateName = template.Name;
        SelectedSport = template.Sport;
        IsBuiltIn = template.IsBuiltIn;
        Categories = new ObservableCollection<CategoryEditorDto>(cats);
    }

    [RelayCommand]
    private async Task SaveTemplateAsync()
    {
        if (string.IsNullOrWhiteSpace(TemplateName)) return;

        await _templateService.UpdateTemplateAsync(Guid.Parse(TemplateId), new UpdateTemplateDto
        {
            Name = TemplateName.Trim(),
            Sport = SelectedSport
        });

        await _dialog.AlertAsync("Saved", "Template updated successfully.");
    }

    [RelayCommand]
    private Task AddCategoryAsync() =>
        _navigation.GoToAsync($"{nameof(Pages.AddEditCategoryPage)}?templateId={TemplateId}");

    [RelayCommand]
    private Task EditCategoryAsync(CategoryEditorDto category) =>
        _navigation.GoToAsync($"{nameof(Pages.AddEditCategoryPage)}?templateId={TemplateId}&categoryId={category.Id}");

    [RelayCommand]
    private async Task DeleteCategoryAsync(CategoryEditorDto category)
    {
        var confirmed = await _dialog.ConfirmAsync(
            "Remove Category",
            $"Remove '{category.Name}' from this template?");
        if (!confirmed) return;

        await _templateService.RemoveCategoryAsync(Guid.Parse(TemplateId), category.Id);
        await LoadTemplateAsync();
    }
}
