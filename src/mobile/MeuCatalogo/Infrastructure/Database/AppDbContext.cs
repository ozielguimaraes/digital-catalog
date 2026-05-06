using System.Linq;
using System.Threading;
using MeuCatalogo.Domain.Entities;
using MeuCatalogo.Features.Auth.Domain;
using MeuCatalogo.Features.Catalogo.Domain;
using MeuCatalogo.Features.Categoria.Domain;
using MeuCatalogo.Features.Produto.Domain;
using SQLite;

namespace MeuCatalogo.Infrastructure.Database;

public class AppDbContext
{
    private SQLiteAsyncConnection? _database;
    private bool _initialized;

    private readonly SemaphoreSlim _initLock = new(1, 1);

    public SQLiteAsyncConnection Database =>
        _database ?? throw new InvalidOperationException("Call InitializeAsync first");

    public async Task InitializeAsync()
    {
        if (_initialized)
            return;

        await _initLock.WaitAsync();

        try
        {
            if (_initialized)
                return;

            _database = new SQLiteAsyncConnection(
                DatabaseConstants.DatabasePath,
                DatabaseConstants.Flags);

            await ConfigureDatabaseAsync();
            await CreateTablesAsync();
            await EnsureCurrentSchemaAsync();
            await CreateIndexesAsync();

            _initialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    private async Task ConfigureDatabaseAsync()
    {
        if (_database == null)
            return;

        try
        {
            _ = await _database.ExecuteScalarAsync<string>("PRAGMA journal_mode=WAL;");
        }
        catch
        {
        }

        try
        {
            _ = await _database.ExecuteScalarAsync<int>("PRAGMA foreign_keys=ON;");
        }
        catch
        {
        }
    }

    private async Task CreateTablesAsync()
    {
        if (_database == null)
            return;

        await _database.CreateTableAsync<SyncQueue>();
        await _database.CreateTableAsync<UserEntity>();
        await _database.CreateTableAsync<CatalogoEntity>();
        await _database.CreateTableAsync<CategoriaEntity>();
        await _database.CreateTableAsync<ProdutoEntity>();
        await _database.CreateTableAsync<ProdutoImagemEntity>();
    }

    private async Task CreateIndexesAsync()
    {
        if (_database == null)
            return;

        await _database.ExecuteAsync(
            "CREATE INDEX IF NOT EXISTS idx_categoria_catalogo ON Categorias (CatalogoId)");

        await _database.ExecuteAsync(
            "CREATE INDEX IF NOT EXISTS idx_produto_catalogo ON Produtos (CatalogoId)");

        await _database.ExecuteAsync(
            "CREATE INDEX IF NOT EXISTS idx_produto_categoria ON Produtos (CategoriaId)");
    }

    private async Task EnsureCurrentSchemaAsync()
    {
        if (_database == null)
            return;

        await EnsureColumnExistsAsync("Produtos", "PrecoComDesconto", "REAL");
        await EnsureColumnExistsAsync("Produtos", "InformacoesAdicionais", "TEXT");
        await EnsureColumnExistsAsync("Produtos", "CatalogoId", "TEXT");
        await EnsureColumnExistsAsync("Produtos", "CategoriaNome", "TEXT");
        await EnsureColumnExistsAsync("Produtos", "ThumbnailUrl", "TEXT");
        await EnsureColumnExistsAsync("SyncQueues", "NextRetryAt", "TEXT");
        await EnsureColumnExistsAsync("SyncQueues", "CompletedAt", "TEXT");
    }

    private async Task EnsureColumnExistsAsync(
        string tableName,
        string columnName,
        string sqliteType)
    {
        if (_database == null)
            return;

        var info = await _database.GetTableInfoAsync(tableName);

        if (info.Any(c => string.Equals(c.Name, columnName,
            StringComparison.OrdinalIgnoreCase)))
            return;

        await _database.ExecuteAsync(
            $"ALTER TABLE {tableName} ADD COLUMN {columnName} {sqliteType}");
    }
}
