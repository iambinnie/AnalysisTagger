using AnalysisTagger.Application.DTOs;
using AnalysisTagger.Application.Exceptions;
using AnalysisTagger.Application.Services;
using AnalysisTagger.Domain.Enums;
using AnalysisTagger.Domain.Models;
using AnalysisTagger.Domain.ValueObjects;
using AnalysisTagger.Tests.Application.Fakes;
using FluentAssertions;

namespace AnalysisTagger.Tests.Application;

public class TaggingServiceTests
{
    private static (TaggingService Service, InMemoryUnitOfWork Uow, Project Project) Create(
        TimeSpan? leadTime = null, TimeSpan? lagTime = null)
    {
        var uow = new InMemoryUnitOfWork();
        var svc = new TaggingService(uow);

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Shot",
            DefaultLeadTime = leadTime ?? TimeSpan.FromSeconds(3),
            DefaultLagTime = lagTime ?? TimeSpan.FromSeconds(5)
        };

        var homePlayer = new Player { Id = Guid.NewGuid(), Name = "Alice", ShirtNumber = 9 };
        var awayPlayer = new Player { Id = Guid.NewGuid(), Name = "Bob", ShirtNumber = 10 };

        var project = new Project
        {
            Template = new TagTemplate { Categories = new List<Category> { category } },
            HomeTeam = new Team { Name = "Home", Players = new List<Player> { homePlayer } },
            AwayTeam = new Team { Name = "Away", Players = new List<Player> { awayPlayer } }
        };

        uow.ProjectRepository.AddAsync(project).GetAwaiter().GetResult();

        return (svc, uow, project);
    }

    [Fact]
    public async Task TagEventAsync_CreatesTagWithLeadLagApplied()
    {
        var (svc, _, project) = Create(leadTime: TimeSpan.FromSeconds(3), lagTime: TimeSpan.FromSeconds(5));
        var categoryId = project.Template.Categories[0].Id;

        var result = await svc.TagEventAsync(project.Id, new CreateEventTagDto
        {
            CategoryId = categoryId,
            Position = Timecode.FromSeconds(30)
        });

        result.StartTime.Value.Should().Be(TimeSpan.FromSeconds(27)); // 30 - 3
        result.EndTime.Value.Should().Be(TimeSpan.FromSeconds(35));   // 30 + 5
    }

    [Fact]
    public async Task TagEventAsync_ClampsStartToZero_WhenPositionIsWithinLeadTime()
    {
        var (svc, _, project) = Create(leadTime: TimeSpan.FromSeconds(5), lagTime: TimeSpan.FromSeconds(3));
        var categoryId = project.Template.Categories[0].Id;

        var result = await svc.TagEventAsync(project.Id, new CreateEventTagDto
        {
            CategoryId = categoryId,
            Position = Timecode.FromSeconds(2) // less than lead time of 5s
        });

        result.StartTime.Should().Be(Timecode.Zero);
        result.EndTime.Value.Should().Be(TimeSpan.FromSeconds(5)); // 2 + 3
    }

    [Fact]
    public async Task TagEventAsync_AddsEventToProject()
    {
        var (svc, uow, project) = Create();
        var categoryId = project.Template.Categories[0].Id;

        await svc.TagEventAsync(project.Id, new CreateEventTagDto
        {
            CategoryId = categoryId,
            Position = Timecode.FromSeconds(60)
        });

        var stored = await uow.Projects.GetByIdAsync(project.Id);
        stored!.Events.Should().HaveCount(1);
    }

    [Fact]
    public async Task TagEventAsync_IncludesSpecifiedPlayers()
    {
        var (svc, uow, project) = Create();
        var categoryId = project.Template.Categories[0].Id;
        var playerId = project.HomeTeam.Players[0].Id;

        await svc.TagEventAsync(project.Id, new CreateEventTagDto
        {
            CategoryId = categoryId,
            Position = Timecode.FromSeconds(60),
            PlayerIds = new List<Guid> { playerId }
        });

        var stored = await uow.Projects.GetByIdAsync(project.Id);
        stored!.Events[0].TaggedPlayers.Should().ContainSingle()
            .Which.Id.Should().Be(playerId);
    }

    [Fact]
    public async Task TagEventAsync_Throws_WhenProjectNotFound()
    {
        var (svc, _, project) = Create();
        var categoryId = project.Template.Categories[0].Id;

        var act = async () => await svc.TagEventAsync(Guid.NewGuid(), new CreateEventTagDto
        {
            CategoryId = categoryId,
            Position = Timecode.FromSeconds(30)
        });

        await act.Should().ThrowAsync<ProjectNotFoundException>();
    }

    [Fact]
    public async Task TagEventAsync_Throws_WhenCategoryNotInTemplate()
    {
        var (svc, _, project) = Create();

        var act = async () => await svc.TagEventAsync(project.Id, new CreateEventTagDto
        {
            CategoryId = Guid.NewGuid(),
            Position = Timecode.FromSeconds(30)
        });

        await act.Should().ThrowAsync<InvalidTagException>();
    }

    [Fact]
    public async Task DeleteTagAsync_RemovesTagFromProject()
    {
        var (svc, uow, project) = Create();
        var categoryId = project.Template.Categories[0].Id;

        var tag = await svc.TagEventAsync(project.Id, new CreateEventTagDto
        {
            CategoryId = categoryId,
            Position = Timecode.FromSeconds(60)
        });

        await svc.DeleteTagAsync(project.Id, tag.Id);

        var stored = await uow.Projects.GetByIdAsync(project.Id);
        stored!.Events.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteTagAsync_Throws_WhenTagNotFound()
    {
        var (svc, _, project) = Create();

        var act = async () => await svc.DeleteTagAsync(project.Id, Guid.NewGuid());

        await act.Should().ThrowAsync<InvalidTagException>();
    }
}
