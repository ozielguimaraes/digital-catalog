using MeuCatalogo.Features.Produto.Local.Entities;
using MeuCatalogo.Features.Produto.Responses;
using MeuCatalogo.Infrastructure.LocalData;

namespace MeuCatalogo.Features.Produto.Local;

public sealed class ProdutoLocalRepository : BaseLocalRepository, IProdutoLocalRepository
{
    public ProdutoLocalRepository()
        : base()
    {
    }

    public async Task SaveCatalogoProdutosFromRemoteAsync(Guid catalogoId, IEnumerable<ProdutoResponse> produtos, CancellationToken ct = default)
    {
        await EnsureInitializedAsync();

        var normalized = produtos.ToList();
        var incomingIds = normalized.Select(p => p.Id).ToHashSet();

        var localCatalogoProdutos = await Db.Table<ProdutoLocalEntity>()
            .Where(p => p.CatalogoId == catalogoId)
            .ToListAsync();

        foreach (var local in localCatalogoProdutos.Where(local => !incomingIds.Contains(local.Id) && local.SyncStatus == (int)LocalSyncStatus.Synced))
        {
            await Db.ExecuteAsync("DELETE FROM produto_imagens WHERE ProdutoId = ?", local.Id);
            await Db.DeleteAsync(local);
        }

        foreach (var produto in normalized)
        {
            await SaveProdutoInternalAsync(produto, LocalSyncStatus.Synced);
        }
    }

    public Task SaveProdutoFromRemoteAsync(ProdutoResponse produto, CancellationToken ct = default)
    {
        return SaveProdutoInternalAsync(produto, LocalSyncStatus.Synced);
    }

    public Task SaveProdutoOfflineAsync(ProdutoResponse produto, LocalSyncStatus syncStatus, CancellationToken ct = default)
    {
        return SaveProdutoInternalAsync(produto, syncStatus);
    }

    public async Task SaveProdutoImagemOfflineAsync(Guid produtoId, ProdutoImagemResponse imagem, LocalSyncStatus syncStatus, CancellationToken ct = default)
    {
        await EnsureInitializedAsync();

        var entity = new ProdutoImagemLocalEntity
        {
            Id = imagem.Id,
            ProdutoId = produtoId,
            Url = imagem.Url,
            ThumbnailUrl = imagem.Images.Thumbnail,
            MediumUrl = imagem.Images.Medium,
            FullUrl = imagem.Images.Full,
            IsPrincipal = imagem.IsPrincipal,
            Ordem = imagem.Ordem,
            SyncStatus = (int)syncStatus,
            UpdatedAtUtc = DateTime.UtcNow
        };

        await Db.InsertOrReplaceAsync(entity);
    }

    public async Task<ICollection<ProdutoResponse>> GetProdutosByCatalogoIdAsync(Guid catalogoId, CancellationToken ct = default)
    {
        await EnsureInitializedAsync();

        var produtos = await Db.Table<ProdutoLocalEntity>()
            .Where(p => p.CatalogoId == catalogoId && p.SyncStatus != (int)LocalSyncStatus.PendingDelete)
            .OrderBy(p => p.Nome)
            .ToListAsync();

        var result = new List<ProdutoResponse>(produtos.Count);
        foreach (var produto in produtos)
        {
            var imagens = await GetProdutoImagensAsync(produto.Id);
            result.Add(MapToResponse(produto, imagens));
        }

        return result;
    }

    public async Task<ProdutoResponse?> GetProdutoByIdAsync(Guid produtoId, CancellationToken ct = default)
    {
        await EnsureInitializedAsync();

        var produto = await Db.FindAsync<ProdutoLocalEntity>(produtoId);
        if (produto == null || produto.SyncStatus == (int)LocalSyncStatus.PendingDelete)
            return null;

        var imagens = await GetProdutoImagensAsync(produtoId);
        return MapToResponse(produto, imagens);
    }

    public async Task MarkProdutoAsDeletedAsync(Guid produtoId, CancellationToken ct = default)
    {
        await EnsureInitializedAsync();

        var produto = await Db.FindAsync<ProdutoLocalEntity>(produtoId);
        if (produto == null)
            return;

        produto.SyncStatus = (int)LocalSyncStatus.PendingDelete;
        produto.UpdatedAtUtc = DateTime.UtcNow;
        await Db.UpdateAsync(produto);
    }

    public async Task RemoveProdutoAsync(Guid produtoId, CancellationToken ct = default)
    {
        await EnsureInitializedAsync();
        await Db.ExecuteAsync("DELETE FROM produto_imagens WHERE ProdutoId = ?", produtoId);
        await Db.DeleteAsync<ProdutoLocalEntity>(produtoId);
    }

    private async Task SaveProdutoInternalAsync(ProdutoResponse produto, LocalSyncStatus syncStatus)
    {
        await EnsureInitializedAsync();

        var entity = new ProdutoLocalEntity
        {
            Id = produto.Id,
            CatalogoId = produto.CatalogoId,
            Nome = produto.Nome,
            Preco = produto.Preco,
            PrecoComDesconto = produto.PrecoComDesconto,
            InformacoesAdicionais = produto.InformacoesAdicionais,
            CategoriaId = produto.CategoriaId,
            CategoriaNome = produto.CategoriaNome,
            EstoqueQuantidade = produto.Estoque?.Quantidade,
            EstoqueQuantidadeMinima = produto.Estoque?.QuantidadeMinima,
            EstoqueQuantidadeMaxima = produto.Estoque?.QuantidadeMaxima,
            EstoqueDisponivel = produto.Estoque?.Disponivel ?? true,
            EstoqueEhIlimitado = produto.Estoque?.EhIlimitado ?? true,
            DataCriacao = DateTime.UtcNow,
            DataAtualizacao = DateTime.UtcNow,
            SyncStatus = (int)syncStatus,
            UpdatedAtUtc = DateTime.UtcNow
        };

        await Db.InsertOrReplaceAsync(entity);
        await Db.ExecuteAsync("DELETE FROM produto_imagens WHERE ProdutoId = ?", produto.Id);

        var imagens = produto.Imagens ?? [];
        foreach (var imagem in imagens)
        {
            var imgEntity = new ProdutoImagemLocalEntity
            {
                Id = imagem.Id,
                ProdutoId = produto.Id,
                Url = imagem.Url,
                ThumbnailUrl = imagem.Images.Thumbnail,
                MediumUrl = imagem.Images.Medium,
                FullUrl = imagem.Images.Full,
                IsPrincipal = imagem.IsPrincipal,
                Ordem = imagem.Ordem,
                SyncStatus = (int)syncStatus,
                UpdatedAtUtc = DateTime.UtcNow
            };

            await Db.InsertOrReplaceAsync(imgEntity);
        }
    }

    private async Task<List<ProdutoImagemLocalEntity>> GetProdutoImagensAsync(Guid produtoId)
    {
        return await Db.Table<ProdutoImagemLocalEntity>()
            .Where(i => i.ProdutoId == produtoId && i.SyncStatus != (int)LocalSyncStatus.PendingDelete)
            .OrderBy(i => i.Ordem)
            .ToListAsync();
    }

    private static ProdutoResponse MapToResponse(ProdutoLocalEntity entity, List<ProdutoImagemLocalEntity> imagens)
    {
        var response = new ProdutoResponse
        {
            Id = entity.Id,
            CatalogoId = entity.CatalogoId,
            Nome = entity.Nome,
            Preco = entity.Preco,
            PrecoComDesconto = entity.PrecoComDesconto,
            InformacoesAdicionais = entity.InformacoesAdicionais,
            CategoriaId = entity.CategoriaId,
            CategoriaNome = entity.CategoriaNome,
            Estoque = new EstoqueResponse
            {
                ProdutoId = entity.Id,
                Quantidade = entity.EstoqueQuantidade,
                QuantidadeMinima = entity.EstoqueQuantidadeMinima,
                QuantidadeMaxima = entity.EstoqueQuantidadeMaxima,
                Disponivel = entity.EstoqueDisponivel,
                EhIlimitado = entity.EstoqueEhIlimitado
            },
            Imagens = imagens.Select(i => new ProdutoImagemResponse
            {
                Id = i.Id,
                Url = i.Url,
                Images = new ProdutoImagemLinksResponse
                {
                    Thumbnail = i.ThumbnailUrl,
                    Medium = i.MediumUrl,
                    Full = i.FullUrl
                },
                IsPrincipal = i.IsPrincipal,
                Ordem = i.Ordem,
                SyncStatus = (LocalSyncStatus)i.SyncStatus
            }).ToList(),
            SyncStatus = (LocalSyncStatus)entity.SyncStatus
        };

        return response;
    }

    protected override async Task CreateSchemaAsync()
    {
        await Db.CreateTableAsync<ProdutoLocalEntity>();
        await Db.CreateTableAsync<ProdutoImagemLocalEntity>();
        await Db.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_produto_imagens_produtoid ON produto_imagens(ProdutoId)");
    }
}
