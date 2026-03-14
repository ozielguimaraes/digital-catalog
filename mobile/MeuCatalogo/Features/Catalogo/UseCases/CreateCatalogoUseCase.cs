using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Features.Catalogo.Data;
using MeuCatalogo.Features.Catalogo.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Catalogo.Domain;

namespace MeuCatalogo.Features.Catalogo.UseCases;

public sealed class CreateCatalogoUseCase : IUseCase<CatalogoCreateRequest, ApiResponse<CatalogoInfo>>
{
    private readonly ICatalogoRepository _repository;

    public CreateCatalogoUseCase(ICatalogoRepository repository)
    {
        _repository = repository;
    }

    public Task<ApiResponse<CatalogoInfo>> ExecuteAsync(CatalogoCreateRequest request)
        => _repository.CreateAsync(request);
}
