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

        string? conn = configuration["BlobStorage:ConnectionString"] 
                       ?? configuration["AZURE_STORAGE_CONNECTION_STRING"]
                       ?? Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");

        if (string.IsNullOrWhiteSpace(conn))
            throw new InvalidOperationException("BlobStorage:ConnectionString or AZURE_STORAGE_CONNECTION_STRING not configured.");

        var service = new BlobServiceClient(conn);
        _container = service.GetBlobContainerClient(options.ContainerName);
        _container.CreateIfNotExists(PublicAccessType.Blob);
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

    public string? GetReadSasUrl(string blobPath, TimeSpan ttl)
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
