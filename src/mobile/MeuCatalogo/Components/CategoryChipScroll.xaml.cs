using System.Collections;
using System.Collections.Specialized;
using System.Reflection;

namespace MeuCatalogo.Components;

public partial class CategoryChipScroll : ContentView
{
    private readonly Dictionary<object, Border> _itemToChip = new();

    public CategoryChipScroll()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
        nameof(ItemsSource), typeof(IEnumerable), typeof(CategoryChipScroll), null,
        propertyChanged: OnItemsSourceChanged);

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(
        nameof(SelectedItem), typeof(object), typeof(CategoryChipScroll), null,
        BindingMode.TwoWay,
        propertyChanged: OnSelectedItemChanged);

    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public static readonly BindableProperty DisplayMemberPathProperty = BindableProperty.Create(
        nameof(DisplayMemberPath), typeof(string), typeof(CategoryChipScroll), string.Empty,
        propertyChanged: (b, _, _) => ((CategoryChipScroll)b).Rebuild());

    public string DisplayMemberPath
    {
        get => (string)GetValue(DisplayMemberPathProperty);
        set => SetValue(DisplayMemberPathProperty, value);
    }

    private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (CategoryChipScroll)bindable;
        if (oldValue is INotifyCollectionChanged oldNcc) oldNcc.CollectionChanged -= view.OnCollectionChanged;
        if (newValue is INotifyCollectionChanged newNcc) newNcc.CollectionChanged += view.OnCollectionChanged;
        view.Rebuild();
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => Rebuild();

    private static void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((CategoryChipScroll)bindable).UpdateSelectionVisuals();
    }

    private void Rebuild()
    {
        ChipsHost.Children.Clear();
        _itemToChip.Clear();
        if (ItemsSource == null) return;

        var defaultStyle = ResolveStyle("CategoryChipDefault");
        var selectedStyle = ResolveStyle("CategoryChipSelected");
        var defaultTextStyle = ResolveStyle("CategoryChipTextDefault");
        var selectedTextStyle = ResolveStyle("CategoryChipTextSelected");

        foreach (var item in ItemsSource)
        {
            if (item == null) continue;

            var isSelected = Equals(item, SelectedItem);
            var label = new Label
            {
                Text = ResolveDisplayText(item),
                Style = isSelected ? selectedTextStyle : defaultTextStyle,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
            };

            var border = new Border
            {
                Style = isSelected ? selectedStyle : defaultStyle,
                Content = label,
            };

            var capturedItem = item;
            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) => SelectedItem = capturedItem;
            border.GestureRecognizers.Add(tap);

            _itemToChip[item] = border;
            ChipsHost.Children.Add(border);
        }
    }

    private void UpdateSelectionVisuals()
    {
        var defaultStyle = ResolveStyle("CategoryChipDefault");
        var selectedStyle = ResolveStyle("CategoryChipSelected");
        var defaultTextStyle = ResolveStyle("CategoryChipTextDefault");
        var selectedTextStyle = ResolveStyle("CategoryChipTextSelected");

        foreach (var (item, border) in _itemToChip)
        {
            var isSelected = Equals(item, SelectedItem);
            border.Style = isSelected ? selectedStyle : defaultStyle;
            if (border.Content is Label label)
            {
                label.Style = isSelected ? selectedTextStyle : defaultTextStyle;
            }
        }
    }

    private string ResolveDisplayText(object item)
    {
        if (string.IsNullOrWhiteSpace(DisplayMemberPath)) return item.ToString() ?? string.Empty;
        var prop = item.GetType().GetProperty(DisplayMemberPath, BindingFlags.Public | BindingFlags.Instance);
        return prop?.GetValue(item)?.ToString() ?? string.Empty;
    }

    private static Style? ResolveStyle(string key)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Style s)
        {
            return s;
        }
        return null;
    }
}
