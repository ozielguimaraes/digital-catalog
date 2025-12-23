namespace MeuCatalogo.Application.Interfaces;

public interface IStorageService
{
    Task<string> UploadAsync(string blobPath, Stream content, string contentType, CancellationToken ct = default);
    Task<bool> DeletePrefixAsync(string prefix, CancellationToken ct = default);
    string GetBlobUrl(string blobPath);
}

