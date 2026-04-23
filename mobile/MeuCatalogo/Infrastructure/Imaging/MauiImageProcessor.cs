using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using MeuCatalogo.Core.Abstractions.Imaging;

namespace MeuCatalogo.Infrastructure.Imaging;

public sealed class MauiImageProcessor : IImageProcessor
{
    public async Task<Stream> CompressAsync(Stream stream, float quality = 0.75f, int maxWidth = 1920, int maxHeight = 1920)
    {
        var image = PlatformImage.FromStream(stream);

        if (image == null) return stream;

        var ratioX = (float)maxWidth / image.Width;
        var ratioY = (float)maxHeight / image.Height;
        var ratio = Math.Min(ratioX, ratioY);

        if (ratio < 1)
        {
            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            image = image.Resize(newWidth, newHeight);
        }

        var memoryStream = new MemoryStream();
        image.Save(memoryStream, ImageFormat.Jpeg, quality);
        memoryStream.Position = 0;
        return memoryStream;
    }
}
