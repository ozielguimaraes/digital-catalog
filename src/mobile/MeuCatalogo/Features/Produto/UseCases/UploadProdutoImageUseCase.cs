using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Features.Produto.Data;
using MeuCatalogo.Features.Produto.Data.Remote.Contracts.Responses;

namespace MeuCatalogo.Features.Produto.UseCases;

public sealed class UploadProdutoImageUseCase(IProdutoRepository repository)
    : IUseCase<(Guid ProdutoId, FileResult File, bool IsPrincipal, int Ordem), ApiResponse<ProdutoImagemResponse>>
{
    public Task<ApiResponse<ProdutoImagemResponse>> ExecuteAsync((Guid ProdutoId, FileResult File, bool IsPrincipal, int Ordem) request)
        => repository.UploadImageAsync(request.ProdutoId, request.File, request.IsPrincipal, request.Ordem);
}

