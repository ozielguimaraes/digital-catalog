using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Features.Produto.Data;
using MeuCatalogo.Features.Produto.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Produto.Data.Remote.Contracts.Responses;

namespace MeuCatalogo.Features.Produto.UseCases;

public sealed class CreateProdutoRemoteUseCase : IUseCase<ProdutoCreateRequest, ApiResponse<ProdutoResponse>>
{
    private readonly IProdutoRepository _repository;

    public CreateProdutoRemoteUseCase(IProdutoRepository repository)
    {
        _repository = repository;
    }

    public Task<ApiResponse<ProdutoResponse>> ExecuteAsync(ProdutoCreateRequest request)
        => _repository.CreateAsync(request);
}

