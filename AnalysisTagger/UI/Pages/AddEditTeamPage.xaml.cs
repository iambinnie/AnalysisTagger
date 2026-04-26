using AnalysisTagger.UI.ViewModels;

namespace AnalysisTagger.UI.Pages;

public partial class AddEditTeamPage : ContentPage
{
    public AddEditTeamPage(AddEditTeamViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
