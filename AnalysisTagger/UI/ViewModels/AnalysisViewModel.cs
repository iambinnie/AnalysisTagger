using AnalysisTagger.Application.DTOs;
using AnalysisTagger.Application.Interfaces;
using AnalysisTagger.Application.Services;
using AnalysisTagger.Domain.ValueObjects;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AnalysisTagger.UI.ViewModels;

[QueryProperty("ProjectId", "projectId")]
public partial class AnalysisViewModel : ObservableObject, IDisposable
{
    private readonly ProjectService _projectService;
    private readonly TaggingService _taggingService;
    private readonly IVideoPlayer _videoPlayer;
    private Guid _currentProjectId;

    // Video player state
    [ObservableProperty] private bool _isPlaying;
    [ObservableProperty] private bool _hasMedia;
    [ObservableProperty] private string _positionText = "00:00:00.00";
    [ObservableProperty] private string _durationText = "00:00:00.00";
    [ObservableProperty] private double _positionSeconds;
    [ObservableProperty] private double _durationSeconds = 1;

    // Project state
    [ObservableProperty] private string _projectId = string.Empty;
    [ObservableProperty] private string _projectTitle = "Analysis";
    [ObservableProperty] private ObservableCollection<CategoryDto> _categories = [];
    [ObservableProperty] private ObservableCollection<EventTagDto> _events = [];

    public bool IsSeeking { get; set; }

    public AnalysisViewModel(ProjectService projectService, TaggingService taggingService, IVideoPlayer videoPlayer)
    {
        _projectService = projectService;
        _taggingService = taggingService;
        _videoPlayer = videoPlayer;
        _videoPlayer.PositionChanged += OnPositionChanged;
        _videoPlayer.MediaLoaded += OnMediaLoaded;
        _videoPlayer.PlaybackEnded += OnPlaybackEnded;
    }

    partial void OnProjectIdChanged(string value)
    {
        if (Guid.TryParse(value, out var id))
        {
            _currentProjectId = id;
            _ = LoadProjectAsync();
        }
    }

    private async Task LoadProjectAsync()
    {
        var project = await _projectService.GetProjectAsync(_currentProjectId);
        ProjectTitle = project.Title;

        var (categories, tags) = await _taggingService.GetProjectSummaryAsync(_currentProjectId);
        Categories = new ObservableCollection<CategoryDto>(categories);
        Events = new ObservableCollection<EventTagDto>(tags);
    }

    private async Task RefreshEventsAsync()
    {
        var (_, tags) = await _taggingService.GetProjectSummaryAsync(_currentProjectId);
        Events = new ObservableCollection<EventTagDto>(tags);
    }

    // ── Video commands ──────────────────────────────────────────────

    [RelayCommand]
    private async Task LoadVideoAsync()
    {
        var result = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Select video file",
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.WinUI,       new[] { ".mp4", ".avi", ".mkv", ".mov", ".wmv" } },
                { DevicePlatform.Android,     new[] { "video/*" } },
                { DevicePlatform.iOS,         new[] { "public.movie" } },
                { DevicePlatform.MacCatalyst, new[] { "public.movie" } },
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

    // ── Tagging commands ────────────────────────────────────────────

    [RelayCommand]
    private async Task TagEventAsync(CategoryDto category)
    {
        await _taggingService.TagEventAsync(_currentProjectId, new CreateEventTagDto
        {
            CategoryId = category.Id,
            Position = _videoPlayer.Position
        });
        await RefreshEventsAsync();
    }

    [RelayCommand]
    private void SeekToEvent(EventTagDto eventTag) =>
        _videoPlayer.Seek(eventTag.StartTime);

    [RelayCommand]
    private async Task DeleteEventAsync(EventTagDto eventTag)
    {
        await _taggingService.DeleteTagAsync(_currentProjectId, eventTag.Id);
        await RefreshEventsAsync();
    }

    // ── Video player event handlers ─────────────────────────────────

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
