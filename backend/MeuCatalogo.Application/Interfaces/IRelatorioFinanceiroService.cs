using MeuCatalogo.Application.DTOs.Requests;
using MeuCatalogo.Application.DTOs.Responses;

namespace MeuCatalogo.Application.Interfaces;

public interface IRelatorioFinanceiroService
{
    Task<ApiResponse<RelatorioFinanceiroResponse>> LancamentosPorCategoriaAsync(RelatorioFinanceiroRequest request, string userId);
}
