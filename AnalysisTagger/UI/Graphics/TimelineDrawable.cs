using AnalysisTagger.Application.DTOs;
using Microsoft.Maui.Graphics;

namespace AnalysisTagger.UI.Graphics;

public class TimelineDrawable : IDrawable
{
    public IReadOnlyList<TimelineSegmentData> Segments { get; set; } = [];
    public double PositionSeconds { get; set; }
    public double DurationSeconds { get; set; } = 1;

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        float w = dirtyRect.Width;
        float h = dirtyRect.Height;

        // Background track
        canvas.FillColor = Color.FromArgb("#2A2A2A");
        canvas.FillRoundedRectangle(0, h * 0.35f, w, h * 0.30f, 3);

        if (DurationSeconds <= 0) return;

        // Pin markers
        foreach (var seg in Segments)
        {
            float x = (float)(seg.StartSeconds / DurationSeconds * w);
            canvas.FillColor = ParseColor(seg.ColorHex);
            canvas.FillRectangle(x - 1.5f, 2, 3, h - 4);
        }

        // Playhead
        float playX = (float)(PositionSeconds / DurationSeconds * w);
        canvas.StrokeColor = Colors.White;
        canvas.StrokeSize = 2;
        canvas.DrawLine(playX, 0, playX, h);
    }

    private static Color ParseColor(string hex)
    {
        try { return Color.FromArgb(hex); }
        catch { return Colors.Gray; }
    }
}
