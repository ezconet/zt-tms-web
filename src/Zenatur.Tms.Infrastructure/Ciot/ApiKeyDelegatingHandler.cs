using Microsoft.Extensions.Options;

namespace Zenatur.Tms.Infrastructure.Ciot;

/// <summary>
/// Injeta header X-Api-Key em toda chamada outbound para CIOT API.
/// </summary>
public sealed class ApiKeyDelegatingHandler : DelegatingHandler
{
    private readonly IOptionsMonitor<CiotApiOptions> _options;

    public ApiKeyDelegatingHandler(IOptionsMonitor<CiotApiOptions> options) => _options = options;

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var key = _options.CurrentValue.ApiKey;
        if (!string.IsNullOrWhiteSpace(key) && !request.Headers.Contains("X-Api-Key"))
            request.Headers.Add("X-Api-Key", key);

        return base.SendAsync(request, cancellationToken);
    }
}
