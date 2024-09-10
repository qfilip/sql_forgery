using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SqlForgery.Tests.Database;

namespace SqlForgery.Tests.Utils;

internal sealed class ContextCreator : IDisposable
{
    private bool disposed = false;
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<TestDbContext> _options;
    public ContextCreator()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(_connection)
            .Options;

        var context = new TestDbContext(_options);
        context.Database.EnsureCreated();
    }

    public TestDbContext GetTestDbContext()
    {
        return new TestDbContext(_options);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposed) return;

        if(disposing)
            _connection.Dispose();

        disposed = true;
    }

    ~ContextCreator()
    {
        Dispose(false);
    }
}
