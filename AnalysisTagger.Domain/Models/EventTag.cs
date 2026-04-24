// Domain/Models/EventTag.cs
// The central entity — a tagged moment in the video

using AnalysisTagger.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnalysisTagger.Domain.Models
{
    public class EventTag
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public VideoSegment Segment { get; set; }
        public Category Category { get; set; } = null!;
        public string? SubCategory { get; set; }
        public List<Player> TaggedPlayers { get; set; } = new();
        public string Notes { get; set; } = string.Empty;
        public List<DrawingAnnotation> Drawings { get; set; } = new();
        public Dictionary<string, string> CustomData { get; set; } = new();
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

        // Convenience passthrough to segment
        public Timecode StartTime => Segment.Start;
        public Timecode EndTime => Segment.End;
        public TimeSpan Duration => Segment.Duration;
    }
}
