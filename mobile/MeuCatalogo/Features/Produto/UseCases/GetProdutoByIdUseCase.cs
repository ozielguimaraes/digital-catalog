using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Features.Produto.Data;
using MeuCatalogo.Features.Produto.Data.Remote.Contracts.Responses;

namespace MeuCatalogo.Features.Produto.UseCases;

public sealed class GetProdutoByIdUseCase(IProdutoRepository repository) : IUseCase<Guid, ApiResponse<ProdutoResponse>>
{
    public Task<ApiResponse<ProdutoResponse>> ExecuteAsync(Guid request)
        => repository.ObterPorIdAsync(request);
}

