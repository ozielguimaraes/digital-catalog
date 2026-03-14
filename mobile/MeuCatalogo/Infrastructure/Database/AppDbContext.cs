using System.Linq;
using MeuCatalogo.Domain.Entities;
using MeuCatalogo.Features.Auth.Domain;
using MeuCatalogo.Features.Catalogo.Domain;
using MeuCatalogo.Features.Produto.Domain;
using SQLite;

namespace MeuCatalogo.Infrastructure.Database;

public class AppDbContext
{
    private SQLiteAsyncConnection? _database;
    private bool _initialized = false;

    public AppDbContext()
    {
    }

    public SQLiteAsyncConnection Database
    {
        get
        {
            return _database ?? throw new InvalidOperationException("Call InitializeAsync first");
        }
    }

    public async Task InitializeAsync()
    {
        if (_initialized)
            return;

        _database = new SQLiteAsyncConnection(DatabaseConstants.DatabasePath, DatabaseConstants.Flags);

        await _database.CreateTableAsync<SyncQueue>();
        await _database.CreateTableAsync<UserEntity>();
        await _database.CreateTableAsync<CatalogoEntity>();
        await _database.CreateTableAsync<ProdutoEntity>();
        await _database.CreateTableAsync<ProdutoImagemEntity>();

        _initialized = true;
    }

    private async Task EnsureColumnExistsAsync(string tableName, string columnName, string sqliteType)
    {
        if (_database == null)
            return;

        var info = await _database.GetTableInfoAsync(tableName);
        if (info.Any(c => string.Equals(c.Name, columnName, StringComparison.OrdinalIgnoreCase)))
            return;

        await _database.ExecuteAsync($"ALTER TABLE {tableName} ADD COLUMN {columnName} {sqliteType}");
    }
}
