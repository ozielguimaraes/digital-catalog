using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Features.Categoria.Data;
using MeuCatalogo.Features.Categoria.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Categoria.Domain;

namespace MeuCatalogo.Features.Categoria.UseCases;

public sealed class CreateCategoriaUseCase(ICategoriaRepository repository)
    : IUseCase<CategoriaUpsertRequest, ApiResponse<CategoriaInfo>>
{
    public Task<ApiResponse<CategoriaInfo>> ExecuteAsync(CategoriaUpsertRequest request)
        => repository.CreateAsync(request);
}
