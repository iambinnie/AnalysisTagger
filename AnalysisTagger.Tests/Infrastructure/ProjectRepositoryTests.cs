using AnalysisTagger.Domain.Enums;
using AnalysisTagger.Domain.Models;
using AnalysisTagger.Domain.ValueObjects;
using AnalysisTagger.Infrastructure;
using AnalysisTagger.Infrastructure.Repositories;
using AnalysisTagger.Tests.Infrastructure.Helpers;
using FluentAssertions;

namespace AnalysisTagger.Tests.Infrastructure;

public class ProjectRepositoryTests
{
    private static Project BuildFullProject()
    {
        var template = TagTemplate.CreateDefault(SportType.Football);
        var category = template.Categories.First();

        var homePlayer = new Player { Name = "Alice", ShirtNumber = 9, Position = "Forward" };
        var awayPlayer = new Player { Name = "Bob", ShirtNumber = 10, Position = "Midfielder" };

        var project = new Project
        {
            Title = "Test Match",
            Competition = "League",
            Season = "2024/25",
            Sport = SportType.Football,
            VideoFilePath = "match.mp4",
            Template = template,
            HomeTeam = new Team { Name = "Home FC", Players = new List<Player> { homePlayer } },
            AwayTeam = new Team { Name = "Away FC", Players = new List<Player> { awayPlayer } }
        };

        var tag = new EventTag
        {
            Segment = new VideoSegment(Timecode.FromSeconds(10), Timecode.FromSeconds(20)),
            Category = category,
            Notes = "Good chance",
            TaggedPlayers = new List<Player> { homePlayer }
        };
        tag.Drawings.Add(new DrawingAnnotation
        {
            FrameTimecode = Timecode.FromSeconds(12),
            SerializedDrawingData = "{}"
        });
        project.AddEvent(tag);

        var playlist = new Playlist { Name = "Highlights" };
        playlist.AddEntry(tag);
        project.Playlists.Add(playlist);

        return project;
    }

    [Fact]
    public async Task AddAsync_PersistsProject()
    {
        using var fixture = new SqliteTestFixture();
        var project = BuildFullProject();

        using (var ctx = fixture.CreateContext())
        {
            var repo = new ProjectRepository(ctx);
            await repo.AddAsync(project);
            await ctx.SaveChangesAsync();
        }

        using (var ctx = fixture.CreateContext())
        {
            ctx.Projects.Should().ContainSingle(p => p.Id == project.Id);
        }
    }

    [Fact]
    public async Task GetByIdAsync_LoadsFullGraph()
    {
        using var fixture = new SqliteTestFixture();
        var project = BuildFullProject();
        var projectId = project.Id;
        var tagId = project.Events[0].Id;

        using (var ctx = fixture.CreateContext())
        {
            await new ProjectRepository(ctx).AddAsync(project);
            await ctx.SaveChangesAsync();
        }

        using (var ctx = fixture.CreateContext())
        {
            var loaded = await new ProjectRepository(ctx).GetByIdAsync(projectId);

            loaded.Should().NotBeNull();
            loaded!.Title.Should().Be("Test Match");

            loaded.Template.Should().NotBeNull();
            loaded.Template.Categories.Should().NotBeEmpty();

            loaded.HomeTeam.Players.Should().ContainSingle(p => p.Name == "Alice");
            loaded.AwayTeam.Players.Should().ContainSingle(p => p.Name == "Bob");

            loaded.Events.Should().ContainSingle(e => e.Id == tagId);
            var loadedTag = loaded.Events.First();
            loadedTag.Category.Should().NotBeNull();
            loadedTag.TaggedPlayers.Should().ContainSingle(p => p.Name == "Alice");
            loadedTag.Drawings.Should().ContainSingle();
            loadedTag.Notes.Should().Be("Good chance");

            loaded.Playlists.Should().ContainSingle(pl => pl.Name == "Highlights");
            loaded.Playlists[0].Entries.Should().ContainSingle();
        }
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        using var fixture = new SqliteTestFixture();
        using var ctx = fixture.CreateContext();

        var result = await new ProjectRepository(ctx).GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllProjects()
    {
        using var fixture = new SqliteTestFixture();

        using (var ctx = fixture.CreateContext())
        {
            var repo = new ProjectRepository(ctx);
            await repo.AddAsync(BuildFullProject());
            await repo.AddAsync(BuildFullProject());
            await ctx.SaveChangesAsync();
        }

        using (var ctx = fixture.CreateContext())
        {
            var results = (await new ProjectRepository(ctx).GetAllAsync()).ToList();
            results.Should().HaveCount(2);
        }
    }

    [Fact]
    public async Task UpdateAsync_SavesChanges()
    {
        using var fixture = new SqliteTestFixture();
        var project = BuildFullProject();

        using (var ctx = fixture.CreateContext())
        {
            await new ProjectRepository(ctx).AddAsync(project);
            await ctx.SaveChangesAsync();
        }

        using (var ctx = fixture.CreateContext())
        {
            var repo = new ProjectRepository(ctx);
            var loaded = await repo.GetByIdAsync(project.Id);
            loaded!.Title = "Updated Title";
            await repo.UpdateAsync(loaded);
            await ctx.SaveChangesAsync();
        }

        using (var ctx = fixture.CreateContext())
        {
            var loaded = await new ProjectRepository(ctx).GetByIdAsync(project.Id);
            loaded!.Title.Should().Be("Updated Title");
        }
    }

    [Fact]
    public async Task DeleteAsync_RemovesProject()
    {
        using var fixture = new SqliteTestFixture();
        var project = BuildFullProject();

        using (var ctx = fixture.CreateContext())
        {
            await new ProjectRepository(ctx).AddAsync(project);
            await ctx.SaveChangesAsync();
        }

        using (var ctx = fixture.CreateContext())
        {
            var repo = new ProjectRepository(ctx);
            await repo.DeleteAsync(project.Id);
            await ctx.SaveChangesAsync();
        }

        using (var ctx = fixture.CreateContext())
        {
            (await new ProjectRepository(ctx).GetByIdAsync(project.Id)).Should().BeNull();
        }
    }

    [Fact]
    public async Task UnitOfWork_SaveChangesAsync_CommitsViaSharedContext()
    {
        using var fixture = new SqliteTestFixture();
        var project = BuildFullProject();

        using (var ctx = fixture.CreateContext())
        {
            var uow = new UnitOfWork(ctx);
            await uow.Projects.AddAsync(project);
            await uow.SaveChangesAsync();
        }

        using (var ctx = fixture.CreateContext())
        {
            ctx.Projects.Should().ContainSingle(p => p.Id == project.Id);
        }
    }
}
