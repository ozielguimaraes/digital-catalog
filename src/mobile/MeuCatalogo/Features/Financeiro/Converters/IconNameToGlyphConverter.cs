using System.Globalization;
using MeuCatalogo.Features.Financeiro.Icons;

namespace MeuCatalogo.Features.Financeiro.Converters;

public sealed class IconNameToGlyphConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => IconCatalog.GetGlyph(value as string);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
