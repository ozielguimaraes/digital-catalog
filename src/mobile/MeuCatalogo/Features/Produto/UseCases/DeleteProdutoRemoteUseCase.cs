using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Features.Produto.Data;

namespace MeuCatalogo.Features.Produto.UseCases;

public sealed class DeleteProdutoRemoteUseCase(IProdutoRepository repository) : IUseCase<Guid, ApiResponse<Guid>>
{
    public Task<ApiResponse<Guid>> ExecuteAsync(Guid request)
        => repository.DeleteAsync(request);
}

