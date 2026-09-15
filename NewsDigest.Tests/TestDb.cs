using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NewsDigest.Infrastructure.Data;

namespace NewsDigest.Tests;

internal static class TestDb
{
    public static AppDbContext CreateContext()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}