using AnalysisTagger.UI.ViewModels;

namespace AnalysisTagger.UI.Pages;

public partial class AddEditPlayerPage : ContentPage
{
    public AddEditPlayerPage(AddEditPlayerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
