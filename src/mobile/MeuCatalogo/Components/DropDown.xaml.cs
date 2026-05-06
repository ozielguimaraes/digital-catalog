using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MeuCatalogo.Components;

public partial class DropDown : ContentView
{
    public DropDown()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(DropDown), null);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly BindableProperty SelectedItemProperty =
        BindableProperty.Create(nameof(SelectedItem), typeof(DropDownItem), typeof(DropDown), null);

    public DropDownItem SelectedItem
    {
        get => (DropDownItem)GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }
}

public partial class DropDownItem : ObservableObject
{
    [ObservableProperty] private string _displayText;
    [ObservableProperty] private object? _value;
}
