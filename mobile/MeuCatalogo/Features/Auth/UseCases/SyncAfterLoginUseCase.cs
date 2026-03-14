using System.Linq;
using System.Text.Json;
using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Domain.Enums;
using MeuCatalogo.Features.Catalogo.Data.Local;
using MeuCatalogo.Features.Catalogo.Domain;
using MeuCatalogo.Features.Produto.Data.Local;
using MeuCatalogo.Features.Settings.Services;
using MeuCatalogo.Infrastructure.SyncEngine;
using Microsoft.Extensions.Logging;

namespace MeuCatalogo.Features.Auth.UseCases;

public sealed class SyncAfterLoginUseCase : IUseCaseOut<SyncAfterLoginResult>
{
    private readonly ILogger<SyncAfterLoginUseCase> _logger;
    private readonly ISyncEngine _syncEngine;
    private readonly ICatalogoLocalRepository _catalogoLocalRepository;
    private readonly IProdutoLocalRepository _produtoLocalRepository;
    private readonly ISettingsService _settingsService;

    public SyncAfterLoginUseCase(
        ILogger<SyncAfterLoginUseCase> logger,
        ISyncEngine syncEngine,
        ICatalogoLocalRepository catalogoLocalRepository,
        IProdutoLocalRepository produtoLocalRepository,
        ISettingsService settingsService)
    {
        _logger = logger;
        _syncEngine = syncEngine;
        _catalogoLocalRepository = catalogoLocalRepository;
        _produtoLocalRepository = produtoLocalRepository;
        _settingsService = settingsService;
    }

    public async Task<SyncAfterLoginResult> ExecuteAsync()
    {
        await _syncEngine.QueueSyncAsync(SyncEntityTypes.Catalogos, "all", SyncOperation.PullCatalogos, "{}");
        await _syncEngine.ProcessQueueAsync();

        var catalogos = (await _catalogoLocalRepository.GetAllAsync()).ToList();
        if (catalogos.Count == 0)
            return new SyncAfterLoginResult(0, 0, false);

        var primeiro = catalogos[0];

        bool favoritoFoiDefinido = false;
        if (_settingsService.CatalogoFavorito == null || !catalogos.Any(c => Guid.TryParse(c.Id, out var id) && id == _settingsService.CatalogoFavorito.Id))
        {
            _settingsService.CatalogoFavorito = ToInfo(primeiro);
            favoritoFoiDefinido = true;
        }

        var payload = JsonSerializer.Serialize(new PullProdutosByCatalogoPayload { CatalogoId = primeiro.Id });
        await _syncEngine.QueueSyncAsync(SyncEntityTypes.ProdutosByCatalogo, primeiro.Id, SyncOperation.PullProdutosByCatalogoId, payload);
        await _syncEngine.ProcessQueueAsync();

        var produtos = await _produtoLocalRepository.GetByCatalogoIdAsync(primeiro.Id);

        _logger.LogInformation("Sincronização pós-login concluída. Catalogos={Catalogos} Produtos={Produtos} FavoritoDefinido={Favorito}", catalogos.Count, produtos.Count(), favoritoFoiDefinido);
        return new SyncAfterLoginResult(catalogos.Count, produtos.Count(), favoritoFoiDefinido);
    }

    private static CatalogoInfo ToInfo(CatalogoEntity entity)
    {
        return new CatalogoInfo
        {
            Id = Guid.Parse(entity.Id),
            Nome = entity.Nome,
            NomeCurto = entity.NomeCurto,
            Email = entity.Email,
            NumeroWhatsapp = entity.NumeroWhatsapp,
            Descricao = entity.Descricao
        };
    }

    private sealed record PullProdutosByCatalogoPayload
    {
        public required string CatalogoId { get; init; }
    }
}

public sealed record SyncAfterLoginResult(int CatalogosSincronizados, int ProdutosSincronizados, bool FavoritoFoiDefinido);
