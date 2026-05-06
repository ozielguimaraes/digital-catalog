using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Features.Catalogo.Domain;

namespace MeuCatalogo.Features.Catalogo.Data.Local;

public interface ICatalogoLocalRepository : IRepository<CatalogoEntity>
{
    Task ReplaceAllAsync(IEnumerable<CatalogoEntity> catalogos);
}

