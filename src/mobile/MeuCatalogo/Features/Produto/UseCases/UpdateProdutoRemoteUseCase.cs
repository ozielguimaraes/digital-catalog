using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Features.Produto.Data;
using MeuCatalogo.Features.Produto.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Produto.Data.Remote.Contracts.Responses;

namespace MeuCatalogo.Features.Produto.UseCases;

public sealed class UpdateProdutoRemoteUseCase(IProdutoRepository repository)
    : IUseCase<(Guid Id, ProdutoUpdateRequest Request), ApiResponse<ProdutoResponse>>
{
    public Task<ApiResponse<ProdutoResponse>> ExecuteAsync((Guid Id, ProdutoUpdateRequest Request) request)
        => repository.UpdateAsync(request.Id, request.Request);
}

