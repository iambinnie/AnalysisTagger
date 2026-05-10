using AnalysisTagger.UI.ViewModels;

namespace AnalysisTagger.UI.Pages;

public partial class ProjectSetupPage : ContentPage
{
    private readonly ProjectSetupViewModel _viewModel;

    public ProjectSetupPage(ProjectSetupViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadCommand.ExecuteAsync(null);
    }
}
