using AnalysisTagger.Domain.ValueObjects;
using FluentAssertions;

namespace AnalysisTagger.Tests.Domain;

public class VideoSegmentTests
{
    [Fact]
    public void Constructor_CreatesSegment_WhenEndIsAfterStart()
    {
        var segment = new VideoSegment(Timecode.FromSeconds(5), Timecode.FromSeconds(10));
        segment.Start.Value.Should().Be(TimeSpan.FromSeconds(5));
        segment.End.Value.Should().Be(TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void Constructor_Throws_WhenEndIsBeforeStart()
    {
        var act = () => new VideoSegment(Timecode.FromSeconds(10), Timecode.FromSeconds(5));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_Throws_WhenEndEqualsStart()
    {
        var tc = Timecode.FromSeconds(5);
        var act = () => new VideoSegment(tc, tc);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Duration_ReturnsEndMinusStart()
    {
        var segment = new VideoSegment(Timecode.FromSeconds(5), Timecode.FromSeconds(12));
        segment.Duration.Should().Be(TimeSpan.FromSeconds(7));
    }

    [Fact]
    public void WithLeadTime_MovesStartEarlier_LeavesEndUnchanged()
    {
        var segment = new VideoSegment(Timecode.FromSeconds(10), Timecode.FromSeconds(20));
        var result = segment.WithLeadTime(TimeSpan.FromSeconds(3));
        result.Start.Value.Should().Be(TimeSpan.FromSeconds(7));
        result.End.Value.Should().Be(TimeSpan.FromSeconds(20));
    }

    [Fact]
    public void WithLagTime_MovesEndLater_LeavesStartUnchanged()
    {
        var segment = new VideoSegment(Timecode.FromSeconds(10), Timecode.FromSeconds(20));
        var result = segment.WithLagTime(TimeSpan.FromSeconds(4));
        result.Start.Value.Should().Be(TimeSpan.FromSeconds(10));
        result.End.Value.Should().Be(TimeSpan.FromSeconds(24));
    }
}
