
using AnalysisTagger.Domain.Models;
using AnalysisTagger.Domain.ValueObjects;
using FluentAssertions;

namespace AnalysisTagger.Tests.Domain;

public class PlaylistTests
{
    private static EventTag MakeTag(double startSecs, double endSecs) => new()
    {
        Segment = new VideoSegment(Timecode.FromSeconds(startSecs), Timecode.FromSeconds(endSecs)),
        Category = new Category { Name = "Test" }
    };

    [Fact]
    public void AddEntry_AddsTagWithCorrectReference()
    {
        var playlist = new Playlist { Name = "Test" };
        var tag = MakeTag(0, 5);
        playlist.AddEntry(tag);
        playlist.Entries.Should().HaveCount(1);
        playlist.Entries[0].Tag.Should().BeSameAs(tag);
    }

    [Fact]
    public void AddEntry_AssignsSortOrderSequentially()
    {
        var playlist = new Playlist { Name = "Test" };
        playlist.AddEntry(MakeTag(0, 5));
        playlist.AddEntry(MakeTag(5, 10));
        playlist.AddEntry(MakeTag(10, 15));
        playlist.Entries.Select(e => e.SortOrder).Should().Equal(0, 1, 2);
    }

    [Fact]
    public void TotalDuration_SumsAllEntryDurations()
    {
        var playlist = new Playlist { Name = "Test" };
        playlist.AddEntry(MakeTag(0, 5));    // 5s
        playlist.AddEntry(MakeTag(10, 13));  // 3s
        playlist.TotalDuration.Should().Be(TimeSpan.FromSeconds(8));
    }

    [Fact]
    public void TotalDuration_IsZero_ForEmptyPlaylist()
    {
        new Playlist { Name = "Test" }.TotalDuration.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void Reorder_MovesEntryToNewPosition()
    {
        var playlist = new Playlist { Name = "Test" };
        var tagA = MakeTag(0, 5);
        var tagB = MakeTag(5, 10);
        var tagC = MakeTag(10, 15);
        playlist.AddEntry(tagA);
        playlist.AddEntry(tagB);
        playlist.AddEntry(tagC);

        playlist.Reorder(playlist.Entries[0].Id, 2);

        playlist.Entries[0].Tag.Should().BeSameAs(tagB);
        playlist.Entries[1].Tag.Should().BeSameAs(tagC);
        playlist.Entries[2].Tag.Should().BeSameAs(tagA);
    }

    [Fact]
    public void Reorder_NormalisesSortOrders_AfterMove()
    {
        var playlist = new Playlist { Name = "Test" };
        playlist.AddEntry(MakeTag(0, 5));
        playlist.AddEntry(MakeTag(5, 10));
        playlist.AddEntry(MakeTag(10, 15));

        playlist.Reorder(playlist.Entries[0].Id, 2);

        playlist.Entries.Select(e => e.SortOrder).Should().Equal(0, 1, 2);
    }

    [Fact]
    public void EffectiveSegment_UsesOverride_WhenSet()
    {
        var playlist = new Playlist { Name = "Test" };
        playlist.AddEntry(MakeTag(0, 10));
        var overrideSegment = new VideoSegment(Timecode.FromSeconds(2), Timecode.FromSeconds(8));
        playlist.Entries[0].SegmentOverride = overrideSegment;

        playlist.Entries[0].EffectiveSegment.Should().Be(overrideSegment);
    }

    [Fact]
    public void EffectiveSegment_UsesTagSegment_WhenNoOverride()
    {
        var playlist = new Playlist { Name = "Test" };
        var tag = MakeTag(0, 10);
        playlist.AddEntry(tag);

        playlist.Entries[0].EffectiveSegment.Should().Be(tag.Segment);
    }
}
