using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Features.Produto.Data;
using MeuCatalogo.Features.Produto.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Produto.Data.Remote.Contracts.Responses;

namespace MeuCatalogo.Features.Produto.UseCases;

public sealed class CreateProdutoRemoteUseCase(IProdutoRepository repository)
    : IUseCase<ProdutoCreateRequest, ApiResponse<ProdutoResponse>>
{
    public Task<ApiResponse<ProdutoResponse>> ExecuteAsync(ProdutoCreateRequest request)
        => repository.CreateAsync(request);
}

