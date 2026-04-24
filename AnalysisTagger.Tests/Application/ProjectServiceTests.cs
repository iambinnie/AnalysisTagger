using AnalysisTagger.Application.DTOs;
using AnalysisTagger.Application.Exceptions;
using AnalysisTagger.Application.Services;
using AnalysisTagger.Domain.Enums;
using AnalysisTagger.Tests.Application.Fakes;
using FluentAssertions;

namespace AnalysisTagger.Tests.Application;

public class ProjectServiceTests
{
    private static (ProjectService Service, InMemoryUnitOfWork Uow) Create()
    {
        var uow = new InMemoryUnitOfWork();
        return (new ProjectService(uow), uow);
    }

    private static CreateProjectDto DefaultDto(string title = "Test Match") => new()
    {
        Title = title,
        Competition = "League",
        Season = "2024/25",
        MatchDate = new DateTime(2025, 1, 1),
        Sport = SportType.Football,
        VideoFilePath = "match.mp4"
    };

    [Fact]
    public async Task CreateProjectAsync_ReturnsDto_WithCorrectFields()
    {
        var (svc, _) = Create();
        var dto = DefaultDto("Cup Final");

        var result = await svc.CreateProjectAsync(dto);

        result.Title.Should().Be("Cup Final");
        result.Competition.Should().Be("League");
        result.Sport.Should().Be(SportType.Football);
        result.VideoFilePath.Should().Be("match.mp4");
        result.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateProjectAsync_AssignsDefaultTemplate_ForSport()
    {
        var (svc, uow) = Create();

        var result = await svc.CreateProjectAsync(DefaultDto());

        var stored = await uow.Projects.GetByIdAsync(result.Id);
        stored!.Template.Should().NotBeNull();
        stored.Template.Sport.Should().Be(SportType.Football);
        stored.Template.Categories.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateProjectAsync_PersistsViaUnitOfWork()
    {
        var (svc, uow) = Create();

        var result = await svc.CreateProjectAsync(DefaultDto());

        uow.SaveCount.Should().Be(1);
        (await uow.Projects.GetByIdAsync(result.Id)).Should().NotBeNull();
    }

    [Fact]
    public async Task GetProjectAsync_ReturnsCorrectProject()
    {
        var (svc, _) = Create();
        var created = await svc.CreateProjectAsync(DefaultDto("Match A"));

        var result = await svc.GetProjectAsync(created.Id);

        result.Id.Should().Be(created.Id);
        result.Title.Should().Be("Match A");
    }

    [Fact]
    public async Task GetProjectAsync_Throws_WhenProjectNotFound()
    {
        var (svc, _) = Create();

        var act = async () => await svc.GetProjectAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<ProjectNotFoundException>();
    }

    [Fact]
    public async Task GetAllProjectsAsync_ReturnsAllProjects()
    {
        var (svc, _) = Create();
        await svc.CreateProjectAsync(DefaultDto("A"));
        await svc.CreateProjectAsync(DefaultDto("B"));

        var results = (await svc.GetAllProjectsAsync()).ToList();

        results.Should().HaveCount(2);
        results.Select(r => r.Title).Should().Contain("A").And.Contain("B");
    }

    [Fact]
    public async Task DeleteProjectAsync_RemovesProject()
    {
        var (svc, uow) = Create();
        var created = await svc.CreateProjectAsync(DefaultDto());

        await svc.DeleteProjectAsync(created.Id);

        (await uow.Projects.GetByIdAsync(created.Id)).Should().BeNull();
    }

    [Fact]
    public async Task DeleteProjectAsync_Throws_WhenProjectNotFound()
    {
        var (svc, _) = Create();

        var act = async () => await svc.DeleteProjectAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<ProjectNotFoundException>();
    }

    [Fact]
    public async Task UpdateProjectAsync_UpdatesMutableFields()
    {
        var (svc, uow) = Create();
        var created = await svc.CreateProjectAsync(DefaultDto("Old Title"));

        var update = new ProjectDto
        {
            Id = created.Id,
            Title = "New Title",
            Competition = "Cup",
            Season = "2025/26",
            MatchDate = new DateTime(2025, 6, 1),
            VideoFilePath = "new.mp4"
        };
        await svc.UpdateProjectAsync(update);

        var stored = await uow.Projects.GetByIdAsync(created.Id);
        stored!.Title.Should().Be("New Title");
        stored.Competition.Should().Be("Cup");
        stored.VideoFilePath.Should().Be("new.mp4");
    }
}
