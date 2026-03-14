using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Features.Categoria.Data;
using MeuCatalogo.Features.Categoria.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Categoria.Domain;

namespace MeuCatalogo.Features.Categoria.UseCases;

public sealed class UpdateCategoriaUseCase : IUseCase<(Guid Id, CategoriaUpsertRequest Request), ApiResponse<CategoriaInfo>>
{
    private readonly ICategoriaRepository _repository;

    public UpdateCategoriaUseCase(ICategoriaRepository repository)
    {
        _repository = repository;
    }

    public Task<ApiResponse<CategoriaInfo>> ExecuteAsync((Guid Id, CategoriaUpsertRequest Request) request)
        => _repository.UpdateAsync(request.Id, request.Request);
}
