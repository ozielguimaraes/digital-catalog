using MeuCatalogo.Application.Interfaces;

namespace MeuCatalogo.Application.Tests.Helpers;

internal sealed class StubStorageService : IStorageService
{
    public Task<string> UploadAsync(string blobPath, Stream content, string contentType, CancellationToken ct = default)
        => Task.FromResult($"/stub/{blobPath}");

    public Task<bool> DeletePrefixAsync(string prefix, CancellationToken ct = default)
        => Task.FromResult(true);

    public string GetBlobUrl(string blobPath) => $"/stub/{blobPath}";

    public string GetPresignedUrlFromPublicUrl(string publicUrl, TimeSpan expiration) => publicUrl;
}
