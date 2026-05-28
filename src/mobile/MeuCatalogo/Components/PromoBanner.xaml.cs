using System.Windows.Input;

namespace MeuCatalogo.Components;

public partial class PromoBanner : ContentView
{
    public PromoBanner()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty BadgeTextProperty = BindableProperty.Create(
        nameof(BadgeText), typeof(string), typeof(PromoBanner), string.Empty,
        propertyChanged: (b, _, v) =>
            ((PromoBanner)b).HasBadge = !string.IsNullOrWhiteSpace(v as string));

    public string BadgeText
    {
        get => (string)GetValue(BadgeTextProperty);
        set => SetValue(BadgeTextProperty, value);
    }

    public static readonly BindableProperty HasBadgeProperty = BindableProperty.Create(
        nameof(HasBadge), typeof(bool), typeof(PromoBanner), false);

    public bool HasBadge
    {
        get => (bool)GetValue(HasBadgeProperty);
        private set => SetValue(HasBadgeProperty, value);
    }

    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(PromoBanner), string.Empty);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly BindableProperty BodyProperty = BindableProperty.Create(
        nameof(Body), typeof(string), typeof(PromoBanner), string.Empty,
        propertyChanged: (b, _, v) =>
            ((PromoBanner)b).HasBody = !string.IsNullOrWhiteSpace(v as string));

    public string Body
    {
        get => (string)GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }

    public static readonly BindableProperty HasBodyProperty = BindableProperty.Create(
        nameof(HasBody), typeof(bool), typeof(PromoBanner), false);

    public bool HasBody
    {
        get => (bool)GetValue(HasBodyProperty);
        private set => SetValue(HasBodyProperty, value);
    }

    public static readonly BindableProperty TapCommandProperty = BindableProperty.Create(
        nameof(TapCommand), typeof(ICommand), typeof(PromoBanner), null);

    public ICommand? TapCommand
    {
        get => (ICommand?)GetValue(TapCommandProperty);
        set => SetValue(TapCommandProperty, value);
    }

    public static readonly BindableProperty TapCommandParameterProperty = BindableProperty.Create(
        nameof(TapCommandParameter), typeof(object), typeof(PromoBanner), null);

    public object? TapCommandParameter
    {
        get => GetValue(TapCommandParameterProperty);
        set => SetValue(TapCommandParameterProperty, value);
    }
}
