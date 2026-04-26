using AnalysisTagger.Application.DTOs;
using AnalysisTagger.Application.Interfaces;
using AnalysisTagger.Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AnalysisTagger.UI.ViewModels;

[QueryProperty(nameof(TeamId), "teamId")]
[QueryProperty(nameof(PlayerId), "playerId")]
public partial class AddEditPlayerViewModel : ObservableObject
{
    private readonly TeamService _teamService;
    private readonly INavigationService _navigation;

    [ObservableProperty] private string _teamId = string.Empty;
    [ObservableProperty] private string _playerId = string.Empty;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _shirtNumber = string.Empty;
    [ObservableProperty] private string _position = string.Empty;
    [ObservableProperty] private string? _photoPath;
    [ObservableProperty] private bool _isEditMode;

    public string PageTitle => IsEditMode ? "Edit Player" : "New Player";

    public AddEditPlayerViewModel(TeamService teamService, INavigationService navigation)
    {
        _teamService = teamService;
        _navigation = navigation;
    }

    partial void OnPlayerIdChanged(string value)
    {
        if (Guid.TryParse(value, out _))
        {
            IsEditMode = true;
            _ = LoadPlayerAsync();
        }
    }

    private async Task LoadPlayerAsync()
    {
        var players = await _teamService.GetPlayersAsync(Guid.Parse(TeamId));
        var player = players.FirstOrDefault(p => p.Id == Guid.Parse(PlayerId));
        if (player is null) return;

        Name = player.Name;
        ShirtNumber = player.ShirtNumber.ToString();
        Position = player.Position;
        PhotoPath = player.PhotoPath;
    }

    [RelayCommand]
    private async Task PickPhotoAsync()
    {
        var result = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Select player photo",
            FileTypes = FilePickerFileType.Images
        });
        if (result != null)
            PhotoPath = result.FullPath;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name)) return;
        if (!int.TryParse(ShirtNumber, out var number)) number = 0;

        if (IsEditMode)
            await _teamService.UpdatePlayerAsync(Guid.Parse(TeamId), Guid.Parse(PlayerId), new UpdatePlayerDto
            {
                Name = Name,
                ShirtNumber = number,
                Position = Position,
                PhotoPath = PhotoPath
            });
        else
            await _teamService.AddPlayerAsync(Guid.Parse(TeamId), new CreatePlayerDto
            {
                Name = Name,
                ShirtNumber = number,
                Position = Position,
                PhotoPath = PhotoPath
            });

        await _navigation.GoBackAsync();
    }

    [RelayCommand]
    private Task CancelAsync() => _navigation.GoBackAsync();
}
