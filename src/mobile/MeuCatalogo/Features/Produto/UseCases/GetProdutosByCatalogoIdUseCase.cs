using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Features.Produto.Data;
using MeuCatalogo.Features.Produto.Data.Remote.Contracts.Responses;

namespace MeuCatalogo.Features.Produto.UseCases;

public sealed class GetProdutosByCatalogoIdUseCase(IProdutoRepository repository)
    : IUseCase<Guid, ApiResponse<ICollection<ProdutoResponse>>>
{
    public Task<ApiResponse<ICollection<ProdutoResponse>>> ExecuteAsync(Guid request)
        => repository.ObterPorCatalogoIdAsync(request);
}

