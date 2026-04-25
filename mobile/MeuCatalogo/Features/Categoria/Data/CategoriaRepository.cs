using MeuCatalogo.Domain.Enums;
using MeuCatalogo.Features.Categoria.Data.Local;
using MeuCatalogo.Features.Categoria.Data.Remote;
using MeuCatalogo.Features.Categoria.Data.Remote.Contracts.Requests;
using MeuCatalogo.Features.Categoria.Domain;

namespace MeuCatalogo.Features.Categoria.Data;

public sealed class CategoriaRepository(
    ICategoriaRemoteDataSource remote,
    ICategoriaLocalRepository local)
    : ICategoriaRepository
{
    public Task<ApiResponse<CategoriaInfo>> GetByIdAsync(Guid id, CancellationToken ct = default)
        => remote.GetByIdAsync(id, ct);

    public Task<ApiResponse<IReadOnlyList<CategoriaInfo>>> GetByCatalogoIdAsync(Guid catalogoId, CancellationToken ct = default)
        => remote.GetByCatalogoIdAsync(catalogoId, ct);

    public async Task<ApiResponse<CategoriaInfo>> CreateAsync(CategoriaUpsertRequest request, CancellationToken ct = default)
    {
        var response = await remote.CreateAsync(request, ct);
        if (response.RetornouComErro || response.Dados == null)
            return response;

        await local.UpsertAsync(new CategoriaEntity
        {
            Id = response.Dados.Id.ToString(),
            Nome = response.Dados.Nome,
            Descricao = response.Dados.Descricao,
            CatalogoId = response.Dados.CatalogoId.ToString(),
            SyncStatus = SyncStatus.Completed,
            LastModified = DateTime.UtcNow
        });

        return response;
    }

    public async Task<ApiResponse<CategoriaInfo>> UpdateAsync(Guid id, CategoriaUpsertRequest request, CancellationToken ct = default)
    {
        var response = await remote.UpdateAsync(id, request, ct);
        if (response.RetornouComErro || response.Dados == null)
            return response;

        await local.UpsertAsync(new CategoriaEntity
        {
            Id = response.Dados.Id.ToString(),
            Nome = response.Dados.Nome,
            Descricao = response.Dados.Descricao,
            CatalogoId = response.Dados.CatalogoId.ToString(),
            SyncStatus = SyncStatus.Completed,
            LastModified = DateTime.UtcNow
        });

        return response;
    }
}
