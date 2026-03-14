using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Features.Catalogo.Data;

namespace MeuCatalogo.Features.Catalogo.UseCases;

public sealed class DeleteCatalogoUseCase : IUseCase<Guid, ApiResponse<Guid>>
{
    private readonly ICatalogoRepository _repository;

    public DeleteCatalogoUseCase(ICatalogoRepository repository)
    {
        _repository = repository;
    }

    public Task<ApiResponse<Guid>> ExecuteAsync(Guid request)
        => _repository.DeleteAsync(request);
}

