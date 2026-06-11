using System.Net;
using System.Net.Http.Headers;
using MeuCatalogo.Features.Auth;
using MeuCatalogo.Features.Auth.Data;
using MeuCatalogo.Features.Auth.Data.Local;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeuCatalogo.Infrastructure;

public class AuthenticationHandler(IServiceProvider serviceProvider, ILogger<AuthenticationHandler> logger)
    : DelegatingHandler
{
    // Serializa o refresh entre requisições concorrentes. O refresh token do backend é one-time use
    // (revogado a cada renovação), então dois refresh em paralelo fariam o segundo usar um token já
    // revogado e cair no logout. Estático porque o handler é registrado como Transient.
    private static readonly SemaphoreSlim _refreshLock = new(1, 1);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.RequestUri?.AbsolutePath.Contains("/auth/login") == true ||
            request.RequestUri?.AbsolutePath.Contains("/auth/refresh-token") == true ||
            request.RequestUri?.AbsolutePath.Contains("/auth/register") == true)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        var authLocal = serviceProvider.GetService<IAuthLocalDataSource>();
        var tokenUsado = authLocal != null ? await authLocal.GetAccessTokenAsync(cancellationToken) : null;
        if (!string.IsNullOrEmpty(tokenUsado))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenUsado);
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.Unauthorized)
        {
            return response;
        }

        logger.LogWarning("Recebido 401 Unauthorized. Tentando refresh token...");

        try
        {
            var novoToken = await EnsureTokenRefreshedAsync(authLocal, tokenUsado, cancellationToken);

            if (string.IsNullOrEmpty(novoToken))
            {
                logger.LogWarning("Refresh token falhou. Redirecionando para login.");
                await NavigateToLogin();
                return response;
            }

            logger.LogInformation("Token disponível após refresh. Reenviando requisição original.");

            var newRequest = await CloneHttpRequestMessageAsync(request);
            newRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", novoToken);

            response.Dispose();
            return await base.SendAsync(newRequest, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao tentar refresh token.");
            await NavigateToLogin();
            return response;
        }
    }

    // Garante um único refresh por janela de expiração. Se outra requisição concorrente já renovou o
    // token enquanto esperávamos o lock, reaproveita o novo token em vez de chamar refresh de novo.
    private async Task<string?> EnsureTokenRefreshedAsync(
        IAuthLocalDataSource? authLocal,
        string? tokenUsado,
        CancellationToken cancellationToken)
    {
        await _refreshLock.WaitAsync(cancellationToken);
        try
        {
            var tokenAtual = authLocal != null ? await authLocal.GetAccessTokenAsync(cancellationToken) : null;
            if (!string.IsNullOrEmpty(tokenAtual) && tokenAtual != tokenUsado)
            {
                return tokenAtual;
            }

            var authRepository = serviceProvider.GetRequiredService<IAuthRepository>();
            var refreshed = await authRepository.RefreshTokenAsync(cancellationToken);
            if (!refreshed)
            {
                return null;
            }

            return authLocal != null ? await authLocal.GetAccessTokenAsync(cancellationToken) : null;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private Task NavigateToLogin()
    {
        var authRepository = serviceProvider.GetService<IAuthRepository>();
        if (authRepository != null)
        {
            Task.Run(async () => await authRepository.LogoutAsync());
        }

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
