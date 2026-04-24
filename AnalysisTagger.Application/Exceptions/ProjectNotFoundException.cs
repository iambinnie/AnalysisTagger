namespace AnalysisTagger.Application.Exceptions;

public class ProjectNotFoundException : AnalysisTaggerException
{
    public Guid ProjectId { get; }

    public ProjectNotFoundException(Guid projectId)
        : base($"Project '{projectId}' was not found.")
    {
        ProjectId = projectId;
    }
}
