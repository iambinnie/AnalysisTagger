using AnalysisTagger.Application.Interfaces;
using AnalysisTagger.Domain.Models;

namespace AnalysisTagger.Tests.Application.Fakes;

public class InMemoryTeamRepository : ITeamRepository
{
    private readonly Dictionary<Guid, Team> _store = new();

    public Task<Team?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_store.TryGetValue(id, out var t) ? t : null);

    public Task<IEnumerable<Team>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IEnumerable<Team>>(_store.Values.OrderBy(t => t.Name).ToList());

    public Task AddAsync(Team team, CancellationToken cancellationToken = default)
    {
        _store[team.Id] = team;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Team team, CancellationToken cancellationToken = default)
    {
        _store[team.Id] = team;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _store.Remove(id);
        return Task.CompletedTask;
    }
}
