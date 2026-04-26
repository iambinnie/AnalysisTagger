using AnalysisTagger.Application.DTOs;
using AnalysisTagger.Application.Interfaces;
using AnalysisTagger.Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AnalysisTagger.UI.ViewModels;

[QueryProperty(nameof(TeamId), "teamId")]
public partial class TeamRosterViewModel : ObservableObject
{
    private readonly TeamService _teamService;
    private readonly INavigationService _navigation;
    private readonly IDialogService _dialog;

    [ObservableProperty] private string _teamId = string.Empty;
    [ObservableProperty] private string _teamName = string.Empty;
    [ObservableProperty] private ObservableCollection<PlayerDto> _players = [];

    public TeamRosterViewModel(TeamService teamService, INavigationService navigation, IDialogService dialog)
    {
        _teamService = teamService;
        _navigation = navigation;
        _dialog = dialog;
    }

    partial void OnTeamIdChanged(string value)
    {
        if (Guid.TryParse(value, out _))
            _ = LoadRosterAsync();
    }

    [RelayCommand]
    private async Task LoadRosterAsync()
    {
        var team = await _teamService.GetTeamAsync(Guid.Parse(TeamId));
        TeamName = team.Name;
        var players = await _teamService.GetPlayersAsync(Guid.Parse(TeamId));
        Players = new ObservableCollection<PlayerDto>(players.OrderBy(p => p.ShirtNumber));
    }

    [RelayCommand]
    private Task AddPlayerAsync() =>
        _navigation.GoToAsync($"{nameof(Pages.AddEditPlayerPage)}?teamId={TeamId}");

    [RelayCommand]
    private Task EditPlayerAsync(PlayerDto player) =>
        _navigation.GoToAsync($"{nameof(Pages.AddEditPlayerPage)}?teamId={TeamId}&playerId={player.Id}");

    [RelayCommand]
    private async Task DeletePlayerAsync(PlayerDto player)
    {
        var confirmed = await _dialog.ConfirmAsync(
            "Remove Player",
            $"Remove {player.Name} (#{player.ShirtNumber}) from the squad?");
        if (!confirmed) return;

        await _teamService.RemovePlayerAsync(Guid.Parse(TeamId), player.Id);
        await LoadRosterAsync();
    }
}
