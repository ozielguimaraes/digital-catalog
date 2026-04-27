using MeuCatalogo.Features.Fornecedor.Data;
using MeuCatalogo.Features.Fornecedor.Domain;

namespace MeuCatalogo.Features.Fornecedor.UseCases;

public sealed class GetFornecedoresUseCase(IFornecedorRepository repository)
{
    public Task<ApiResponse<IReadOnlyList<FornecedorInfo>>> ExecuteAsync(CancellationToken ct = default)
        => repository.GetAllAsync(ct);
}
