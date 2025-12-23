namespace MeuCatalogo.Application.Infrastructure.Storage;

public class BlobStorageOptions
{
    public string AccountUrl { get; set; } = string.Empty;
    public string ContainerName { get; set; } = string.Empty;
}

