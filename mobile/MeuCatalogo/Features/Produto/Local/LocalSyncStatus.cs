namespace MeuCatalogo.Features.Produto.Local;

public enum LocalSyncStatus
{
    Synced = 0,
    PendingCreate = 1,
    PendingUpdate = 2,
    PendingDelete = 3,
    SyncFailed = 4
}
