using AnalysisTagger.Domain.Enums;
using AnalysisTagger.Domain.Models;
using AnalysisTagger.Domain.ValueObjects;
using AnalysisTagger.Tests.Infrastructure.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AnalysisTagger.Tests.Infrastructure;

/// <summary>
/// Verifies EF Core configuration: owned entities, value converters, JSON columns, many-to-many.
/// Each test writes to the DB then reads back in a fresh context to rule out cache hits.
/// </summary>
public class EfMappingTests
{
    private static Project MinimalProject(TagTemplate? template = null) => new()
    {
        Title = "Test",
        Template = template ?? TagTemplate.CreateDefault(SportType.Generic),
        HomeTeam = new Team { Name = "Home" },
        AwayTeam = new Team { Name = "Away" }
    };

    [Fact]
    public async Task VideoSegment_OwnedEntity_RoundTrips_Correctly()
    {
        using var fixture = new SqliteTestFixture();
        var start = Timecode.FromSeconds(13.5);
        var end = Timecode.FromSeconds(27.0);
        var project = MinimalProject();
        var category = project.Template.Categories.First();
        var tag = new EventTag
        {
            Segment = new VideoSegment(start, end),
            Category = category
        };
        project.AddEvent(tag);

        using (var ctx = fixture.CreateContext())
        {
            ctx.Projects.Add(project);
            await ctx.SaveChangesAsync();
        }

        using (var ctx = fixture.CreateContext())
        {
            var loaded = await ctx.EventTags.FirstAsync(t => t.Id == tag.Id);
            loaded.Segment.Start.Value.Should().Be(start.Value);
            loaded.Segment.End.Value.Should().Be(end.Value);
            loaded.Segment.Duration.Should().Be(end.Value - start.Value);
        }
    }

    [Fact]
    public async Task PlaylistEntry_NullSegmentOverride_RoundTrips()
    {
        using var fixture = new SqliteTestFixture();
        var project = MinimalProject();
        var category = project.Template.Categories.First();
        var tag = new EventTag
        {
            Segment = new VideoSegment(Timecode.FromSeconds(5), Timecode.FromSeconds(10)),
            Category = category
        };
        project.AddEvent(tag);
        var playlist = new Playlist { Name = "Test" };
        playlist.AddEntry(tag); // no SegmentOverride
        project.Playlists.Add(playlist);
        var entryId = playlist.Entries[0].Id;

        using (var ctx = fixture.CreateContext())
        {
            ctx.Projects.Add(project);
            await ctx.SaveChangesAsync();
        }

        using (var ctx = fixture.CreateContext())
        {
            var loaded = await ctx.PlaylistEntries.FirstAsync(e => e.Id == entryId);
            loaded.SegmentOverride.Should().BeNull();
        }
    }

    [Fact]
    public async Task PlaylistEntry_WithSegmentOverride_RoundTrips()
    {
        using var fixture = new SqliteTestFixture();
        var project = MinimalProject();
        var category = project.Template.Categories.First();
        var tag = new EventTag
        {
            Segment = new VideoSegment(Timecode.FromSeconds(5), Timecode.FromSeconds(15)),
            Category = category
        };
        project.AddEvent(tag);
        var playlist = new Playlist { Name = "Test" };
        playlist.AddEntry(tag);
        var overrideSegment = new VideoSegment(Timecode.FromSeconds(7), Timecode.FromSeconds(12));
        playlist.Entries[0].SegmentOverride = overrideSegment;
        project.Playlists.Add(playlist);
        var entryId = playlist.Entries[0].Id;

        using (var ctx = fixture.CreateContext())
        {
            ctx.Projects.Add(project);
            await ctx.SaveChangesAsync();
        }

        using (var ctx = fixture.CreateContext())
        {
            var loaded = await ctx.PlaylistEntries.FirstAsync(e => e.Id == entryId);
            loaded.SegmentOverride.Should().NotBeNull();
            loaded.SegmentOverride!.Start.Value.Should().Be(TimeSpan.FromSeconds(7));
            loaded.SegmentOverride.End.Value.Should().Be(TimeSpan.FromSeconds(12));
        }
    }

    [Fact]
    public async Task EventTag_TaggedPlayers_ManyToMany_RoundTrips()
    {
        using var fixture = new SqliteTestFixture();
        var project = MinimalProject();
        var category = project.Template.Categories.First();
        var player1 = new Player { Name = "Alice", ShirtNumber = 9, Position = "FW" };
        var player2 = new Player { Name = "Bob", ShirtNumber = 10, Position = "MF" };
        project.HomeTeam.Players.Add(player1);
        project.AwayTeam.Players.Add(player2);

        var tag = new EventTag
        {
            Segment = new VideoSegment(Timecode.FromSeconds(5), Timecode.FromSeconds(10)),
            Category = category,
            TaggedPlayers = new List<Player> { player1, player2 }
        };
        project.AddEvent(tag);

        using (var ctx = fixture.CreateContext())
        {
            ctx.Projects.Add(project);
            await ctx.SaveChangesAsync();
        }

        using (var ctx = fixture.CreateContext())
        {
            var loaded = await ctx.EventTags
                .Include(t => t.TaggedPlayers)
                .FirstAsync(t => t.Id == tag.Id);

            loaded.TaggedPlayers.Should().HaveCount(2);
            loaded.TaggedPlayers.Select(p => p.Name).Should().Contain("Alice").And.Contain("Bob");
        }
    }

    [Fact]
    public async Task Category_SubCategories_JsonColumn_RoundTrips()
    {
        using var fixture = new SqliteTestFixture();
        var category = new Category
        {
            Name = "Shot",
            SubCategories = new List<string> { "On Target", "Off Target", "Blocked" }
        };
        var template = new TagTemplate { Name = "Test", Categories = new List<Category> { category } };
        var project = MinimalProject(template);

        using (var ctx = fixture.CreateContext())
        {
            ctx.Projects.Add(project);
            await ctx.SaveChangesAsync();
        }

        using (var ctx = fixture.CreateContext())
        {
            var loaded = await ctx.Categories.FirstAsync(c => c.Id == category.Id);
            loaded.SubCategories.Should().Equal("On Target", "Off Target", "Blocked");
        }
    }

    [Fact]
    public async Task Player_CustomAttributes_JsonColumn_RoundTrips()
    {
        using var fixture = new SqliteTestFixture();
        var player = new Player
        {
            Name = "Alice",
            ShirtNumber = 9,
            Position = "FW",
            CustomAttributes = new Dictionary<string, string> { ["Nationality"] = "English", ["Height"] = "170cm" }
        };
        var project = MinimalProject();
        project.HomeTeam.Players.Add(player);

        using (var ctx = fixture.CreateContext())
        {
            ctx.Projects.Add(project);
            await ctx.SaveChangesAsync();
        }

        using (var ctx = fixture.CreateContext())
        {
            var loaded = await ctx.Players.FirstAsync(p => p.Id == player.Id);
            loaded.CustomAttributes.Should().ContainKey("Nationality").WhoseValue.Should().Be("English");
            loaded.CustomAttributes.Should().ContainKey("Height").WhoseValue.Should().Be("170cm");
        }
    }

    [Fact]
    public async Task Category_LeadLagTimes_RoundTrip_ThroughValueConverter()
    {
        using var fixture = new SqliteTestFixture();
        var category = new Category
        {
            Name = "Corner",
            DefaultLeadTime = TimeSpan.FromSeconds(3.5),
            DefaultLagTime = TimeSpan.FromSeconds(7.25)
        };
        var template = new TagTemplate { Name = "Test", Categories = new List<Category> { category } };
        var project = MinimalProject(template);

        using (var ctx = fixture.CreateContext())
        {
            ctx.Projects.Add(project);
            await ctx.SaveChangesAsync();
        }

        using (var ctx = fixture.CreateContext())
        {
            var loaded = await ctx.Categories.FirstAsync(c => c.Id == category.Id);
            loaded.DefaultLeadTime.Should().Be(TimeSpan.FromSeconds(3.5));
            loaded.DefaultLagTime.Should().Be(TimeSpan.FromSeconds(7.25));
        }
    }

    [Fact]
    public async Task DrawingAnnotation_Timecode_RoundTrips()
    {
        using var fixture = new SqliteTestFixture();
        var frameTime = Timecode.FromSeconds(15.75);
        var project = MinimalProject();
        var category = project.Template.Categories.First();
        var tag = new EventTag
        {
            Segment = new VideoSegment(Timecode.FromSeconds(10), Timecode.FromSeconds(20)),
            Category = category
        };
        var drawing = new DrawingAnnotation
        {
            FrameTimecode = frameTime,
            SerializedDrawingData = @"{""shapes"":[]}"
        };
        tag.Drawings.Add(drawing);
        project.AddEvent(tag);

        using (var ctx = fixture.CreateContext())
        {
            ctx.Projects.Add(project);
            await ctx.SaveChangesAsync();
        }

        using (var ctx = fixture.CreateContext())
        {
            var loaded = await ctx.DrawingAnnotations.FirstAsync(d => d.Id == drawing.Id);
            loaded.FrameTimecode.Value.Should().Be(frameTime.Value);
            loaded.SerializedDrawingData.Should().Be(@"{""shapes"":[]}");
        }
    }
}
