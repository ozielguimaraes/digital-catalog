using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using MeuCatalogo.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace MeuCatalogo.Application.Infrastructure.Storage;

public class AzureBlobStorageService : IStorageService
{
    private readonly BlobContainerClient _container;
    private readonly string _accountUrl;

    public AzureBlobStorageService(BlobStorageOptions options, IConfiguration configuration)
    {
        _accountUrl = options.AccountUrl.TrimEnd('/');

        string? conn = configuration["BlobStorage:ConnectionString"];

        if (string.IsNullOrWhiteSpace(conn))
            throw new InvalidOperationException("BlobStorage:ConnectionString not configured.");

        var service = new BlobServiceClient(conn);
        _container = service.GetBlobContainerClient(options.ContainerName);
        _container.CreateIfNotExists(PublicAccessType.None);
    }

    public async Task<string> UploadAsync(string blobPath, Stream content, string contentType, CancellationToken ct = default)
    {
        var blob = _container.GetBlobClient(blobPath);
        await blob.UploadAsync(content, new BlobHttpHeaders { ContentType = contentType }, cancellationToken: ct);
        return GetBlobUrl(blobPath);
    }

    public async Task<bool> DeletePrefixAsync(string prefix, CancellationToken ct = default)
    {
        bool any = false;
        await foreach (var item in _container.GetBlobsAsync(prefix: prefix, cancellationToken: ct))
        {
            var blob = _container.GetBlobClient(item.Name);
            await blob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: ct);
            any = true;
        }
        return any;
    }

    public string GetBlobUrl(string blobPath)
        => $"{_accountUrl}/{_container.Name}/{blobPath}";

    public string GetPresignedUrlFromPublicUrl(string publicUrl, TimeSpan expiration)
    {
        // Extract blob path from public URL
        // Format: AccountUrl/ContainerName/BlobPath
        var prefix = $"{_accountUrl}/{_container.Name}/";
        
        if (!publicUrl.StartsWith(prefix))
        {
            // If URL doesn't match our storage account, return as is (maybe external image)
            return publicUrl;
        }

        var blobPath = publicUrl.Substring(prefix.Length);
        return GetReadSasUrl(blobPath, expiration) ?? publicUrl;
    }

    private string? GetReadSasUrl(string blobPath, TimeSpan ttl)
    {
        var blob = _container.GetBlobClient(blobPath);
        if (blob.CanGenerateSasUri)
        {
            var expiresOn = DateTimeOffset.UtcNow.Add(ttl);
            var sasUri = blob.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, expiresOn);
            return sasUri.ToString();
        }
        return null;
    }
}
