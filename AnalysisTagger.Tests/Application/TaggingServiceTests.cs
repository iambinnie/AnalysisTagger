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

    // ── GetProjectSummaryAsync ──────────────────────────────────────

    [Fact]
    public async Task GetProjectSummaryAsync_ReturnsCategoriesOrderedBySortOrder()
    {
        var uow = new InMemoryUnitOfWork();
        var svc = new TaggingService(uow);

        var project = new Project
        {
            Template = new TagTemplate
            {
                Categories = new List<Category>
                {
                    new() { Id = Guid.NewGuid(), Name = "B", SortOrder = 2 },
                    new() { Id = Guid.NewGuid(), Name = "A", SortOrder = 1 },
                    new() { Id = Guid.NewGuid(), Name = "C", SortOrder = 3 },
                }
            },
            HomeTeam = new Team(),
            AwayTeam = new Team()
        };
        await uow.Projects.AddAsync(project);

        var (categories, _) = await svc.GetProjectSummaryAsync(project.Id);

        categories.Select(c => c.Name).Should().ContainInOrder("A", "B", "C");
    }

    [Fact]
    public async Task GetProjectSummaryAsync_ReturnsTagsOrderedByStartTime()
    {
        var (svc, uow, project) = Create();
        var categoryId = project.Template.Categories[0].Id;

        await svc.TagEventAsync(project.Id, new CreateEventTagDto { CategoryId = categoryId, Position = Timecode.FromSeconds(60) });
        await svc.TagEventAsync(project.Id, new CreateEventTagDto { CategoryId = categoryId, Position = Timecode.FromSeconds(10) });
        await svc.TagEventAsync(project.Id, new CreateEventTagDto { CategoryId = categoryId, Position = Timecode.FromSeconds(30) });

        var (_, tags) = await svc.GetProjectSummaryAsync(project.Id);

        tags.Select(t => t.StartTime.Value.TotalSeconds)
            .Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task GetProjectSummaryAsync_ReturnsCategoryDtoFields()
    {
        var uow = new InMemoryUnitOfWork();
        var svc = new TaggingService(uow);
        var catId = Guid.NewGuid();

        var project = new Project
        {
            Template = new TagTemplate
            {
                Categories = new List<Category>
                {
                    new() { Id = catId, Name = "Shot", Color = "#FF0000", SortOrder = 1 }
                }
            },
            HomeTeam = new Team(),
            AwayTeam = new Team()
        };
        await uow.Projects.AddAsync(project);

        var (categories, _) = await svc.GetProjectSummaryAsync(project.Id);
        var dto = categories.Single();

        dto.Id.Should().Be(catId);
        dto.Name.Should().Be("Shot");
        dto.Color.Should().Be("#FF0000");
        dto.SortOrder.Should().Be(1);
    }

    [Fact]
    public async Task GetProjectSummaryAsync_MapsSubCategoriesToDto()
    {
        var uow = new InMemoryUnitOfWork();
        var svc = new TaggingService(uow);

        var project = new Project
        {
            Template = new TagTemplate
            {
                Categories = new List<Category>
                {
                    new() { Name = "Shot", SortOrder = 1, SubCategories = new List<string> { "On Target", "Off Target" } }
                }
            },
            HomeTeam = new Team(),
            AwayTeam = new Team()
        };
        await uow.Projects.AddAsync(project);

        var (categories, _) = await svc.GetProjectSummaryAsync(project.Id);
        var dto = categories.Single();

        dto.SubCategories.Should().HaveCount(2);
        dto.SubCategories.Should().Contain("On Target").And.Contain("Off Target");
    }

    [Fact]
    public async Task TagEventAsync_RecordsSubCategory()
    {
        var (svc, uow, project) = Create();
        var categoryId = project.Template.Categories[0].Id;

        var result = await svc.TagEventAsync(project.Id, new CreateEventTagDto
        {
            CategoryId = categoryId,
            Position = Timecode.FromSeconds(30),
            SubCategory = "On Target"
        });

        result.SubCategory.Should().Be("On Target");
        var stored = await uow.Projects.GetByIdAsync(project.Id);
        stored!.Events[0].SubCategory.Should().Be("On Target");
    }

    [Fact]
    public async Task GetProjectSummaryAsync_Throws_WhenProjectNotFound()
    {
        var uow = new InMemoryUnitOfWork();
        var svc = new TaggingService(uow);

        var act = async () => await svc.GetProjectSummaryAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<ProjectNotFoundException>();
    }
}
