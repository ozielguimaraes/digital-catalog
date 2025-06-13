using Microsoft.Extensions.Logging;

namespace MeuCatalogo.Features.Produto;

public partial class ProdutoListaPageViewModel : BasePageViewModel
{
    private readonly ILogger<ProdutoListaPageViewModel> _logger;

    public ProdutoListaPageViewModel(ILogger<ProdutoListaPageViewModel> logger)
    {
        _logger = logger;
    }
}
