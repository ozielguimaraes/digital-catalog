using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Features.Catalogo.Data;
using MeuCatalogo.Features.Catalogo.Domain;

namespace MeuCatalogo.Features.Catalogo.UseCases;

public sealed class GetCatalogosUseCase(ICatalogoRepository repository) : IUseCaseOut<ApiResponse<IReadOnlyList<CatalogoInfo>>>
{
    public Task<ApiResponse<IReadOnlyList<CatalogoInfo>>> ExecuteAsync()
        => repository.GetCatalogosAsync();
}
