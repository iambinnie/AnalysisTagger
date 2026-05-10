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
    private Task CreateProjectAsync() =>
        _navigation.GoToAsync(nameof(Pages.ProjectSetupPage));

    [RelayCommand]
    private Task OpenProjectAsync(ProjectDto project) =>
        _navigation.GoToAsync($"{nameof(Pages.AnalysisPage)}?projectId={project.Id}");

    [RelayCommand]
    private Task EditProjectAsync(ProjectDto project) =>
        _navigation.GoToAsync($"{nameof(Pages.ProjectSetupPage)}?projectId={project.Id}");

    [RelayCommand]
    private async Task DeleteProjectAsync(ProjectDto project)
    {
        var confirmed = await _dialog.ConfirmAsync(
            "Delete Project",
            $"Delete '{project.Title}'? All tags will be lost. This cannot be undone.");
        if (!confirmed) return;

        await _projectService.DeleteProjectAsync(project.Id);
        await LoadProjectsAsync();
    }
}
