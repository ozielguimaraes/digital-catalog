using MeuCatalogo.Application.DTOs.Responses;
using MeuCatalogo.Application.Entities;
using MeuCatalogo.Application.Infrastructure.Data;
using MeuCatalogo.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MeuCatalogo.Application.Services;

public sealed class ComprovanteFinanceiroService : IComprovanteFinanceiroService
{
    private static readonly HashSet<string> ContentTypesPermitidos = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp", "image/gif", "image/heic",
        "application/pdf"
    };
    private const long TamanhoMaximoBytes = 10 * 1024 * 1024;

    private readonly ApplicationDbContext _db;
    private readonly IStorageService _storage;

    public ComprovanteFinanceiroService(ApplicationDbContext db, IStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    public async Task<ApiResponse<ComprovanteFinanceiroResponse>> UploadAsync(
        string userId, string fileName, string contentType, long size, Stream content, string? descricao)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return ApiResponse<ComprovanteFinanceiroResponse>.Error("Arquivo é obrigatório");
        if (size <= 0 || size > TamanhoMaximoBytes)
            return ApiResponse<ComprovanteFinanceiroResponse>.Error($"Tamanho deve estar entre 1 byte e {TamanhoMaximoBytes / (1024 * 1024)}MB");
        if (!ContentTypesPermitidos.Contains(contentType))
            return ApiResponse<ComprovanteFinanceiroResponse>.Error("Tipo de arquivo não suportado");

        var ext = Path.GetExtension(fileName);
        if (string.IsNullOrEmpty(ext)) ext = ContentTypeToExtension(contentType);
        var safeName = $"{Guid.NewGuid():N}{ext}";
        var basePath = $"comprovantes-financeiros/{userId}/{safeName}";

        await _storage.UploadAsync(basePath, content, contentType);

        var entidade = new ComprovanteFinanceiro
        {
            UserId = userId,
            Descricao = descricao?.Trim(),
            BasePath = basePath,
            ContentType = contentType,
            Size = size,
            FileName = fileName
        };
        _db.ComprovantesFinanceiros.Add(entidade);
        await _db.SaveChangesAsync();

        return ApiResponse<ComprovanteFinanceiroResponse>.Success(ResponseType.Created, MapToResponse(entidade), "Comprovante enviado");
    }

    public async Task<ApiResponse<ComprovanteFinanceiroResponse>> GetByIdAsync(Guid id, string userId)
    {
        var c = await _db.ComprovantesFinanceiros.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId && x.Ativo);
        if (c == null) return ApiResponse<ComprovanteFinanceiroResponse>.Error(ResponseType.NotFound, "Comprovante não encontrado");
        return ApiResponse<ComprovanteFinanceiroResponse>.Success(MapToResponse(c));
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, string userId)
    {
        var c = await _db.ComprovantesFinanceiros.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        if (c == null) return ApiResponse<bool>.Error(ResponseType.NotFound, "Comprovante não encontrado");

        var emUso = await _db.Lancamentos.AnyAsync(l => l.ComprovanteFinanceiroId == id)
                    || await _db.LancamentosBaixas.AnyAsync(b => b.ComprovanteFinanceiroId == id);
        if (emUso) return ApiResponse<bool>.Error("Comprovante em uso. Desvincule antes de excluir.");

        await _storage.DeletePrefixAsync(c.BasePath);
        _db.ComprovantesFinanceiros.Remove(c);
        await _db.SaveChangesAsync();
        return ApiResponse<bool>.Success(true);
    }

    private ComprovanteFinanceiroResponse MapToResponse(ComprovanteFinanceiro c) => new()
    {
        Id = c.Id,
        Descricao = c.Descricao,
        FileName = c.FileName,
        ContentType = c.ContentType,
        Size = c.Size,
        Url = _storage.GetBlobUrl(c.BasePath),
        CreatedAt = c.DataCriacao
    };

    private static string ContentTypeToExtension(string contentType) => contentType.ToLowerInvariant() switch
    {
        "image/jpeg" => ".jpg",
        "image/png" => ".png",
        "image/webp" => ".webp",
        "image/gif" => ".gif",
        "image/heic" => ".heic",
        "application/pdf" => ".pdf",
        _ => ""
    };
}
