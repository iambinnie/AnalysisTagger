using AnalysisTagger.Application.Interfaces;
using AnalysisTagger.Domain.Models;
using AnalysisTagger.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AnalysisTagger.Infrastructure.Repositories;

public class TemplateRepository : ITemplateRepository
{
    private readonly AppDbContext _context;

    public TemplateRepository(AppDbContext context) => _context = context;

    public async Task<TagTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.TagTemplates
            .Include(t => t.Categories)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<IEnumerable<TagTemplate>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.TagTemplates
            .Include(t => t.Categories)
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(TagTemplate template, CancellationToken cancellationToken = default) =>
        await _context.TagTemplates.AddAsync(template, cancellationToken);

    public Task UpdateAsync(TagTemplate template, CancellationToken cancellationToken = default)
    {
        if (_context.Entry(template).State == EntityState.Detached)
            _context.TagTemplates.Update(template);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var template = await _context.TagTemplates.FindAsync(new object[] { id }, cancellationToken);
        if (template is not null)
            _context.TagTemplates.Remove(template);
    }

    public void TrackNewCategory(Category category) =>
        _context.Categories.Add(category);
}
