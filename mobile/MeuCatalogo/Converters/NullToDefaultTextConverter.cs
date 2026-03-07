using System.Globalization;
using MeuCatalogo.Features.Produto.Models;

namespace MeuCatalogo.Converters;

public class NullToDefaultTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        string textoPadrao = parameter as string ?? string.Empty;

        return value switch
        {
            null => textoPadrao,
            CategoriaModel categoria => categoria.Nome,
            _ => value.ToString() ?? textoPadrao
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
