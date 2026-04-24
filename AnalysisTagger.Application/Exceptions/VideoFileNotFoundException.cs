namespace AnalysisTagger.Application.Exceptions;

public class VideoFileNotFoundException : AnalysisTaggerException
{
    public string FilePath { get; }

    public VideoFileNotFoundException(string filePath)
        : base($"Video file not found: '{filePath}'")
    {
        FilePath = filePath;
    }
}
