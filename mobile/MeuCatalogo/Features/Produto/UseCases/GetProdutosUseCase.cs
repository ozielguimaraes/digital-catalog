using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Features.Produto.Data.Local;
using MeuCatalogo.Features.Produto.Domain;

namespace MeuCatalogo.Features.Produto.UseCases;

public class GetProdutosRequest
{
    public bool ForceRefresh { get; set; } = false;
}

public class GetProdutosUseCase : IUseCase<GetProdutosRequest, IEnumerable<ProdutoEntity>>
{
    private readonly IProdutoLocalRepository _repository;

    public GetProdutosUseCase(IProdutoLocalRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ProdutoEntity>> ExecuteAsync(GetProdutosRequest request)
    {
        if (request.ForceRefresh)
        {
            _ = Task.Run(() => _repository.SyncWithRemoteAsync());
        }

        return await _repository.GetAllAsync();
    }
}
