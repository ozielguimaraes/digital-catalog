using System.Net;
using System.Net.Http.Headers;
using MeuCatalogo.Features;
using MeuCatalogo.Features.Auth;
using MeuCatalogo.Features.Auth.ApiClients;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeuCatalogo.Infrastructure;

public class AuthenticationHandler(IServiceProvider serviceProvider, ILogger<AuthenticationHandler> logger)
    : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.RequestUri?.AbsolutePath.Contains("/auth/login") == true ||
            request.RequestUri?.AbsolutePath.Contains("/auth/refresh-token") == true ||
            request.RequestUri?.AbsolutePath.Contains("/auth/register") == true)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        var token = Preferences.Get(BaseApiService.TokenKey, string.Empty);
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            logger.LogWarning("Recebido 401 Unauthorized. Tentando refresh token...");

            try
            {
                var authService = serviceProvider.GetRequiredService<IAuthService>();
                var refreshed = await authService.RefreshTokenAsync(cancellationToken);

                if (refreshed)
                {
                    logger.LogInformation("Token atualizado com sucesso. Reenviando requisição original.");
                    
                    var newToken = Preferences.Get(BaseApiService.TokenKey, string.Empty);
                    
                    // Clona a requisição para reenvio
                    var newRequest = await CloneHttpRequestMessageAsync(request);
                    newRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
                    
                    return await base.SendAsync(newRequest, cancellationToken);
                }
                else
                {
                    logger.LogWarning("Refresh token falhou. Redirecionando para login.");
                    await NavigateToLogin();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao tentar refresh token.");
                await NavigateToLogin();
            }
        }

        return response;
    }

    private Task NavigateToLogin()
    {
        Preferences.Remove(BaseApiService.TokenKey);
        Preferences.Remove(BaseApiService.RefreshTokenKey);
        Preferences.Remove(BaseApiService.UserInfoKey);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (Application.Current != null)
            {
                var loginPage = serviceProvider.GetRequiredService<LoginPage>();
                Application.Current.MainPage = loginPage;
            }
        });

        return Task.CompletedTask;
    }

    private static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage req)
    {
        HttpRequestMessage clone = new HttpRequestMessage(req.Method, req.RequestUri);

        if (req.Content != null)
        {
            var ms = new MemoryStream();
            await req.Content.CopyToAsync(ms);
            ms.Position = 0;
            clone.Content = new StreamContent(ms);

            foreach (var h in req.Content.Headers)
                clone.Content.Headers.Add(h.Key, h.Value);
        }

        clone.Version = req.Version;

        foreach (KeyValuePair<string, IEnumerable<string>> header in req.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        foreach (KeyValuePair<string, object?> prop in req.Options)
            clone.Options.TryAdd(prop.Key, prop.Value);

        return clone;
    }
}
