using AnalysisTagger.Application.Interfaces;
using AnalysisTagger.Domain.Models;
using AnalysisTagger.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AnalysisTagger.Infrastructure.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly AppDbContext _context;

    public ProjectRepository(AppDbContext context) => _context = context;

    public async Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await FullGraph()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IEnumerable<Project>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.Projects
            .Include(p => p.Events)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Project project, CancellationToken cancellationToken = default) =>
        await _context.Projects.AddAsync(project, cancellationToken);

    public Task UpdateAsync(Project project, CancellationToken cancellationToken = default)
    {
        // Only attach when detached — if already tracked (singleton DbContext),
        // EF's change detection handles the update without forcing all related
        // entities into Modified state (which would UPDATE newly-added children).
        if (_context.Entry(project).State == EntityState.Detached)
            _context.Projects.Update(project);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var project = await _context.Projects.FindAsync(new object[] { id }, cancellationToken);
        if (project is not null)
            _context.Projects.Remove(project);
    }

    public void TrackNewEventTag(EventTag tag) =>
        _context.EventTags.Add(tag);

    private IQueryable<Project> FullGraph() =>
        _context.Projects
            .Include(p => p.Template).ThenInclude(t => t.Categories)
            .Include(p => p.HomeTeam).ThenInclude(t => t.Players)
            .Include(p => p.AwayTeam).ThenInclude(t => t.Players)
            .Include(p => p.Events).ThenInclude(e => e.Category)
            .Include(p => p.Events).ThenInclude(e => e.TaggedPlayers)
            .Include(p => p.Events).ThenInclude(e => e.Drawings)
            .Include(p => p.Playlists).ThenInclude(pl => pl.Entries).ThenInclude(e => e.Tag);
}
