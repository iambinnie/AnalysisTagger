// Domain/Models/PlaylistEntry.cs
// A single item in a playlist — wraps an EventTag with optional overrides

using AnalysisTagger.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnalysisTagger.Domain.Models
{
    public class PlaylistEntry
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public EventTag Tag { get; set; } = null!;
        public int SortOrder { get; set; }

        // Allow per-entry override of the segment window
        public VideoSegment? SegmentOverride { get; set; }
        public VideoSegment EffectiveSegment => SegmentOverride ?? Tag.Segment;
    }
}
