using AnalysisTagger.Application.DTOs;
using Microsoft.Maui.Graphics;

namespace AnalysisTagger.UI.Graphics;

public class TimelineDrawable : IDrawable
{
    public const float RowHeight = 32f;
    public const float LabelWidth = 90f;

    private const float LabelPadding = 6f;
    private const float SegmentHeight = 18f;
    private const float SegmentMinWidth = 3f;

    public IReadOnlyList<TimelineTrack> Tracks { get; set; } = [];
    public double PositionSeconds { get; set; }
    public double DurationSeconds { get; set; } = 1;

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        float w = dirtyRect.Width;
        float trackW = w - LabelWidth;

        for (int i = 0; i < Tracks.Count; i++)
        {
            var track = Tracks[i];
            float rowY = i * RowHeight;

            // Alternating row background
            canvas.FillColor = i % 2 == 0
                ? Color.FromArgb("#1E1E1E")
                : Color.FromArgb("#252525");
            canvas.FillRectangle(0, rowY, w, RowHeight);

            // Category label
            canvas.FontColor = Colors.LightGray;
            canvas.FontSize = 10;
            canvas.DrawString(
                track.CategoryName,
                LabelPadding, rowY,
                LabelWidth - LabelPadding * 2, RowHeight,
                HorizontalAlignment.Left, VerticalAlignment.Center);

            // Track groove
            canvas.FillColor = Color.FromArgb("#111111");
            float grooveY = rowY + (RowHeight - SegmentHeight) / 2f;
            canvas.FillRoundedRectangle(LabelWidth, grooveY, trackW, SegmentHeight, 2);

            // Segments
            if (DurationSeconds > 0 && track.Segments.Count > 0)
            {
                canvas.FillColor = ParseColor(track.ColorHex);
                foreach (var seg in track.Segments)
                {
                    float segX = LabelWidth + (float)(seg.StartSeconds / DurationSeconds * trackW);
                    float segW = Math.Max((float)((seg.EndSeconds - seg.StartSeconds) / DurationSeconds * trackW), SegmentMinWidth);
                    canvas.FillRoundedRectangle(segX, grooveY, segW, SegmentHeight, 2);
                }
            }
        }

        // Divider between label column and track area
        canvas.StrokeColor = Color.FromArgb("#444444");
        canvas.StrokeSize = 1;
        canvas.DrawLine(LabelWidth, 0, LabelWidth, Tracks.Count * RowHeight);

        // Playhead spanning all rows
        if (DurationSeconds > 0 && Tracks.Count > 0)
        {
            float playX = LabelWidth + (float)(PositionSeconds / DurationSeconds * trackW);
            canvas.StrokeColor = Colors.White;
            canvas.StrokeSize = 2;
            canvas.DrawLine(playX, 0, playX, Tracks.Count * RowHeight);
        }
    }

    private static Color ParseColor(string hex)
    {
        try { return Color.FromArgb(hex); }
        catch { return Colors.Gray; }
    }
}
