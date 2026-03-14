using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Features.Produto.Data;
using MeuCatalogo.Features.Produto.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Produto.Data.Remote.Contracts.Responses;

namespace MeuCatalogo.Features.Produto.UseCases;

public sealed class UpdateProdutoRemoteUseCase : IUseCase<(Guid Id, ProdutoUpdateRequest Request), ApiResponse<ProdutoResponse>>
{
    private readonly IProdutoRepository _repository;

    public UpdateProdutoRemoteUseCase(IProdutoRepository repository)
    {
        _repository = repository;
    }

    public Task<ApiResponse<ProdutoResponse>> ExecuteAsync((Guid Id, ProdutoUpdateRequest Request) request)
        => _repository.UpdateAsync(request.Id, request.Request);
}

