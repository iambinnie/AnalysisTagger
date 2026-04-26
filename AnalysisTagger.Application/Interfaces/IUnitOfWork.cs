namespace AnalysisTagger.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IProjectRepository Projects { get; }
    ITeamRepository Teams { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
