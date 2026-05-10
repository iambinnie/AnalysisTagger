using AnalysisTagger.Services;
using AnalysisTagger.UI.ViewModels;

namespace AnalysisTagger.UI.Pages;

public partial class AnalysisPage : ContentPage
{
    private readonly AnalysisViewModel _viewModel;
    private readonly MediaElementVideoPlayer _videoPlayer;
    private bool _initialNavigationComplete;

    public AnalysisPage(AnalysisViewModel viewModel, MediaElementVideoPlayer videoPlayer)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        _videoPlayer = videoPlayer;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _videoPlayer.Attach(VideoElement);

        await _viewModel.LoadProjectAsync();

        if (!_initialNavigationComplete)
        {
            _initialNavigationComplete = true;
            _viewModel.AutoLoadVideo();
        }
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
