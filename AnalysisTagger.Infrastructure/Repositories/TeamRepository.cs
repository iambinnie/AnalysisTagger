using AnalysisTagger.Application.Interfaces;
using AnalysisTagger.Domain.Models;
using AnalysisTagger.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AnalysisTagger.Infrastructure.Repositories;

public class TeamRepository : ITeamRepository
{
    private readonly AppDbContext _context;

    public TeamRepository(AppDbContext context) => _context = context;

    public async Task<Team?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Teams
            .Include(t => t.Players)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<IEnumerable<Team>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.Teams
            .Include(t => t.Players)
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Team team, CancellationToken cancellationToken = default) =>
        await _context.Teams.AddAsync(team, cancellationToken);

    public Task UpdateAsync(Team team, CancellationToken cancellationToken = default)
    {
        if (_context.Entry(team).State == EntityState.Detached)
            _context.Teams.Update(team);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var team = await _context.Teams.FindAsync(new object[] { id }, cancellationToken);
        if (team is not null)
            _context.Teams.Remove(team);
    }
}
