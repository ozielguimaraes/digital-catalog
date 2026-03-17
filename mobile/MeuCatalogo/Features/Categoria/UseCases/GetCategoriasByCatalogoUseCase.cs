using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Features.Categoria.Data;
using MeuCatalogo.Features.Categoria.Domain;

namespace MeuCatalogo.Features.Categoria.UseCases;

public sealed class GetCategoriasByCatalogoUseCase(ICategoriaRepository repository)
    : IUseCase<Guid, ApiResponse<IReadOnlyList<CategoriaInfo>>>
{
    public Task<ApiResponse<IReadOnlyList<CategoriaInfo>>> ExecuteAsync(Guid request)
        => repository.GetByCatalogoIdAsync(request);
}
