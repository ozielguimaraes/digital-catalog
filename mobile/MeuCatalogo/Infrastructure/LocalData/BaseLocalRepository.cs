using SQLite;

namespace MeuCatalogo.Infrastructure.LocalData;

public abstract class BaseLocalRepository
{
    protected readonly SQLiteAsyncConnection Db;

    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _initialized;

    protected BaseLocalRepository(string databaseName = "meucatalogo.db3")
    {
        string dbPath = Path.Combine(FileSystem.AppDataDirectory, databaseName);
        Db = new SQLiteAsyncConnection(
            dbPath,
            SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);
    }

    protected async Task EnsureInitializedAsync()
    {
        if (_initialized)
            return;

        await _initLock.WaitAsync();
        try
        {
            if (_initialized)
                return;

            await CreateSchemaAsync();
            _initialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    protected abstract Task CreateSchemaAsync();
}
