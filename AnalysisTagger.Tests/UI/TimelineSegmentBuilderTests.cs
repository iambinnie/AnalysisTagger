using AnalysisTagger.Application.DTOs;
using AnalysisTagger.Application.Projections;
using AnalysisTagger.Domain.ValueObjects;
using FluentAssertions;

namespace AnalysisTagger.Tests.UI;

public class TimelineSegmentBuilderTests
{
    private static CategoryDto MakeCategory(string name = "Shot", string color = "#3498DB") => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        Color = color
    };

    private static EventTagDto MakeEvent(Guid categoryId, double startSeconds, double endSeconds) => new()
    {
        CategoryId = categoryId,
        StartTime = Timecode.FromSeconds(startSeconds),
        EndTime = Timecode.FromSeconds(endSeconds),
        CategoryColor = "#3498DB"
    };

    [Fact]
    public void Build_NoCategories_ReturnsEmptyList()
    {
        var result = TimelineSegmentBuilder.Build([], []);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Build_CategoryWithNoEvents_ReturnsTrackWithNoSegments()
    {
        var cat = MakeCategory("Pass");

        var result = TimelineSegmentBuilder.Build([cat], []);

        result.Should().HaveCount(1);
        result[0].Segments.Should().BeEmpty();
    }

    [Fact]
    public void Build_RetainsOneLanePerCategory()
    {
        var cats = new[] { MakeCategory("Shot"), MakeCategory("Pass"), MakeCategory("Tackle") };

        var result = TimelineSegmentBuilder.Build(cats, []);

        result.Should().HaveCount(3);
    }

    [Fact]
    public void Build_TrackNameAndColorMatchCategory()
    {
        var cat = MakeCategory("Corner", "#E74C3C");

        var result = TimelineSegmentBuilder.Build([cat], []);

        result[0].CategoryName.Should().Be("Corner");
        result[0].ColorHex.Should().Be("#E74C3C");
    }

    [Fact]
    public void Build_EventMappedToCorrectTrack()
    {
        var shot = MakeCategory("Shot");
        var pass = MakeCategory("Pass");
        var ev = MakeEvent(pass.Id, 10, 15);

        var result = TimelineSegmentBuilder.Build([shot, pass], [ev]);

        result.First(t => t.CategoryId == shot.Id).Segments.Should().BeEmpty();
        result.First(t => t.CategoryId == pass.Id).Segments.Should().HaveCount(1);
    }

    [Fact]
    public void Build_SegmentStartAndEndSecondsCorrect()
    {
        var cat = MakeCategory();
        var ev = MakeEvent(cat.Id, 12.5, 17.0);

        var result = TimelineSegmentBuilder.Build([cat], [ev]);

        var seg = result[0].Segments[0];
        seg.StartSeconds.Should().BeApproximately(12.5, 0.001);
        seg.EndSeconds.Should().BeApproximately(17.0, 0.001);
    }

    [Fact]
    public void Build_MultipleEventsInSameTrack()
    {
        var cat = MakeCategory();
        var events = new[]
        {
            MakeEvent(cat.Id, 5, 8),
            MakeEvent(cat.Id, 20, 25),
            MakeEvent(cat.Id, 60, 65),
        };

        var result = TimelineSegmentBuilder.Build([cat], events);

        result[0].Segments.Should().HaveCount(3);
    }

    [Fact]
    public void Build_CategoryOrderPreserved()
    {
        var cats = new[] { MakeCategory("A"), MakeCategory("B"), MakeCategory("C") };

        var result = TimelineSegmentBuilder.Build(cats, []);

        result.Select(t => t.CategoryName).Should().Equal("A", "B", "C");
    }
}
