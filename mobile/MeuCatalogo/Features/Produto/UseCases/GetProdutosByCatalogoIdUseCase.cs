using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Features.Produto.Data;
using MeuCatalogo.Features.Produto.Data.Remote.Contracts.Responses;

namespace MeuCatalogo.Features.Produto.UseCases;

public sealed class GetProdutosByCatalogoIdUseCase : IUseCase<Guid, ApiResponse<ICollection<ProdutoResponse>>>
{
    private readonly IProdutoRepository _repository;

    public GetProdutosByCatalogoIdUseCase(IProdutoRepository repository)
    {
        _repository = repository;
    }

    public Task<ApiResponse<ICollection<ProdutoResponse>>> ExecuteAsync(Guid request)
        => _repository.ObterPorCatalogoIdAsync(request);
}

