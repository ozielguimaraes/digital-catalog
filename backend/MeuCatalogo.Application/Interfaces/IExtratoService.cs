using MeuCatalogo.Application.DTOs.Responses;

namespace MeuCatalogo.Application.Interfaces;

public interface IExtratoService
{
    Task<ApiResponse<ExtratoResponse>> ObterPorContaAsync(Guid contaId, DateTime dataInicio, DateTime dataFim, string userId);
    Task<ApiResponse<ExtratoResponse>> ObterConsolidadoAsync(DateTime dataInicio, DateTime dataFim, IEnumerable<Guid>? contaIds, string userId);
}
