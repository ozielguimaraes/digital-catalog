using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Features.Catalogo.Data.Local;
using MeuCatalogo.Features.Catalogo.Domain;

namespace MeuCatalogo.Features.Catalogo.UseCases;

public sealed class GetCatalogosLocalUseCase : IUseCaseOut<IReadOnlyList<CatalogoInfo>>
{
    private readonly ICatalogoLocalRepository _localRepository;

    public GetCatalogosLocalUseCase(ICatalogoLocalRepository localRepository)
    {
        _localRepository = localRepository;
    }

    public async Task<IReadOnlyList<CatalogoInfo>> ExecuteAsync()
    {
        var entities = await _localRepository.GetAllAsync();
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

