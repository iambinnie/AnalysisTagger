using AnalysisTagger.Application.DTOs;
using AnalysisTagger.Application.Exceptions;
using AnalysisTagger.Application.Services;
using AnalysisTagger.Domain.Models;
using AnalysisTagger.Tests.Application.Fakes;
using FluentAssertions;

namespace AnalysisTagger.Tests.Application;

public class TeamServiceTests
{
    private static (TeamService Service, InMemoryUnitOfWork Uow) Create()
    {
        var uow = new InMemoryUnitOfWork();
        return (new TeamService(uow), uow);
    }

    // ── CRUD ────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateTeamAsync_PersistsTeamWithTrimmedName()
    {
        var (svc, uow) = Create();

        var result = await svc.CreateTeamAsync(new CreateTeamDto { Name = "  Arsenal FC  " });

        result.Name.Should().Be("Arsenal FC");
        var stored = await uow.Teams.GetByIdAsync(result.Id);
        stored.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateTeamAsync_SetsShieldImagePath()
    {
        var (svc, _) = Create();

        var result = await svc.CreateTeamAsync(new CreateTeamDto
        {
            Name = "Arsenal FC",
            ShieldImagePath = "/images/arsenal.png"
        });

        result.ShieldImagePath.Should().Be("/images/arsenal.png");
    }

    [Fact]
    public async Task GetAllTeamsAsync_ReturnsAllTeams()
    {
        var (svc, _) = Create();
        await svc.CreateTeamAsync(new CreateTeamDto { Name = "Arsenal" });
        await svc.CreateTeamAsync(new CreateTeamDto { Name = "Chelsea" });

        var teams = await svc.GetAllTeamsAsync();

        teams.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetTeamAsync_Throws_WhenNotFound()
    {
        var (svc, _) = Create();

        var act = async () => await svc.GetTeamAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<TeamNotFoundException>();
    }

    [Fact]
    public async Task UpdateTeamAsync_UpdatesNameAndShield()
    {
        var (svc, uow) = Create();
        var team = await svc.CreateTeamAsync(new CreateTeamDto { Name = "Old Name" });

        await svc.UpdateTeamAsync(team.Id, new UpdateTeamDto
        {
            Name = "New Name",
            ShieldImagePath = "/new/path.png"
        });

        var stored = await uow.Teams.GetByIdAsync(team.Id);
        stored!.Name.Should().Be("New Name");
        stored.ShieldImagePath.Should().Be("/new/path.png");
    }

    [Fact]
    public async Task UpdateTeamAsync_Throws_WhenNotFound()
    {
        var (svc, _) = Create();

        var act = async () => await svc.UpdateTeamAsync(Guid.NewGuid(), new UpdateTeamDto { Name = "X" });

        await act.Should().ThrowAsync<TeamNotFoundException>();
    }

    [Fact]
    public async Task DeleteTeamAsync_RemovesTeam()
    {
        var (svc, uow) = Create();
        var team = await svc.CreateTeamAsync(new CreateTeamDto { Name = "Arsenal" });

        await svc.DeleteTeamAsync(team.Id);

        var stored = await uow.Teams.GetByIdAsync(team.Id);
        stored.Should().BeNull();
    }

    // ── Player management ───────────────────────────────────────────

    [Fact]
    public async Task AddPlayerAsync_AddsPlayerToTeam()
    {
        var (svc, _) = Create();
        var team = await svc.CreateTeamAsync(new CreateTeamDto { Name = "Arsenal" });

        var player = await svc.AddPlayerAsync(team.Id, new CreatePlayerDto
        {
            Name = "Bukayo Saka",
            ShirtNumber = 7,
            Position = "Winger"
        });

        player.Name.Should().Be("Bukayo Saka");
        player.ShirtNumber.Should().Be(7);
        var players = await svc.GetPlayersAsync(team.Id);
        players.Should().ContainSingle();
    }

    [Fact]
    public async Task AddPlayerAsync_Throws_WhenTeamNotFound()
    {
        var (svc, _) = Create();

        var act = async () => await svc.AddPlayerAsync(Guid.NewGuid(),
            new CreatePlayerDto { Name = "X", ShirtNumber = 1, Position = "" });

        await act.Should().ThrowAsync<TeamNotFoundException>();
    }

    [Fact]
    public async Task UpdatePlayerAsync_UpdatesPlayerDetails()
    {
        var (svc, _) = Create();
        var team = await svc.CreateTeamAsync(new CreateTeamDto { Name = "Arsenal" });
        var player = await svc.AddPlayerAsync(team.Id,
            new CreatePlayerDto { Name = "Old", ShirtNumber = 1, Position = "GK" });

        await svc.UpdatePlayerAsync(team.Id, player.Id,
            new UpdatePlayerDto { Name = "New", ShirtNumber = 99, Position = "ST" });

        var players = await svc.GetPlayersAsync(team.Id);
        var updated = players.Single();
        updated.Name.Should().Be("New");
        updated.ShirtNumber.Should().Be(99);
        updated.Position.Should().Be("ST");
    }

    [Fact]
    public async Task RemovePlayerAsync_RemovesPlayerFromTeam()
    {
        var (svc, _) = Create();
        var team = await svc.CreateTeamAsync(new CreateTeamDto { Name = "Arsenal" });
        var player = await svc.AddPlayerAsync(team.Id,
            new CreatePlayerDto { Name = "Saka", ShirtNumber = 7, Position = "Winger" });

        await svc.RemovePlayerAsync(team.Id, player.Id);

        var players = await svc.GetPlayersAsync(team.Id);
        players.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPlayersAsync_ReturnsEmptyList_WhenNoPlayers()
    {
        var (svc, _) = Create();
        var team = await svc.CreateTeamAsync(new CreateTeamDto { Name = "Arsenal" });

        var players = await svc.GetPlayersAsync(team.Id);

        players.Should().BeEmpty();
    }
}
