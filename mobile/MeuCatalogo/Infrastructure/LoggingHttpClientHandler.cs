using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace MeuCatalogo.Infrastructure;

public class LoggingHttpClientHandler(ILogger<LoggingHttpClientHandler> logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        string uuid = Guid.NewGuid().ToString();
        string headers = request.Headers.ToString();

        if (request.Content != null)
        {
            string requestPayload = await request.Content.ReadAsStringAsync(cancellationToken);
            logger.LogInformation(
                "uuid:{uuid} - HTTP {Method} REQUEST: Sending request to {Url} with headers {Headers} and payload {Payload}",
                uuid, request.Method, request.RequestUri, headers, requestPayload);
        }
        else
        {
            logger.LogInformation(
                "uuid:{uuid} - HTTP {Method} REQUEST: Sending request to {Url} with headers {Headers}",
                uuid, request.Method, request.RequestUri, headers);
        }

        try
        {
            var response = await base.SendAsync(request, cancellationToken);
            stopwatch.Stop();

            string responsePayload = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogInformation(
                "uuid:{uuid} - HTTP {Method} RESPONSE ({duration}ms): Received response from {Url} with status code {StatusCode}, headers {Headers}, and payload {Payload}",
                uuid, request.Method, stopwatch.ElapsedMilliseconds, request.RequestUri, response.StatusCode,
                response.Headers.ToString(), responsePayload);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(ex, "uuid:{uuid} - HTTP ERROR ({duration}ms): Exception occurred while sending request to {Url}",
                uuid, stopwatch.ElapsedMilliseconds, request.RequestUri);
            throw;
        }
    }
}
