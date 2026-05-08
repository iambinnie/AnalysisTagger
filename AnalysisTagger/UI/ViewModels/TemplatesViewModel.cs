using AnalysisTagger.Application.DTOs;
using AnalysisTagger.Application.Interfaces;
using AnalysisTagger.Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AnalysisTagger.UI.ViewModels;

public partial class TemplatesViewModel : ObservableObject
{
    private readonly TemplateService _templateService;
    private readonly INavigationService _navigation;
    private readonly IDialogService _dialog;

    [ObservableProperty] private ObservableCollection<TemplateDto> _templates = [];
    [ObservableProperty] private bool _isLoading;

    public TemplatesViewModel(TemplateService templateService, INavigationService navigation, IDialogService dialog)
    {
        _templateService = templateService;
        _navigation = navigation;
        _dialog = dialog;
    }

    [RelayCommand]
    private async Task LoadTemplatesAsync()
    {
        IsLoading = true;
        try
        {
            var list = await _templateService.GetAllTemplatesAsync();
            Templates = new ObservableCollection<TemplateDto>(list);
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task CreateTemplateAsync()
    {
        var name = await _dialog.PromptAsync("New Template", "Enter template name:");
        if (string.IsNullOrWhiteSpace(name)) return;

        var dto = await _templateService.CreateTemplateAsync(new CreateTemplateDto { Name = name.Trim() });
        await _navigation.GoToAsync($"{nameof(Pages.TemplateEditorPage)}?templateId={dto.Id}");
        await LoadTemplatesAsync();
    }

    [RelayCommand]
    private Task OpenTemplateAsync(TemplateDto template) =>
        _navigation.GoToAsync($"{nameof(Pages.TemplateEditorPage)}?templateId={template.Id}");

    [RelayCommand]
    private async Task DeleteTemplateAsync(TemplateDto template)
    {
        if (template.IsBuiltIn)
        {
            await _dialog.AlertAsync("Cannot Delete", "Built-in templates cannot be deleted.");
            return;
        }

        var confirmed = await _dialog.ConfirmAsync(
            "Delete Template",
            $"Delete '{template.Name}'? This cannot be undone.");
        if (!confirmed) return;

        await _templateService.DeleteTemplateAsync(template.Id);
        await LoadTemplatesAsync();
    }
}
