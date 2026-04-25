using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Features.Categoria.Data.Local;
using MeuCatalogo.Features.Categoria.Domain;

namespace MeuCatalogo.Features.Categoria.UseCases;

public sealed class GetCategoriasByCatalogoUseCase(
    ICategoriaLocalRepository localRepository,
    SyncCategoriasByCatalogoUseCase syncCategoriasByCatalogoUseCase,
    IConnectivity connectivity)
    : IUseCase<Guid, ApiResponse<IReadOnlyList<CategoriaInfo>>>
{
    public async Task<ApiResponse<IReadOnlyList<CategoriaInfo>>> ExecuteAsync(Guid request)
    {
        var catalogoId = request.ToString();

        var local = await localRepository.GetByCatalogoIdAsync(catalogoId);

        if (local.Count == 0 && connectivity.NetworkAccess == NetworkAccess.Internet)
        {
            await syncCategoriasByCatalogoUseCase.ExecuteAsync(request);
            local = await localRepository.GetByCatalogoIdAsync(catalogoId);
        }
        else if (connectivity.NetworkAccess == NetworkAccess.Internet)
        {
            _ = Task.Run(() => syncCategoriasByCatalogoUseCase.ExecuteAsync(request));
        }

        var mapped = local
            .Select(c => new CategoriaInfo
            {
                Id = Guid.Parse(c.Id),
                Nome = c.Nome,
                Descricao = c.Descricao,
                CatalogoId = Guid.Parse(c.CatalogoId)
            })
            .ToList();

        return ApiResponse<IReadOnlyList<CategoriaInfo>>.Success(mapped);
    }
}
