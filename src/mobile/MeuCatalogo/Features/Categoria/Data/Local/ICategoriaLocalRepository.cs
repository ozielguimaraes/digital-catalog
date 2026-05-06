using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Features.Categoria.Domain;

namespace MeuCatalogo.Features.Categoria.Data.Local;

public interface ICategoriaLocalRepository : IRepository<CategoriaEntity>
{
    Task<IReadOnlyList<CategoriaEntity>> GetByCatalogoIdAsync(string catalogoId);
    Task ReplaceByCatalogoIdAsync(string catalogoId, IEnumerable<CategoriaEntity> categorias);
    Task UpsertAsync(CategoriaEntity categoria);
}

