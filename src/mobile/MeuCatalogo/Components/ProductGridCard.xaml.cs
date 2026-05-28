using System.Windows.Input;

namespace MeuCatalogo.Components;

public partial class ProductGridCard : ContentView
{
    public ProductGridCard()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty ThumbnailSourceProperty = BindableProperty.Create(
        nameof(ThumbnailSource), typeof(ImageSource), typeof(ProductGridCard), null);

    public ImageSource? ThumbnailSource
    {
        get => (ImageSource?)GetValue(ThumbnailSourceProperty);
        set => SetValue(ThumbnailSourceProperty, value);
    }

    public static readonly BindableProperty NomeProperty = BindableProperty.Create(
        nameof(Nome), typeof(string), typeof(ProductGridCard), string.Empty);

    public string Nome
    {
        get => (string)GetValue(NomeProperty);
        set => SetValue(NomeProperty, value);
    }

    public static readonly BindableProperty PrecoProperty = BindableProperty.Create(
        nameof(Preco), typeof(decimal), typeof(ProductGridCard), 0m);

    public decimal Preco
    {
        get => (decimal)GetValue(PrecoProperty);
        set => SetValue(PrecoProperty, value);
    }

    public static readonly BindableProperty PrecoComDescontoProperty = BindableProperty.Create(
        nameof(PrecoComDesconto), typeof(decimal?), typeof(ProductGridCard), null);

    public decimal? PrecoComDesconto
    {
        get => (decimal?)GetValue(PrecoComDescontoProperty);
        set => SetValue(PrecoComDescontoProperty, value);
    }

    public static readonly BindableProperty SubtitleTextProperty = BindableProperty.Create(
        nameof(SubtitleText), typeof(string), typeof(ProductGridCard), string.Empty,
        propertyChanged: (b, _, v) =>
            ((ProductGridCard)b).HasSubtitle = !string.IsNullOrWhiteSpace(v as string));

    public string SubtitleText
    {
        get => (string)GetValue(SubtitleTextProperty);
        set => SetValue(SubtitleTextProperty, value);
    }

    public static readonly BindableProperty HasSubtitleProperty = BindableProperty.Create(
        nameof(HasSubtitle), typeof(bool), typeof(ProductGridCard), false);

    public bool HasSubtitle
    {
        get => (bool)GetValue(HasSubtitleProperty);
        private set => SetValue(HasSubtitleProperty, value);
    }

    public static readonly BindableProperty TapCommandProperty = BindableProperty.Create(
        nameof(TapCommand), typeof(ICommand), typeof(ProductGridCard), null);

    public ICommand? TapCommand
    {
        get => (ICommand?)GetValue(TapCommandProperty);
        set => SetValue(TapCommandProperty, value);
    }

    public static readonly BindableProperty TapCommandParameterProperty = BindableProperty.Create(
        nameof(TapCommandParameter), typeof(object), typeof(ProductGridCard), null);

    public object? TapCommandParameter
    {
        get => GetValue(TapCommandParameterProperty);
        set => SetValue(TapCommandParameterProperty, value);
    }
}
