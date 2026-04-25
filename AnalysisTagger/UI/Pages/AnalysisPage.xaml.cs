using AnalysisTagger.Services;
using AnalysisTagger.UI.ViewModels;

namespace AnalysisTagger.UI.Pages;

public partial class AnalysisPage : ContentPage
{
    private readonly AnalysisViewModel _viewModel;
    private readonly MediaElementVideoPlayer _videoPlayer;

    public AnalysisPage(AnalysisViewModel viewModel, MediaElementVideoPlayer videoPlayer)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        _videoPlayer = videoPlayer;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _videoPlayer.Attach(VideoElement);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _videoPlayer.Detach();
        _viewModel.Dispose();
    }

    private void OnSeekDragStarted(object? sender, EventArgs e) =>
        _viewModel.IsSeeking = true;

    private void OnSeekDragCompleted(object? sender, EventArgs e)
    {
        _viewModel.IsSeeking = false;
        _viewModel.SeekToSeconds(SeekSlider.Value);
    }
}
