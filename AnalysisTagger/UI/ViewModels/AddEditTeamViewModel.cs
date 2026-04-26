using AnalysisTagger.Application.DTOs;
using AnalysisTagger.Application.Interfaces;
using AnalysisTagger.Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AnalysisTagger.UI.ViewModels;

[QueryProperty(nameof(TeamId), "teamId")]
public partial class AddEditTeamViewModel : ObservableObject
{
    private readonly TeamService _teamService;
    private readonly INavigationService _navigation;

    [ObservableProperty] private string _teamId = string.Empty;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string? _shieldImagePath;
    [ObservableProperty] private bool _isEditMode;

    public string PageTitle => IsEditMode ? "Edit Team" : "New Team";

    public AddEditTeamViewModel(TeamService teamService, INavigationService navigation)
    {
        _teamService = teamService;
        _navigation = navigation;
    }

    partial void OnTeamIdChanged(string value)
    {
        if (Guid.TryParse(value, out _))
        {
            IsEditMode = true;
            _ = LoadTeamAsync();
        }
    }

    private async Task LoadTeamAsync()
    {
        var team = await _teamService.GetTeamAsync(Guid.Parse(TeamId));
        Name = team.Name;
        ShieldImagePath = team.ShieldImagePath;
    }

    [RelayCommand]
    private async Task PickShieldImageAsync()
    {
        var result = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Select shield image",
            FileTypes = FilePickerFileType.Images
        });
        if (result != null)
            ShieldImagePath = result.FullPath;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name)) return;

        if (IsEditMode)
            await _teamService.UpdateTeamAsync(Guid.Parse(TeamId), new UpdateTeamDto
            {
                Name = Name,
                ShieldImagePath = ShieldImagePath
            });
        else
            await _teamService.CreateTeamAsync(new CreateTeamDto
            {
                Name = Name,
                ShieldImagePath = ShieldImagePath
            });

        await _navigation.GoBackAsync();
    }

    [RelayCommand]
    private Task CancelAsync() => _navigation.GoBackAsync();
}
