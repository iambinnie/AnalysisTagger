using AnalysisTagger.Application.Interfaces;
using AnalysisTagger.Domain.ValueObjects;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AnalysisTagger.UI.ViewModels;

[QueryProperty("ProjectId", "projectId")]
public partial class AnalysisViewModel : ObservableObject, IDisposable
{
    private readonly IVideoPlayer _videoPlayer;

    [ObservableProperty] private string _projectId = string.Empty;
    [ObservableProperty] private string _projectTitle = "Analysis";
    [ObservableProperty] private bool _isPlaying;
    [ObservableProperty] private bool _hasMedia;
    [ObservableProperty] private string _positionText = "00:00:00.00";
    [ObservableProperty] private string _durationText = "00:00:00.00";
    [ObservableProperty] private double _positionSeconds;
    [ObservableProperty] private double _durationSeconds = 1;

    public bool IsSeeking { get; set; }

    public AnalysisViewModel(IVideoPlayer videoPlayer)
    {
        _videoPlayer = videoPlayer;
        _videoPlayer.PositionChanged += OnPositionChanged;
        _videoPlayer.MediaLoaded += OnMediaLoaded;
        _videoPlayer.PlaybackEnded += OnPlaybackEnded;
    }

    [RelayCommand]
    private async Task LoadVideoAsync()
    {
        var result = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Select video file",
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.WinUI,         new[] { ".mp4", ".avi", ".mkv", ".mov", ".wmv" } },
                { DevicePlatform.Android,       new[] { "video/*" } },
                { DevicePlatform.iOS,           new[] { "public.movie" } },
                { DevicePlatform.MacCatalyst,   new[] { "public.movie" } },
            })
        });

        if (result == null) return;
        _videoPlayer.Load(result.FullPath);
    }

    [RelayCommand]
    private void PlayPause()
    {
        if (_videoPlayer.IsPlaying) _videoPlayer.Pause();
        else _videoPlayer.Play();
    }

    [RelayCommand]
    private void Stop() => _videoPlayer.Stop();

    [RelayCommand]
    private void StepForward()
    {
        var next = _videoPlayer.Position.Add(TimeSpan.FromMilliseconds(40));
        _videoPlayer.Seek(next.IsAfter(_videoPlayer.Duration) ? _videoPlayer.Duration : next);
    }

    [RelayCommand]
    private void StepBackward()
    {
        var prev = _videoPlayer.Position.Add(TimeSpan.FromMilliseconds(-40));
        _videoPlayer.Seek(prev.IsBefore(Timecode.Zero) ? Timecode.Zero : prev);
    }

    [RelayCommand]
    private void SetRate(string rateString)
    {
        if (double.TryParse(rateString, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var rate))
            _videoPlayer.SetSpeed(rate);
    }

    public void SeekToSeconds(double seconds) =>
        _videoPlayer.Seek(Timecode.FromSeconds(seconds));

    private void OnPositionChanged(object? sender, Timecode position)
    {
        if (IsSeeking) return;
        MainThread.BeginInvokeOnMainThread(() =>
        {
            PositionText = position.ToString();
            PositionSeconds = position.Value.TotalSeconds;
            IsPlaying = _videoPlayer.IsPlaying;
        });
    }

    private void OnMediaLoaded(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            HasMedia = true;
            IsPlaying = false;
            DurationText = _videoPlayer.Duration.ToString();
            DurationSeconds = Math.Max(_videoPlayer.Duration.Value.TotalSeconds, 1);
        });
    }

    private void OnPlaybackEnded(object? sender, EventArgs e) =>
        MainThread.BeginInvokeOnMainThread(() => IsPlaying = false);

    public void Dispose()
    {
        _videoPlayer.PositionChanged -= OnPositionChanged;
        _videoPlayer.MediaLoaded -= OnMediaLoaded;
        _videoPlayer.PlaybackEnded -= OnPlaybackEnded;
    }
}
