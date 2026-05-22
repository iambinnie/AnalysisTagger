using AnalysisTagger.Application.Projections;
using AnalysisTagger.Services;
using AnalysisTagger.UI.Graphics;
using AnalysisTagger.UI.ViewModels;
using System.ComponentModel;

namespace AnalysisTagger.UI.Pages;

public partial class AnalysisPage : ContentPage
{
    private readonly AnalysisViewModel _viewModel;
    private readonly MediaElementVideoPlayer _videoPlayer;
    private readonly TimelineDrawable _timelineDrawable = new();
    private bool _initialNavigationComplete;

    public AnalysisPage(AnalysisViewModel viewModel, MediaElementVideoPlayer videoPlayer)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        _videoPlayer = videoPlayer;
        TimelineView.Drawable = _timelineDrawable;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _videoPlayer.Attach(VideoElement);
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;

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
        _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        _videoPlayer.Detach();
        _viewModel.Dispose();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(AnalysisViewModel.PositionSeconds):
                _timelineDrawable.PositionSeconds = _viewModel.PositionSeconds;
                TimelineView.Invalidate();
                break;

            case nameof(AnalysisViewModel.DurationSeconds):
                _timelineDrawable.DurationSeconds = _viewModel.DurationSeconds;
                TimelineView.Invalidate();
                break;

            case nameof(AnalysisViewModel.Events):
                _timelineDrawable.Segments = TimelineSegmentBuilder.Build(_viewModel.Events);
                TimelineView.Invalidate();
                break;
        }
    }

    private void OnTimelineStartInteraction(object? sender, TouchEventArgs e)
    {
        if (_viewModel.DurationSeconds <= 0) return;
        var touch = e.Touches.FirstOrDefault();
        var width = TimelineView.Width;
        if (width <= 0) return;
        var seconds = touch.X / width * _viewModel.DurationSeconds;
        _viewModel.SeekToSeconds(Math.Clamp(seconds, 0, _viewModel.DurationSeconds));
    }

    private void OnSeekDragStarted(object? sender, EventArgs e) =>
        _viewModel.IsSeeking = true;

    private void OnSeekDragCompleted(object? sender, EventArgs e)
    {
        _viewModel.IsSeeking = false;
        _viewModel.SeekToSeconds(SeekSlider.Value);
    }
}
