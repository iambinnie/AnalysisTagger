// Domain/Models/DrawingAnnotation.cs
// Stores a drawing overlaid on a specific video frame

using AnalysisTagger.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnalysisTagger.Domain.Models
{
    public class DrawingAnnotation
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public Timecode FrameTimecode { get; set; } = Timecode.Zero;
        public string SerializedDrawingData { get; set; } = string.Empty; // JSON of shapes/lines
        public string? ThumbnailPath { get; set; }
    }
}
