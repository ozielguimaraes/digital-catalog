using MeuCatalogo.Application.Interfaces;

namespace MeuCatalogo.Application.Infrastructure.Storage;

public class LocalFileStorageService : IStorageService
{
    private const string UploadRootFolder = "Uploads";

    public async Task<string> UploadAsync(string blobPath, Stream content, string contentType, CancellationToken ct = default)
    {
        var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), UploadRootFolder, NormalizePath(blobPath));
        var directory = Path.GetDirectoryName(physicalPath)!;
        Directory.CreateDirectory(directory);

        using var fileStream = new FileStream(physicalPath, FileMode.Create, FileAccess.Write);
        await content.CopyToAsync(fileStream, ct);
        return GetBlobUrl(blobPath);
    }

    public async Task<bool> DeletePrefixAsync(string prefix, CancellationToken ct = default)
    {
        var dirPath = Path.Combine(Directory.GetCurrentDirectory(), UploadRootFolder, NormalizePath(prefix));
        if (Directory.Exists(dirPath))
        {
            Directory.Delete(dirPath, true);
            await Task.CompletedTask;
            return true;
        }
        return false;
    }

    public string GetBlobUrl(string blobPath)
        => $"/uploads/{NormalizePath(blobPath)}".Replace("\\", "/");

    private static string NormalizePath(string p)
        => p.TrimStart('/', '\\');
}

