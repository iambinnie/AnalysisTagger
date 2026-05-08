using AnalysisTagger.Application.DTOs;
using AnalysisTagger.Application.Exceptions;
using AnalysisTagger.Application.Interfaces;
using AnalysisTagger.Domain.Models;

namespace AnalysisTagger.Application.Services;

public class TeamService
{
    private readonly IUnitOfWork _unitOfWork;

    public TeamService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<IEnumerable<TeamDto>> GetAllTeamsAsync(CancellationToken cancellationToken = default)
    {
        var teams = await _unitOfWork.Teams.GetAllAsync(cancellationToken);
        return teams.Select(MapToDto);
    }

    public async Task<TeamDto> GetTeamAsync(Guid teamId, CancellationToken cancellationToken = default)
    {
        var team = await _unitOfWork.Teams.GetByIdAsync(teamId, cancellationToken)
            ?? throw new TeamNotFoundException(teamId);
        return MapToDto(team);
    }

    public async Task<TeamDto> CreateTeamAsync(CreateTeamDto dto, CancellationToken cancellationToken = default)
    {
        var team = new Team { Name = dto.Name.Trim(), ShieldImagePath = dto.ShieldImagePath };
        await _unitOfWork.Teams.AddAsync(team, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(team);
    }

    public async Task UpdateTeamAsync(Guid teamId, UpdateTeamDto dto, CancellationToken cancellationToken = default)
    {
        var team = await _unitOfWork.Teams.GetByIdAsync(teamId, cancellationToken)
            ?? throw new TeamNotFoundException(teamId);
        team.Name = dto.Name.Trim();
        team.ShieldImagePath = dto.ShieldImagePath;
        await _unitOfWork.Teams.UpdateAsync(team, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteTeamAsync(Guid teamId, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.Teams.DeleteAsync(teamId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<PlayerDto> AddPlayerAsync(Guid teamId, CreatePlayerDto dto, CancellationToken cancellationToken = default)
    {
        var team = await _unitOfWork.Teams.GetByIdAsync(teamId, cancellationToken)
            ?? throw new TeamNotFoundException(teamId);

        var player = new Player
        {
            Name = dto.Name.Trim(),
            ShirtNumber = dto.ShirtNumber,
            Position = dto.Position.Trim(),
            PhotoPath = dto.PhotoPath
        };
        team.Players.Add(player);
        _unitOfWork.Teams.TrackNewPlayer(player);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapPlayerToDto(player);
    }

    public async Task UpdatePlayerAsync(Guid teamId, Guid playerId, UpdatePlayerDto dto, CancellationToken cancellationToken = default)
    {
        var team = await _unitOfWork.Teams.GetByIdAsync(teamId, cancellationToken)
            ?? throw new TeamNotFoundException(teamId);

        var player = team.FindPlayer(playerId)
            ?? throw new InvalidOperationException($"Player '{playerId}' not found in team '{teamId}'.");

        player.Name = dto.Name.Trim();
        player.ShirtNumber = dto.ShirtNumber;
        player.Position = dto.Position.Trim();
        player.PhotoPath = dto.PhotoPath;
        await _unitOfWork.Teams.UpdateAsync(team, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RemovePlayerAsync(Guid teamId, Guid playerId, CancellationToken cancellationToken = default)
    {
        var team = await _unitOfWork.Teams.GetByIdAsync(teamId, cancellationToken)
            ?? throw new TeamNotFoundException(teamId);

        var player = team.FindPlayer(playerId)
            ?? throw new InvalidOperationException($"Player '{playerId}' not found in team '{teamId}'.");

        team.Players.Remove(player);
        await _unitOfWork.Teams.UpdateAsync(team, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<PlayerDto>> GetPlayersAsync(Guid teamId, CancellationToken cancellationToken = default)
    {
        var team = await _unitOfWork.Teams.GetByIdAsync(teamId, cancellationToken)
            ?? throw new TeamNotFoundException(teamId);
        return team.Players.Select(MapPlayerToDto);
    }

    internal static TeamDto MapToDto(Team team) => new()
    {
        Id = team.Id,
        Name = team.Name,
        ShieldImagePath = team.ShieldImagePath,
        PlayerCount = team.Players.Count
    };

    internal static PlayerDto MapPlayerToDto(Player player) => new()
    {
        Id = player.Id,
        Name = player.Name,
        ShirtNumber = player.ShirtNumber,
        Position = player.Position,
        PhotoPath = player.PhotoPath
    };
}
