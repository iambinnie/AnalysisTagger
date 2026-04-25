using AnalysisTagger.Application.Interfaces;
using AnalysisTagger.Domain.ValueObjects;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Maui.Views;

namespace AnalysisTagger.Services;

public sealed class MediaElementVideoPlayer : IVideoPlayer
{
    private MediaElement? _element;

    public Timecode Position => _element != null
        ? new Timecode(_element.Position)
        : Timecode.Zero;

    public Timecode Duration => _element != null
        ? new Timecode(_element.Duration)
        : Timecode.Zero;

    public bool IsPlaying => _element?.CurrentState == MediaElementState.Playing;

    public bool HasMedia { get; private set; }

    public event EventHandler<Timecode>? PositionChanged;
    public event EventHandler? MediaLoaded;
    public event EventHandler? PlaybackEnded;

    public void Attach(MediaElement element)
    {
        if (_element != null) Detach();
        _element = element;
        _element.MediaOpened += OnMediaOpened;
        _element.MediaEnded += OnMediaEnded;
        _element.PositionChanged += OnPositionChanged;
    }

    public void Detach()
    {
        if (_element == null) return;
        _element.MediaOpened -= OnMediaOpened;
        _element.MediaEnded -= OnMediaEnded;
        _element.PositionChanged -= OnPositionChanged;
        _element = null;
        HasMedia = false;
    }

    public void Load(string filePath)
    {
        if (_element == null) return;
        HasMedia = false;
        _element.Source = MediaSource.FromFile(filePath);
    }

    public void Play() => _element?.Play();
    public void Pause() => _element?.Pause();
    public void Stop() => _element?.Stop();

    public void Seek(Timecode position)
    {
        if (_element == null) return;
        _ = MainThread.InvokeOnMainThreadAsync(() => _element.SeekTo(position.Value));
    }

    public void SetSpeed(double rate)
    {
        if (_element != null) _element.Speed = rate;
    }

    private void OnMediaOpened(object? sender, EventArgs e)
    {
        HasMedia = true;
        MediaLoaded?.Invoke(this, EventArgs.Empty);
    }

    private void OnMediaEnded(object? sender, EventArgs e) =>
        PlaybackEnded?.Invoke(this, EventArgs.Empty);

    private void OnPositionChanged(object? sender, EventArgs e)
    {
        if (_element != null)
            PositionChanged?.Invoke(this, new Timecode(_element.Position));
    }
}
