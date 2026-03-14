using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Features.Produto.Data;

namespace MeuCatalogo.Features.Produto.UseCases;

public sealed class DeleteProdutoRemoteUseCase : IUseCase<Guid, ApiResponse<Guid>>
{
    private readonly IProdutoRepository _repository;

    public DeleteProdutoRemoteUseCase(IProdutoRepository repository)
    {
        _repository = repository;
    }

    public Task<ApiResponse<Guid>> ExecuteAsync(Guid request)
        => _repository.DeleteAsync(request);
}

