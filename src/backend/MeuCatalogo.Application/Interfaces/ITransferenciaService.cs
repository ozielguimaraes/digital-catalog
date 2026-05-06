using MeuCatalogo.Application.DTOs.Requests;
using MeuCatalogo.Application.DTOs.Responses;

namespace MeuCatalogo.Application.Interfaces;

public interface ITransferenciaService
{
    Task<ApiResponse<List<LancamentoResponse>>> CriarEntreContasAsync(TransferenciaRequest request, string userId);
    Task<ApiResponse<List<LancamentoResponse>>> CriarPagamentoFaturaAsync(TransferenciaRequest request, string userId);
    Task<ApiResponse<bool>> ExcluirAsync(Guid lancamentoId, string userId);
}
