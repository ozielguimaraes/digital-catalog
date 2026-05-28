using System.Windows.Input;

namespace MeuCatalogo.Components;

public partial class SectionHeader : ContentView
{
    public SectionHeader()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(SectionHeader), string.Empty);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly BindableProperty ActionTextProperty = BindableProperty.Create(
        nameof(ActionText), typeof(string), typeof(SectionHeader), string.Empty,
        propertyChanged: OnActionTextChanged);

    public string ActionText
    {
        get => (string)GetValue(ActionTextProperty);
        set => SetValue(ActionTextProperty, value);
    }

    public static readonly BindableProperty ActionCommandProperty = BindableProperty.Create(
        nameof(ActionCommand), typeof(ICommand), typeof(SectionHeader), null);

    public ICommand? ActionCommand
    {
        get => (ICommand?)GetValue(ActionCommandProperty);
        set => SetValue(ActionCommandProperty, value);
    }

    public static readonly BindableProperty ActionCommandParameterProperty = BindableProperty.Create(
        nameof(ActionCommandParameter), typeof(object), typeof(SectionHeader), null);

    public object? ActionCommandParameter
    {
        get => GetValue(ActionCommandParameterProperty);
        set => SetValue(ActionCommandParameterProperty, value);
    }

    public static readonly BindableProperty HasActionProperty = BindableProperty.Create(
        nameof(HasAction), typeof(bool), typeof(SectionHeader), false);

    public bool HasAction
    {
        get => (bool)GetValue(HasActionProperty);
        private set => SetValue(HasActionProperty, value);
    }

    private static void OnActionTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SectionHeader header)
        {
            header.HasAction = !string.IsNullOrWhiteSpace(newValue as string);
        }
    }
}
