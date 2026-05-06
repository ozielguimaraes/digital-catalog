using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Features.Categoria.Data;
using MeuCatalogo.Features.Categoria.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Categoria.Domain;

namespace MeuCatalogo.Features.Categoria.UseCases;

public sealed class UpdateCategoriaUseCase(ICategoriaRepository repository)
    : IUseCase<(Guid Id, CategoriaUpsertRequest Request), ApiResponse<CategoriaInfo>>
{
    public Task<ApiResponse<CategoriaInfo>> ExecuteAsync((Guid Id, CategoriaUpsertRequest Request) request)
        => repository.UpdateAsync(request.Id, request.Request);
}
