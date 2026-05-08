using AnalysisTagger.Domain.Models;

namespace AnalysisTagger.Application.Interfaces;

public interface ITemplateRepository
{
    Task<TagTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TagTemplate>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(TagTemplate template, CancellationToken cancellationToken = default);
    Task UpdateAsync(TagTemplate template, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    void TrackNewCategory(Category category);
}
