using AnalysisTagger.Application.DTOs;
using AnalysisTagger.UI.ViewModels;

namespace AnalysisTagger.UI.Pages;

public partial class ProjectsPage : ContentPage
{
    private readonly ProjectsViewModel _viewModel;

    public ProjectsPage(ProjectsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadProjectsCommand.ExecuteAsync(null);
    }

    private async void OnProjectSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not ProjectDto project) return;
        ProjectsCollection.SelectedItem = null;
        await _viewModel.OpenProjectCommand.ExecuteAsync(project);
    }
}
