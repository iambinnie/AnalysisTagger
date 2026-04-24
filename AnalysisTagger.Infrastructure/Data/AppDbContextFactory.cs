using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AnalysisTagger.Infrastructure.Data;

// Used by dotnet-ef at design time to create migrations without a running app.
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=design_time_migrations.db")
            .Options;
        return new AppDbContext(options);
    }
}
