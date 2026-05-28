namespace MeuCatalogo.Components;

public partial class QtyStepper : ContentView
{
    public QtyStepper()
    {
        InitializeComponent();
        Render();
    }

    public static readonly BindableProperty QuantityProperty = BindableProperty.Create(
        nameof(Quantity), typeof(int), typeof(QtyStepper), 0, BindingMode.TwoWay,
        propertyChanged: OnQuantityChanged);

    public int Quantity
    {
        get => (int)GetValue(QuantityProperty);
        set => SetValue(QuantityProperty, value);
    }

    public static readonly BindableProperty MinimumProperty = BindableProperty.Create(
        nameof(Minimum), typeof(int), typeof(QtyStepper), 0);

    public int Minimum
    {
        get => (int)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public static readonly BindableProperty MaximumProperty = BindableProperty.Create(
        nameof(Maximum), typeof(int), typeof(QtyStepper), 999);

    public int Maximum
    {
        get => (int)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    private static void OnQuantityChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is QtyStepper stepper) stepper.Render();
    }

    private void Render()
    {
        QuantityLabel.Text = Quantity.ToString();
    }

    private void OnDecrementTapped(object? sender, TappedEventArgs e)
    {
        if (Quantity > Minimum) Quantity--;
    }

    private void OnIncrementTapped(object? sender, TappedEventArgs e)
    {
        if (Quantity < Maximum) Quantity++;
    }
}
