using AnalysisTagger.UI.ViewModels;

namespace AnalysisTagger.UI.Pages;

public partial class TemplateEditorPage : ContentPage
{
    private readonly TemplateEditorViewModel _viewModel;

    public TemplateEditorPage(TemplateEditorViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (!string.IsNullOrEmpty(_viewModel.TemplateId))
            _viewModel.LoadTemplateCommand.Execute(null);
    }
}
