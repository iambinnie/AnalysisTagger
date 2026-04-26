using AnalysisTagger.UI.ViewModels;

namespace AnalysisTagger.UI.Pages;

public partial class TeamRosterPage : ContentPage
{
    private readonly TeamRosterViewModel _viewModel;

    public TeamRosterPage(TeamRosterViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (!string.IsNullOrEmpty(_viewModel.TeamId))
            _viewModel.LoadRosterCommand.Execute(null);
    }
}
