using AnalysisTagger.Application.Interfaces;
using AnalysisTagger.Domain.Models;

namespace AnalysisTagger.Tests.Application.Fakes;

public class InMemoryProjectRepository : IProjectRepository
{
    private readonly Dictionary<Guid, Project> _store = new();

    public Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_store.TryGetValue(id, out var p) ? p : null);

    public Task<IEnumerable<Project>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IEnumerable<Project>>(_store.Values.ToList());

    public Task AddAsync(Project project, CancellationToken cancellationToken = default)
    {
        _store[project.Id] = project;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Project project, CancellationToken cancellationToken = default)
    {
        _store[project.Id] = project;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _store.Remove(id);
        return Task.CompletedTask;
    }
}
