using AnalysisTagger.Application.Interfaces;

namespace AnalysisTagger.Tests.Application.Fakes;

public class InMemoryUnitOfWork : IUnitOfWork
{
    public InMemoryProjectRepository ProjectRepository { get; } = new();
    public InMemoryTeamRepository TeamRepository { get; } = new();
    public IProjectRepository Projects => ProjectRepository;
    public ITeamRepository Teams => TeamRepository;
    public int SaveCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveCount++;
        return Task.FromResult(1);
    }

    public void Dispose() { }
}
