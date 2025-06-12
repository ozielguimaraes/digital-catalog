using MeuCatalogo.Features.Produto.Requests;
using MeuCatalogo.Features.Produto.Responses;
using Microsoft.Maui.Controls.Internals;
using Refit;

namespace MeuCatalogo.Features.Produto.ApiClients;

[Preserve(AllMembers = true)]
public interface IProdutoApi
{
    [Post("/produto")]
    Task<ProdutoResponse> SignupAsync([Body] ProdutoCreateRequest request, CancellationToken ct = default);
}
