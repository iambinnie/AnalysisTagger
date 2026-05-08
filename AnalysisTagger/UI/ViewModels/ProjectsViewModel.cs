using AnalysisTagger.Application.DTOs;
using AnalysisTagger.Application.Interfaces;
using AnalysisTagger.Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AnalysisTagger.UI.ViewModels;

public partial class ProjectsViewModel : ObservableObject
{
    private readonly ProjectService _projectService;
    private readonly INavigationService _navigation;
    private readonly IDialogService _dialog;

    [ObservableProperty] private ObservableCollection<ProjectDto> _projects = [];
    [ObservableProperty] private bool _isLoading;

    public ProjectsViewModel(ProjectService projectService, INavigationService navigation, IDialogService dialog)
    {
        _projectService = projectService;
        _navigation = navigation;
        _dialog = dialog;
    }

    [RelayCommand]
    private async Task LoadProjectsAsync()
    {
        IsLoading = true;
        try
        {
            var list = await _projectService.GetAllProjectsAsync();
            Projects = new ObservableCollection<ProjectDto>(list);
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private async Task CreateProjectAsync()
    {
        var name = await _dialog.PromptAsync("New Project", "Enter project name:");
        if (string.IsNullOrWhiteSpace(name)) return;

        await _projectService.CreateProjectAsync(new CreateProjectDto { Title = name.Trim() });
        await LoadProjectsAsync();
    }

    [RelayCommand]
    private Task OpenProjectAsync(ProjectDto project) =>
        _navigation.GoToAsync($"AnalysisPage?projectId={project.Id}");
}
