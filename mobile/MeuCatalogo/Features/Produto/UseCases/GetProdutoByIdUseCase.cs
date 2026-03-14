using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Features.Produto.Data;
using MeuCatalogo.Features.Produto.Data.Remote.Contracts.Responses;

namespace MeuCatalogo.Features.Produto.UseCases;

public sealed class GetProdutoByIdUseCase : IUseCase<Guid, ApiResponse<ProdutoResponse>>
{
    private readonly IProdutoRepository _repository;

    public GetProdutoByIdUseCase(IProdutoRepository repository)
    {
        _repository = repository;
    }

    public Task<ApiResponse<ProdutoResponse>> ExecuteAsync(Guid request)
        => _repository.ObterPorIdAsync(request);
}

