// Domain/ValueObjects/Timecode.cs
using System;
using System.Collections.Generic;
using System.Text;

// A strongly typed wrapper around TimeSpan.
// Prevents mixing up raw numbers with time values.

namespace AnalysisTagger.Domain.ValueObjects
{
    public record Timecode(TimeSpan Value)
    {
        public static Timecode Zero => new(TimeSpan.Zero);
        public static Timecode FromSeconds(double seconds) =>
            new(TimeSpan.FromSeconds(seconds));
        public static Timecode FromMilliseconds(double ms) =>
            new(TimeSpan.FromMilliseconds(ms));

        public bool IsAfter(Timecode other) => Value > other.Value;
        public bool IsBefore(Timecode other) => Value < other.Value;
        public Timecode Add(TimeSpan duration) => new(Value + duration);

        public override string ToString() => Value.ToString(@"hh\:mm\:ss\.ff");
    }
}
