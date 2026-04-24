// Domain/ValueObjects/VideoSegment.cs
using System;
using System.Collections.Generic;
using System.Text;

// Represents a start/end window of video — the core unit of a tag

namespace AnalysisTagger.Domain.ValueObjects
{
    public record VideoSegment
    {
        public Timecode Start { get; init; }
        public Timecode End { get; init; }

        public VideoSegment(Timecode start, Timecode end)
        {
            if (end.IsAfter(start) == false)
                throw new ArgumentException("End must be after Start");
            Start = start;
            End = end;
        }

        public TimeSpan Duration => End.Value - Start.Value;

        // Lead/lag adjustment — mirrors LongoMatch's original feature
        public VideoSegment WithLeadTime(TimeSpan lead) =>
            new(Start.Add(-lead), End);
        public VideoSegment WithLagTime(TimeSpan lag) =>
            new(Start, End.Add(lag));
    }
}
