using AnalysisTagger.Domain.ValueObjects;
using FluentAssertions;

namespace AnalysisTagger.Tests.Domain;

public class TimecodeTests
{
    [Fact]
    public void Zero_HasZeroTimeSpanValue()
    {
        Timecode.Zero.Value.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void FromSeconds_ConvertsCorrectly()
    {
        Timecode.FromSeconds(90).Value.Should().Be(TimeSpan.FromSeconds(90));
    }

    [Fact]
    public void FromMilliseconds_ConvertsCorrectly()
    {
        Timecode.FromMilliseconds(1500).Value.Should().Be(TimeSpan.FromMilliseconds(1500));
    }

    [Fact]
    public void IsAfter_ReturnsTrue_WhenValueIsGreater()
    {
        Timecode.FromSeconds(10).IsAfter(Timecode.FromSeconds(5)).Should().BeTrue();
    }

    [Fact]
    public void IsAfter_ReturnsFalse_WhenValueIsLess()
    {
        Timecode.FromSeconds(5).IsAfter(Timecode.FromSeconds(10)).Should().BeFalse();
    }

    [Fact]
    public void IsAfter_ReturnsFalse_WhenValuesAreEqual()
    {
        var tc = Timecode.FromSeconds(5);
        tc.IsAfter(tc).Should().BeFalse();
    }

    [Fact]
    public void IsBefore_ReturnsTrue_WhenValueIsLess()
    {
        Timecode.FromSeconds(5).IsBefore(Timecode.FromSeconds(10)).Should().BeTrue();
    }

    [Fact]
    public void IsBefore_ReturnsFalse_WhenValueIsGreater()
    {
        Timecode.FromSeconds(10).IsBefore(Timecode.FromSeconds(5)).Should().BeFalse();
    }

    [Fact]
    public void Add_AddsPositiveDuration()
    {
        Timecode.FromSeconds(10).Add(TimeSpan.FromSeconds(5)).Value
            .Should().Be(TimeSpan.FromSeconds(15));
    }

    [Fact]
    public void Add_AddsNegativeDuration()
    {
        Timecode.FromSeconds(10).Add(TimeSpan.FromSeconds(-3)).Value
            .Should().Be(TimeSpan.FromSeconds(7));
    }

    [Fact]
    public void ToString_FormatsAsHoursMinutesSecondsFraction()
    {
        // 1 hour, 2 minutes, 3 seconds
        Timecode.FromSeconds(3723).ToString().Should().Be("01:02:03.00");
    }

    [Fact]
    public void Equality_HoldsForSameValue()
    {
        Timecode.FromSeconds(5).Should().Be(Timecode.FromSeconds(5));
    }
}
