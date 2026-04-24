namespace AnalysisTagger.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IProjectRepository Projects { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
