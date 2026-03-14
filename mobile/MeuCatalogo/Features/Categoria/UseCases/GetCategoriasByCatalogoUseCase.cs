using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Features.Categoria.Data;
using MeuCatalogo.Features.Categoria.Domain;

namespace MeuCatalogo.Features.Categoria.UseCases;

public sealed class GetCategoriasByCatalogoUseCase : IUseCase<Guid, ApiResponse<IReadOnlyList<CategoriaInfo>>>
{
    private readonly ICategoriaRepository _repository;

    public GetCategoriasByCatalogoUseCase(ICategoriaRepository repository)
    {
        _repository = repository;
    }

    public Task<ApiResponse<IReadOnlyList<CategoriaInfo>>> ExecuteAsync(Guid request)
        => _repository.GetByCatalogoIdAsync(request);
}
