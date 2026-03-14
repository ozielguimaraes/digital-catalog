using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Features.Catalogo.Data;
using MeuCatalogo.Features.Catalogo.Domain;

namespace MeuCatalogo.Features.Catalogo.UseCases;

public sealed class GetCatalogosUseCase : IUseCaseOut<ApiResponse<IReadOnlyList<CatalogoInfo>>>
{
    private readonly ICatalogoRepository _repository;

    public GetCatalogosUseCase(ICatalogoRepository repository)
    {
        _repository = repository;
    }

    public Task<ApiResponse<IReadOnlyList<CatalogoInfo>>> ExecuteAsync()
        => _repository.GetCatalogosAsync();
}
