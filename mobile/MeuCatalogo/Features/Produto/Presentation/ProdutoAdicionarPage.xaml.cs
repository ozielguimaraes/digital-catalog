using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace MeuCatalogo.Features.Produto;

public partial class ProdutoAdicionarPage : ContentPage
{
    private readonly ProdutoAdicionarPageViewModel _viewModel;
    private readonly ILogger<ProdutoAdicionarPage> _logger;

    public ProdutoAdicionarPage(ProdutoAdicionarPageViewModel viewModel, ILogger<ProdutoAdicionarPage> logger)
    {
        var sw = Stopwatch.StartNew();
        InitializeComponent();
        _logger = logger;
        _logger.LogInformation("ProdutoAdicionarPage InitializeComponent em {ElapsedMs}ms", sw.ElapsedMilliseconds);
        BindingContext = _viewModel = viewModel;
        _logger.LogInformation("ProdutoAdicionarPage ctor completo em {ElapsedMs}ms", sw.ElapsedMilliseconds);
    }

    protected override void OnAppearing()
    {
        var sw = Stopwatch.StartNew();
        base.OnAppearing();
        _logger.LogInformation("ProdutoAdicionarPage OnAppearing em {ElapsedMs}ms", sw.ElapsedMilliseconds);
        _ = _viewModel.CarregarCategoriasCommand.ExecuteAsync(null);
    }
}
