using AnalysisTagger.Domain.Enums;
using AnalysisTagger.Domain.Models;
using AnalysisTagger.Domain.ValueObjects;
using FluentAssertions;

namespace AnalysisTagger.Tests.Domain;

public class ProjectTests
{
    private static Category MakeCategory(string name) =>
        new() { Id = Guid.NewGuid(), Name = name };

    private static Player MakePlayer(string name) =>
        new() { Id = Guid.NewGuid(), Name = name, ShirtNumber = 1 };

    private static EventTag MakeTag(Category category, double startSecs, double endSecs) => new()
    {
        Segment = new VideoSegment(Timecode.FromSeconds(startSecs), Timecode.FromSeconds(endSecs)),
        Category = category
    };

    [Fact]
    public void AddEvent_AddsTagToEventsList()
    {
        var project = new Project();
        var cat = MakeCategory("Shot");
        var tag = MakeTag(cat, 0, 5);

        project.AddEvent(tag);

        project.Events.Should().ContainSingle().Which.Should().BeSameAs(tag);
    }

    [Fact]
    public void AddEvent_UpdatesLastModifiedAt()
    {
        var project = new Project();
        var before = project.LastModifiedAt;
        var tag = MakeTag(MakeCategory("Shot"), 0, 5);

        project.AddEvent(tag);

        project.LastModifiedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void GetEventsByCategory_ReturnsOnlyMatchingCategory()
    {
        var cat1 = MakeCategory("Shot");
        var cat2 = MakeCategory("Foul");
        var project = new Project();
        project.AddEvent(MakeTag(cat1, 0, 5));
        project.AddEvent(MakeTag(cat2, 10, 15));
        project.AddEvent(MakeTag(cat1, 20, 25));

        var results = project.GetEventsByCategory(cat1.Id).ToList();

        results.Should().HaveCount(2);
        results.Should().AllSatisfy(e => e.Category.Id.Should().Be(cat1.Id));
    }

    [Fact]
    public void GetEventsByCategory_ReturnsEmpty_WhenNoneMatch()
    {
        var project = new Project();
        project.AddEvent(MakeTag(MakeCategory("Shot"), 0, 5));

        project.GetEventsByCategory(Guid.NewGuid()).Should().BeEmpty();
    }

    [Fact]
    public void GetEventsByPlayer_ReturnsTagsForThatPlayer()
    {
        var player = MakePlayer("Alice");
        var cat = MakeCategory("Shot");
        var project = new Project();

        var tagWithPlayer = MakeTag(cat, 0, 5);
        tagWithPlayer.TaggedPlayers.Add(player);
        project.AddEvent(tagWithPlayer);
        project.AddEvent(MakeTag(cat, 10, 15)); // no players

        var results = project.GetEventsByPlayer(player.Id).ToList();

        results.Should().ContainSingle().Which.Should().BeSameAs(tagWithPlayer);
    }

    [Fact]
    public void GetEventsInWindow_ReturnsTagsWhoseStartAndEndFallWithin()
    {
        var cat = MakeCategory("Shot");
        var project = new Project();
        project.AddEvent(MakeTag(cat, 5, 10));   // inside window
        project.AddEvent(MakeTag(cat, 0, 4));    // before window
        project.AddEvent(MakeTag(cat, 50, 60));  // after window

        var results = project.GetEventsInWindow(
            Timecode.FromSeconds(3),
            Timecode.FromSeconds(45)).ToList();

        results.Should().ContainSingle()
            .Which.StartTime.Value.Should().Be(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void NewProject_HasGenericSportAndPostTaggingMode_ByDefault()
    {
        var project = new Project();
        project.Sport.Should().Be(SportType.Generic);
        project.TaggingMode.Should().Be(TaggingMode.PostTagging);
    }
}
