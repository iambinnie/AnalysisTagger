using AnalysisTagger.Application.DTOs;
using AnalysisTagger.Application.Interfaces;
using AnalysisTagger.Application.Services;
using AnalysisTagger.Domain.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AnalysisTagger.UI.ViewModels;

[QueryProperty(nameof(ProjectId), "projectId")]
public partial class ProjectSetupViewModel : ObservableObject
{
    private readonly ProjectService _projectService;
    private readonly TeamService _teamService;
    private readonly TemplateService _templateService;
    private readonly INavigationService _navigation;

    [ObservableProperty] private string _projectId = string.Empty;
    [ObservableProperty] private string _title = string.Empty;
    [ObservableProperty] private string _competition = string.Empty;
    [ObservableProperty] private string _season = string.Empty;
    [ObservableProperty] private DateTime _matchDate = DateTime.Today;
    [ObservableProperty] private SportType _selectedSport = SportType.Generic;
    [ObservableProperty] private TaggingMode _selectedTaggingMode = TaggingMode.PostTagging;
    [ObservableProperty] private string _videoFilePath = string.Empty;
    [ObservableProperty] private TeamDto? _selectedHomeTeam;
    [ObservableProperty] private TeamDto? _selectedAwayTeam;
    [ObservableProperty] private TemplateDto? _selectedTemplate;
    [ObservableProperty] private ObservableCollection<TeamDto> _teams = [];
    [ObservableProperty] private ObservableCollection<TemplateDto> _templates = [];
    [ObservableProperty] private bool _isEditMode;
    [ObservableProperty] private bool _isLoading;

    public string PageTitle => IsEditMode ? "Edit Project" : "New Project";

    public List<SportType> Sports { get; } = Enum.GetValues<SportType>().ToList();
    public List<TaggingMode> TaggingModes { get; } = Enum.GetValues<TaggingMode>().ToList();

    public ProjectSetupViewModel(
        ProjectService projectService,
        TeamService teamService,
        TemplateService templateService,
        INavigationService navigation)
    {
        _projectService = projectService;
        _teamService = teamService;
        _templateService = templateService;
        _navigation = navigation;
    }

    partial void OnProjectIdChanged(string value)
    {
        if (Guid.TryParse(value, out _))
            IsEditMode = true;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            var teams = await _teamService.GetAllTeamsAsync();
            Teams = new ObservableCollection<TeamDto>(teams);

            var templates = await _templateService.GetAllTemplatesAsync();
            Templates = new ObservableCollection<TemplateDto>(templates);

            if (IsEditMode)
                await LoadProjectAsync();
        }
        finally { IsLoading = false; }
    }

    private async Task LoadProjectAsync()
    {
        var project = await _projectService.GetProjectAsync(Guid.Parse(ProjectId));
        Title = project.Title;
        Competition = project.Competition;
        Season = project.Season;
        MatchDate = project.MatchDate;
        SelectedSport = project.Sport;
        SelectedTaggingMode = project.TaggingMode;
        VideoFilePath = project.VideoFilePath;
    }

    [RelayCommand]
    private async Task PickVideoAsync()
    {
        var result = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Select video file",
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.WinUI, new[] { ".mp4", ".avi", ".mkv", ".mov", ".wmv" } },
                { DevicePlatform.Android, new[] { "video/*" } },
                { DevicePlatform.iOS, new[] { "public.movie" } },
            })
        });
        if (result is not null)
            VideoFilePath = result.FullPath;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Title)) return;

        if (IsEditMode)
        {
            await _projectService.UpdateProjectAsync(new ProjectDto
            {
                Id = Guid.Parse(ProjectId),
                Title = Title.Trim(),
                Competition = Competition,
                Season = Season,
                MatchDate = MatchDate,
                Sport = SelectedSport,
                TaggingMode = SelectedTaggingMode,
                VideoFilePath = VideoFilePath
            });
            await _navigation.GoBackAsync();
        }
        else
        {
            var created = await _projectService.CreateProjectAsync(new CreateProjectDto
            {
                Title = Title.Trim(),
                Competition = Competition,
                Season = Season,
                MatchDate = MatchDate,
                Sport = SelectedSport,
                TaggingMode = SelectedTaggingMode,
                VideoFilePath = VideoFilePath,
                HomeTeamId = SelectedHomeTeam?.Id,
                AwayTeamId = SelectedAwayTeam?.Id,
                TemplateId = SelectedTemplate?.Id
            });
            await _navigation.GoToAsync($"../{nameof(Pages.AnalysisPage)}?projectId={created.Id}");
        }
    }

    [RelayCommand]
    private Task CancelAsync() => _navigation.GoBackAsync();
}
