using AnalysisTagger.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AnalysisTagger.Tests.Infrastructure.Helpers;

/// <summary>
/// Holds an open in-memory SQLite connection for the lifetime of a test.
/// All contexts created from one fixture share the same database.
/// </summary>
public sealed class SqliteTestFixture : IDisposable
{
    private readonly SqliteConnection _connection;
    private bool _schemaCreated;

    public SqliteTestFixture()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
    }

    public AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        var context = new AppDbContext(options);

        if (!_schemaCreated)
        {
            context.Database.EnsureCreated();
            _schemaCreated = true;
        }

        return context;
    }

    public void Dispose() => _connection.Dispose();
}
