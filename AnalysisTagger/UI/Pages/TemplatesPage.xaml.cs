using AnalysisTagger.UI.ViewModels;

namespace AnalysisTagger.UI.Pages;

public partial class TemplatesPage : ContentPage
{
    private readonly TemplatesViewModel _viewModel;

    public TemplatesPage(TemplatesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadTemplatesCommand.Execute(null);
    }
}
