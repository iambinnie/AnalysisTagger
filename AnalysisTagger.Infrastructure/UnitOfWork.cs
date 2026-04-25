using AnalysisTagger.Application.Interfaces;
using AnalysisTagger.Infrastructure.Data;
using AnalysisTagger.Infrastructure.Repositories;

namespace AnalysisTagger.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private ProjectRepository? _projects;

    public UnitOfWork(AppDbContext context) => _context = context;

    public IProjectRepository Projects => _projects ??= new ProjectRepository(_context);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);

    public void Dispose() { }
}
