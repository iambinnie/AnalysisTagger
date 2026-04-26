namespace AnalysisTagger.Application.Exceptions;

public class TeamNotFoundException : AnalysisTaggerException
{
    public Guid TeamId { get; }

    public TeamNotFoundException(Guid teamId)
        : base($"Team '{teamId}' was not found.")
    {
        TeamId = teamId;
    }
}
