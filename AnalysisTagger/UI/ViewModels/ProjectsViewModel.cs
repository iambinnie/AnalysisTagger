using AnalysisTagger.Application.DTOs;
using AnalysisTagger.Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AnalysisTagger.UI.ViewModels;

public partial class ProjectsViewModel : ObservableObject
{
    private readonly ProjectService _projectService;

    [ObservableProperty]
    private ObservableCollection<ProjectDto> _projects = [];

    [ObservableProperty]
    private bool _isLoading;

    public ProjectsViewModel(ProjectService projectService)
    {
        _projectService = projectService;
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
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task CreateProjectAsync()
    {
        var name = await Shell.Current.DisplayPromptAsync(
            "New Project", "Enter project name:", maxLength: 100);

        if (string.IsNullOrWhiteSpace(name)) return;

        await _projectService.CreateProjectAsync(new CreateProjectDto { Title = name.Trim() });
        await LoadProjectsAsync();
    }

    [RelayCommand]
    private async Task OpenProjectAsync(ProjectDto project)
    {
        await Shell.Current.GoToAsync($"AnalysisPage?projectId={project.Id}");
    }
}
