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
    private readonly TemplateService _templateService;
    private readonly IVideoPlayer _videoPlayer;
    private readonly INavigationService _navigation;
    private readonly IDialogService _dialog;

    private Guid _currentProjectId;
    private Guid _currentTemplateId;

    [ObservableProperty] private bool _isPlaying;
    [ObservableProperty] private bool _hasMedia;
    [ObservableProperty] private string _positionText = "00:00:00.00";
    [ObservableProperty] private string _durationText = "00:00:00.00";
    [ObservableProperty] private double _positionSeconds;
    [ObservableProperty] private double _durationSeconds = 1;
    [ObservableProperty] private string _projectId = string.Empty;
    [ObservableProperty] private string _projectTitle = "Analysis";
    [ObservableProperty] private string _templateName = string.Empty;
    [ObservableProperty] private ObservableCollection<CategoryButtonViewModel> _categoryButtons = [];
    [ObservableProperty] private ObservableCollection<EventTagDto> _events = [];

    public IReadOnlyList<CategoryDto> CurrentCategories { get; private set; } = [];

    public bool IsSeeking { get; set; }

    public AnalysisViewModel(
        ProjectService projectService,
        TaggingService taggingService,
        TemplateService templateService,
        IVideoPlayer videoPlayer,
        INavigationService navigation,
        IDialogService dialog)
    {
        _projectService = projectService;
        _taggingService = taggingService;
        _templateService = templateService;
        _videoPlayer = videoPlayer;
        _navigation = navigation;
        _dialog = dialog;
        _videoPlayer.PositionChanged += OnPositionChanged;
        _videoPlayer.MediaLoaded += OnMediaLoaded;
        _videoPlayer.PlaybackEnded += OnPlaybackEnded;
    }

    private string _videoFilePath = string.Empty;

    partial void OnProjectIdChanged(string value)
    {
        if (Guid.TryParse(value, out var id))
            _currentProjectId = id;
    }

    internal async Task LoadProjectAsync()
    {
        var project = await _projectService.GetProjectAsync(_currentProjectId);
        ProjectTitle = project.Title;
        TemplateName = project.TemplateName;
        _currentTemplateId = project.TemplateId;
        _videoFilePath = project.VideoFilePath;

        var (categories, tags) = await _taggingService.GetProjectSummaryAsync(_currentProjectId);
        var categoryList = categories.ToList();
        CurrentCategories = categoryList;
        CategoryButtons = new ObservableCollection<CategoryButtonViewModel>(categoryList.Select(BuildCategoryButton));
        Events = new ObservableCollection<EventTagDto>(tags);
    }

    internal void AutoLoadVideo()
    {
        if (!string.IsNullOrEmpty(_videoFilePath) && File.Exists(_videoFilePath))
            _videoPlayer.Load(_videoFilePath);
    }

    private async Task RefreshEventsAsync()
    {
        var (_, tags) = await _taggingService.GetProjectSummaryAsync(_currentProjectId);
        Events = new ObservableCollection<EventTagDto>(tags);
    }

    [RelayCommand]
    private async Task AssignTemplateAsync()
    {
        var templates = (await _templateService.GetAllTemplatesAsync()).ToList();
        if (templates.Count == 0)
        {
            await _dialog.AlertAsync("No Templates", "Create a template in the Tag Templates section first.");
            return;
        }

        var chosen = await _dialog.ActionSheetAsync(
            "Select Template",
            "Cancel",
            templates.Select(t => t.Name));

        if (string.IsNullOrEmpty(chosen)) return;

        var template = templates.FirstOrDefault(t => t.Name == chosen);
        if (template is null) return;

        await _projectService.AssignTemplateAsync(_currentProjectId, template.Id);
        await LoadProjectAsync();
    }

    [RelayCommand]
    private Task EditTemplateAsync() =>
        _navigation.GoToAsync($"{nameof(Pages.TemplateEditorPage)}?templateId={_currentTemplateId}");

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

        var project = await _projectService.GetProjectAsync(_currentProjectId);
        project.VideoFilePath = result.FullPath;
        await _projectService.UpdateProjectAsync(project);
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

    private CategoryButtonViewModel BuildCategoryButton(CategoryDto c)
    {
        var categoryId = c.Id;
        var tagMain = new AsyncRelayCommand(() => TagCategoryAsync(categoryId, string.Empty));

        var subItems = c.SubCategories
            .Select(sc => new SubCategoryItem(sc, new AsyncRelayCommand(() => TagCategoryAsync(categoryId, sc))))
            .ToList();

        var rows = new List<SubCategoryRow>();
        for (int i = 0; i < subItems.Count; i += 2)
            rows.Add(new SubCategoryRow(subItems[i], i + 1 < subItems.Count ? subItems[i + 1] : null));

        return new CategoryButtonViewModel(c.Name, c.Color, tagMain, rows);
    }

    private async Task TagCategoryAsync(Guid categoryId, string subCategory)
    {
        await _taggingService.TagEventAsync(_currentProjectId, new CreateEventTagDto
        {
            CategoryId = categoryId,
            Position = _videoPlayer.Position,
            SubCategory = subCategory
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
