using AnalysisTagger.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AnalysisTagger.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<TagTemplate> TagTemplates => Set<TagTemplate>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<EventTag> EventTags => Set<EventTag>();
    public DbSet<DrawingAnnotation> DrawingAnnotations => Set<DrawingAnnotation>();
    public DbSet<Playlist> Playlists => Set<Playlist>();
    public DbSet<PlaylistEntry> PlaylistEntries => Set<PlaylistEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
