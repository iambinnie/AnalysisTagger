using AnalysisTagger.Domain.ValueObjects;

namespace AnalysisTagger.Application.Interfaces;

public interface IVideoPlayer
{
    Timecode Position { get; }
    Timecode Duration { get; }
    bool IsPlaying { get; }
    bool HasMedia { get; }

    void Load(string filePath);
    void Play();
    void Pause();
    void Stop();
    void Seek(Timecode position);

    event EventHandler<Timecode>? PositionChanged;
    event EventHandler? MediaLoaded;
    event EventHandler? PlaybackEnded;
}
