using MeuCatalogo.Application.DTOs.Requests;
using MeuCatalogo.Application.DTOs.Responses;

namespace MeuCatalogo.Application.Interfaces;

public interface ILancamentoBaixaService
{
    Task<ApiResponse<List<LancamentoBaixaResponse>>> ListarAsync(Guid lancamentoId, string userId);
    Task<ApiResponse<LancamentoBaixaResponse>> AdicionarAsync(Guid lancamentoId, LancamentoBaixaRequest request, string userId);
    Task<ApiResponse<bool>> RemoverAsync(Guid lancamentoId, Guid baixaId, string userId);
}
