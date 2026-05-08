using AnalysisTagger.Application.Interfaces;
using AnalysisTagger.Domain.Models;

namespace AnalysisTagger.Tests.Application.Fakes;

public class InMemoryTemplateRepository : ITemplateRepository
{
    private readonly Dictionary<Guid, TagTemplate> _store = new();

    public Task<TagTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_store.TryGetValue(id, out var t) ? t : null);

    public Task<IEnumerable<TagTemplate>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IEnumerable<TagTemplate>>(_store.Values.OrderBy(t => t.Name).ToList());

    public Task AddAsync(TagTemplate template, CancellationToken cancellationToken = default)
    {
        _store[template.Id] = template;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(TagTemplate template, CancellationToken cancellationToken = default)
    {
        _store[template.Id] = template;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _store.Remove(id);
        return Task.CompletedTask;
    }

    public void TrackNewCategory(Category category) { }
}
