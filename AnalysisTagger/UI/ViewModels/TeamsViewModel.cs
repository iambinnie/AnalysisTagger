using AnalysisTagger.Application.DTOs;
using AnalysisTagger.Application.Interfaces;
using AnalysisTagger.Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AnalysisTagger.UI.ViewModels;

public partial class TeamsViewModel : ObservableObject
{
    private readonly TeamService _teamService;
    private readonly INavigationService _navigation;
    private readonly IDialogService _dialog;

    [ObservableProperty] private ObservableCollection<TeamDto> _teams = [];
    [ObservableProperty] private bool _isLoading;

    public TeamsViewModel(TeamService teamService, INavigationService navigation, IDialogService dialog)
    {
        _teamService = teamService;
        _navigation = navigation;
        _dialog = dialog;
    }

    [RelayCommand]
    private async Task LoadTeamsAsync()
    {
        IsLoading = true;
        try
        {
            var list = await _teamService.GetAllTeamsAsync();
            Teams = new ObservableCollection<TeamDto>(list);
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private Task AddTeamAsync() =>
        _navigation.GoToAsync($"{nameof(Pages.AddEditTeamPage)}");

    [RelayCommand]
    private Task OpenRosterAsync(TeamDto team) =>
        _navigation.GoToAsync($"{nameof(Pages.TeamRosterPage)}?teamId={team.Id}");

    [RelayCommand]
    private Task EditTeamAsync(TeamDto team) =>
        _navigation.GoToAsync($"{nameof(Pages.AddEditTeamPage)}?teamId={team.Id}");

    [RelayCommand]
    private async Task DeleteTeamAsync(TeamDto team)
    {
        var confirmed = await _dialog.ConfirmAsync(
            "Delete Team",
            $"Delete '{team.Name}'? This cannot be undone.");
        if (!confirmed) return;

        await _teamService.DeleteTeamAsync(team.Id);
        await LoadTeamsAsync();
    }
}
