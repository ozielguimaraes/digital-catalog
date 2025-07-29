using System.Globalization;
using System.Numerics;

namespace MeuCatalogo.Behaviors;

public class CurrencyBehavior : Behavior<Entry>, IDisposable
{
    private Entry? _entry;

    public static readonly BindableProperty UseCurrencySymbolProperty =
        BindableProperty.Create(nameof(UseCurrencySymbol), typeof(bool), typeof(CurrencyBehavior), false);

    public static readonly BindableProperty UseThousandSeparatorProperty =
        BindableProperty.Create(nameof(UseThousandSeparator), typeof(bool), typeof(CurrencyBehavior), true);

    public static readonly BindableProperty UseDecimalSeparatorProperty =
        BindableProperty.Create(nameof(UseDecimalSeparator), typeof(bool), typeof(CurrencyBehavior), true);

    public static readonly BindableProperty MaximumValueProperty =
        BindableProperty.Create(nameof(MaximumValue), typeof(decimal), typeof(CurrencyBehavior), decimal.MaxValue);

    private bool _isUpdating = false;

    public bool UseCurrencySymbol
    {
        get => (bool)GetValue(UseCurrencySymbolProperty);
        set => SetValue(UseCurrencySymbolProperty, value);
    }

    public bool UseThousandSeparator
    {
        get => (bool)GetValue(UseThousandSeparatorProperty);
        set => SetValue(UseThousandSeparatorProperty, value);
    }

    public bool UseDecimalSeparator
    {
        get => (bool)GetValue(UseDecimalSeparatorProperty);
        set => SetValue(UseDecimalSeparatorProperty, value);
    }

    public decimal MaximumValue
    {
        get => (decimal)GetValue(MaximumValueProperty);
        set => SetValue(MaximumValueProperty, value);
    }

    protected override void OnAttachedTo(Entry bindable)
    {
        base.OnAttachedTo(bindable);
        _entry = bindable;
        _entry.Keyboard = Keyboard.Numeric;
        _entry.TextChanged += OnEntryTextChanged;

        if (string.IsNullOrEmpty(_entry.Text))
            _entry.Text = FormatCurrency(0m);
    }

    protected override void OnDetachingFrom(Entry bindable)
    {
        base.OnDetachingFrom(bindable);
        if (_entry != null)
            _entry.TextChanged -= OnEntryTextChanged;
        _entry = null;
    }

    private void OnEntryTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_isUpdating || _entry == null)
            return;

        _isUpdating = true;

        try
        {
            string digitsOnly = new string(e.NewTextValue?.Where(char.IsDigit).ToArray() ?? Array.Empty<char>());

            if (string.IsNullOrEmpty(digitsOnly))
            {
                UpdateTextAndCursor(FormatCurrency(0m));
                return;
            }

            ParseNumber:
            if (!decimal.TryParse(digitsOnly, out decimal number))
            {
                digitsOnly = digitsOnly[..^1];
                goto ParseNumber;
            }

            decimal value = number / 100m;

            string formatted = FormatCurrency(value);

            if (_entry.Text != formatted)
            {
                UpdateTextAndCursor(formatted);
            }
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private void UpdateTextAndCursor(string text)
    {
        _entry?.Dispatcher.Dispatch(() =>
        {
            _entry.Text = text;

            _entry.CursorPosition = text.Length;
        });
    }

    private string FormatCurrency(decimal value)
    {
        var culture = CultureInfo.CurrentCulture;
        var numberFormat = (NumberFormatInfo)culture.NumberFormat.Clone();

        if (!UseCurrencySymbol)
            numberFormat = RemoveCurrencySymbol(numberFormat);

        if (!UseThousandSeparator)
            numberFormat.NumberGroupSeparator = string.Empty;

        if (!UseDecimalSeparator)
            numberFormat.NumberDecimalSeparator = string.Empty;

        if (!UseDecimalSeparator)
        {
            return value == 0 ? "0" : ((int)(value * 100)).ToString("N0", numberFormat);
        }

        return value.ToString(UseCurrencySymbol ? "C2" : "N2", numberFormat);
    }

    private static NumberFormatInfo RemoveCurrencySymbol(NumberFormatInfo nfi)
    {
        nfi.CurrencySymbol = "";
        nfi.CurrencyPositivePattern = 0;
        nfi.CurrencyNegativePattern = 0;
        return nfi;
    }

    public void Dispose()
    {
        if (_entry == null)
        {
            return;
        }

        _entry.TextChanged -= OnEntryTextChanged;
        _entry = null;
    }
}
