namespace MeuCatalogo.Core.Abstractions.Imaging;

public interface IImageProcessor
{
    Task<Stream> CompressAsync(Stream stream, float quality = 0.75f, int maxWidth = 1920, int maxHeight = 1920);
}
