using AnalysisTagger.Application.DTOs;
using AnalysisTagger.Application.Projections;
using AnalysisTagger.Domain.ValueObjects;
using FluentAssertions;

namespace AnalysisTagger.Tests.UI;

public class TimelineSegmentBuilderTests
{
    private static EventTagDto MakeEvent(double startSeconds, double endSeconds, string color = "#3498DB") => new()
    {
        StartTime = Timecode.FromSeconds(startSeconds),
        EndTime = Timecode.FromSeconds(endSeconds),
        CategoryColor = color
    };

    [Fact]
    public void Build_EmptyEvents_ReturnsEmptyList()
    {
        var result = TimelineSegmentBuilder.Build([]);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Build_MapsStartSecondsFromStartTime()
    {
        var result = TimelineSegmentBuilder.Build([MakeEvent(10, 15)]);

        result[0].StartSeconds.Should().BeApproximately(10, 0.001);
    }

    [Fact]
    public void Build_MapsEndSecondsFromEndTime()
    {
        var result = TimelineSegmentBuilder.Build([MakeEvent(10, 15)]);

        result[0].EndSeconds.Should().BeApproximately(15, 0.001);
    }

    [Fact]
    public void Build_PreservesColorHex()
    {
        var result = TimelineSegmentBuilder.Build([MakeEvent(0, 5, "#E74C3C")]);

        result[0].ColorHex.Should().Be("#E74C3C");
    }

    [Fact]
    public void Build_MultipleEvents_RetainsOrder()
    {
        var events = new[]
        {
            MakeEvent(30, 35, "#FF0000"),
            MakeEvent(10, 15, "#00FF00"),
            MakeEvent(20, 25, "#0000FF"),
        };

        var result = TimelineSegmentBuilder.Build(events);

        result.Should().HaveCount(3);
        result[0].StartSeconds.Should().BeApproximately(30, 0.001);
        result[1].StartSeconds.Should().BeApproximately(10, 0.001);
        result[2].StartSeconds.Should().BeApproximately(20, 0.001);
    }

    [Fact]
    public void Build_ZeroLengthEvent_StartAndEndEqual()
    {
        var result = TimelineSegmentBuilder.Build([MakeEvent(45, 45)]);

        result[0].StartSeconds.Should().Be(result[0].EndSeconds);
    }
}
