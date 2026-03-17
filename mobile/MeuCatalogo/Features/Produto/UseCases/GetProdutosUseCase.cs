using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Features.Produto.Data.Local;
using MeuCatalogo.Features.Produto.Domain;

namespace MeuCatalogo.Features.Produto.UseCases;

public class GetProdutosRequest
{
    public bool ForceRefresh { get; set; } = false;
}

public class GetProdutosUseCase(IProdutoLocalRepository repository) : IUseCase<GetProdutosRequest, IEnumerable<ProdutoEntity>>
{
    public async Task<IEnumerable<ProdutoEntity>> ExecuteAsync(GetProdutosRequest request)
    {
        if (request.ForceRefresh)
        {
            _ = Task.Run(() => repository.SyncWithRemoteAsync());
        }

        return await repository.GetAllAsync();
    }
}
