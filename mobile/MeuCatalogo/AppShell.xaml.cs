namespace MeuCatalogo;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        InitializeRouting();
    }

    private static void InitializeRouting()
    {
        Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
    }
}
