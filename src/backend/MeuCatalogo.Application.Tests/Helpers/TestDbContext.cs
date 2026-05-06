using MeuCatalogo.Application.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace MeuCatalogo.Application.Tests.Helpers;

public sealed class TestDbContext : IAsyncDisposable
{
    private readonly SqliteConnection _connection;

    public ApplicationDbContext Db { get; }

    public TestDbContext()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        // FKs desligadas: testes não populam tabelas de Identity (ApplicationUser).
        using (var cmd = _connection.CreateCommand())
        {
            cmd.CommandText = "PRAGMA foreign_keys = OFF;";
            cmd.ExecuteNonQuery();
        }

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        Db = new ApplicationDbContext(options);
        Db.Database.EnsureCreated();
    }

    public async ValueTask DisposeAsync()
    {
        await Db.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
