using MeuCatalogo.Features.Auth.ApiClients;
using Microsoft.Extensions.Logging;

namespace MeuCatalogo.Features.Catalogo;

public sealed partial class CatalagoListaPageViewModel : BasePageViewModel
{
    private readonly ILogger<CatalagoListaPageViewModel> _logger;

    public CatalagoListaPageViewModel(ILogger<CatalagoListaPageViewModel> logger, IAuthService authService)
    {
        _logger = logger;
    }
}
