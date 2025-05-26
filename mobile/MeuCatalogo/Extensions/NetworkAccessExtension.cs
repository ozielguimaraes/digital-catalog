namespace MeuCatalogo.Extensions;

public static class NetworkAccessExtension
{
    public static bool HasInternetConnection(this NetworkAccess networkAccess)
    {
        return networkAccess is NetworkAccess.Unknown or NetworkAccess.None;
    }
}
