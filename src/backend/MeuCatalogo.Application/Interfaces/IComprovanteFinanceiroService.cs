using MeuCatalogo.Application.DTOs.Responses;

namespace MeuCatalogo.Application.Interfaces;

public interface IComprovanteFinanceiroService
{
    Task<ApiResponse<ComprovanteFinanceiroResponse>> UploadAsync(string userId, string fileName, string contentType, long size, Stream content, string? descricao);
    Task<ApiResponse<ComprovanteFinanceiroResponse>> GetByIdAsync(Guid id, string userId);
    Task<ApiResponse<bool>> DeleteAsync(Guid id, string userId);
}
