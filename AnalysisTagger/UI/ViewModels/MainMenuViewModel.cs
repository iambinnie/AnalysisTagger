using AnalysisTagger.Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AnalysisTagger.UI.ViewModels;

public partial class MainMenuViewModel : ObservableObject
{
    private readonly INavigationService _navigation;

    public MainMenuViewModel(INavigationService navigation) => _navigation = navigation;

    [RelayCommand]
    private Task GoToProjectsAsync() => _navigation.GoToAsync(nameof(Pages.ProjectsPage));

    [RelayCommand]
    private Task GoToTeamsAsync() => _navigation.GoToAsync(nameof(Pages.TeamsPage));

    [RelayCommand]
    private Task GoToTemplatesAsync() => _navigation.GoToAsync(nameof(Pages.TemplatesPage));
}
