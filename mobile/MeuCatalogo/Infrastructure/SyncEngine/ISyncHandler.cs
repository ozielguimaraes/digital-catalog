using MeuCatalogo.Domain.Entities;

namespace MeuCatalogo.Infrastructure.SyncEngine;

public interface ISyncHandler
{
    bool CanHandle(SyncQueue item);
    Task HandleAsync(SyncQueue item, CancellationToken ct = default);
}

