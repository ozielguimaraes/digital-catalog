using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Features.Catalogo.Data.Local;
using MeuCatalogo.Features.Catalogo.Domain;

namespace MeuCatalogo.Features.Catalogo.UseCases;

public sealed class GetCatalogosLocalUseCase(ICatalogoLocalRepository localRepository) : IUseCaseOut<IReadOnlyList<CatalogoInfo>>
{
    public async Task<IReadOnlyList<CatalogoInfo>> ExecuteAsync()
    {
        var entities = await localRepository.GetAllAsync();
        return entities
            .Select(e => new CatalogoInfo
            {
                Id = Guid.Parse(e.Id),
                Nome = e.Nome,
                NomeCurto = e.NomeCurto,
                Email = e.Email,
                NumeroWhatsapp = e.NumeroWhatsapp,
                Descricao = e.Descricao
            })
            .ToList();
    }
}

