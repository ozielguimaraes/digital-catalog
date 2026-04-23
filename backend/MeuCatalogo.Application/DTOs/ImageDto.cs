namespace MeuCatalogo.Application.DTOs;

public class ImageDto
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public ImageLinksDto Images { get; set; } = new();
    public string FileName { get; set; } = string.Empty;
    public long Size { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTime UploadDate { get; set; }
    public bool IsPrincipal { get; set; }
    public int Ordem { get; set; }
}

public class ImageUploadRequest
{
    public Guid ProdutoId { get; set; }
    public bool IsPrincipal { get; set; }
    public int Ordem { get; set; }
}

public class ImageUploadResponse
{
    public string Url { get; set; } = string.Empty;
    public ImageLinksDto Images { get; set; } = new();
    public string FileName { get; set; } = string.Empty;
    public bool IsPrincipal { get; set; }
    public int Ordem { get; set; }
}
