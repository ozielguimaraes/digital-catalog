using System.Globalization;

namespace MeuCatalogo.Components;

public partial class PriceTag : ContentView
{
    private static readonly CultureInfo PtBr = new("pt-BR");

    public PriceTag()
    {
        InitializeComponent();
        Render();
    }

    public static readonly BindableProperty PrecoProperty = BindableProperty.Create(
        nameof(Preco), typeof(decimal), typeof(PriceTag), 0m,
        propertyChanged: OnAnyPriceChanged);

    public decimal Preco
    {
        get => (decimal)GetValue(PrecoProperty);
        set => SetValue(PrecoProperty, value);
    }

    public static readonly BindableProperty PrecoComDescontoProperty = BindableProperty.Create(
        nameof(PrecoComDesconto), typeof(decimal?), typeof(PriceTag), null,
        propertyChanged: OnAnyPriceChanged);

    public decimal? PrecoComDesconto
    {
        get => (decimal?)GetValue(PrecoComDescontoProperty);
        set => SetValue(PrecoComDescontoProperty, value);
    }

    public static readonly BindableProperty IsCompactProperty = BindableProperty.Create(
        nameof(IsCompact), typeof(bool), typeof(PriceTag), false,
        propertyChanged: OnAnyPriceChanged);

    public bool IsCompact
    {
        get => (bool)GetValue(IsCompactProperty);
        set => SetValue(IsCompactProperty, value);
    }

    private static void OnAnyPriceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is PriceTag tag) tag.Render();
    }

    private void Render()
    {
        var temDesconto = PrecoComDesconto is decimal d && d > 0m && d < Preco;
        var precoExibido = temDesconto ? PrecoComDesconto!.Value : Preco;

        PrimaryPriceLabel.Text = precoExibido.ToString("C2", PtBr);
        var styleKey = IsCompact ? "PriceMediumPrimary" : "PriceLargePrimary";
        if (Application.Current?.Resources.TryGetValue(styleKey, out var style) == true && style is Style s)
        {
            PrimaryPriceLabel.Style = s;
        }

        OriginalPriceLabel.Text = Preco.ToString("C2", PtBr);
        OriginalPriceLabel.IsVisible = temDesconto;
    }
}
