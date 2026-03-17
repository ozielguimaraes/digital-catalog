using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Features.Catalogo.Data;

namespace MeuCatalogo.Features.Catalogo.UseCases;

public sealed class DeleteCatalogoUseCase(ICatalogoRepository repository) : IUseCase<Guid, ApiResponse<Guid>>
{
    public Task<ApiResponse<Guid>> ExecuteAsync(Guid request)
        => repository.DeleteAsync(request);
}

