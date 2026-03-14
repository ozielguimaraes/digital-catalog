using MeuCatalogo.Core.Abstractions;
using MeuCatalogo.Features.Produto.Data;
using MeuCatalogo.Features.Produto.Data.Remote.Contracts.Responses;

namespace MeuCatalogo.Features.Produto.UseCases;

public sealed class UploadProdutoImageUseCase : IUseCase<(Guid ProdutoId, FileResult File), ApiResponse<ProdutoImagemResponse>>
{
    private readonly IProdutoRepository _repository;

    public UploadProdutoImageUseCase(IProdutoRepository repository)
    {
        _repository = repository;
    }

    public Task<ApiResponse<ProdutoImagemResponse>> ExecuteAsync((Guid ProdutoId, FileResult File) request)
        => _repository.UploadImageAsync(request.ProdutoId, request.File);
}

