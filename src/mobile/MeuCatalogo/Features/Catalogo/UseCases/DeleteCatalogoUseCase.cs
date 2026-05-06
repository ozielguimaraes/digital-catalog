using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Features.Catalogo.Data;
using MeuCatalogo.Features.Catalogo.Data.Local;
using MeuCatalogo.Features.Catalogo.Domain;

namespace MeuCatalogo.Features.Catalogo.UseCases;

public sealed class DeleteCatalogoUseCase(
    ICatalogoRepository repository,
    ICatalogoLocalRepository localRepository) : IUseCase<Guid, ApiResponse<Guid>>
{
    public async Task<ApiResponse<Guid>> ExecuteAsync(Guid request)
    {
        var response = await repository.DeleteAsync(request);
        if (response.RetornouComErro) return response;

        var local = await localRepository.GetByIdAsync(request.ToString());
        if (local is not null) await localRepository.DeleteAsync(local);

        return response;
    }
}

