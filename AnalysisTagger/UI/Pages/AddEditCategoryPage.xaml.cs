using AnalysisTagger.UI.ViewModels;

namespace AnalysisTagger.UI.Pages;

public partial class AddEditCategoryPage : ContentPage
{
    public AddEditCategoryPage(AddEditCategoryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
