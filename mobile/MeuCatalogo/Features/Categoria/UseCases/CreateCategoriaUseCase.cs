using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Features.Categoria.Data;
using MeuCatalogo.Features.Categoria.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Categoria.Domain;

namespace MeuCatalogo.Features.Categoria.UseCases;

public sealed class CreateCategoriaUseCase : IUseCase<CategoriaUpsertRequest, ApiResponse<CategoriaInfo>>
{
    private readonly ICategoriaRepository _repository;

    public CreateCategoriaUseCase(ICategoriaRepository repository)
    {
        _repository = repository;
    }

    public Task<ApiResponse<CategoriaInfo>> ExecuteAsync(CategoriaUpsertRequest request)
        => _repository.CreateAsync(request);
}
