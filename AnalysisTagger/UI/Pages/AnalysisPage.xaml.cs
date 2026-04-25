using AnalysisTagger.UI.ViewModels;

namespace AnalysisTagger.UI.Pages;

public partial class AnalysisPage : ContentPage
{
    public AnalysisPage(AnalysisViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
