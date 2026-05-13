using Microsoft.Extensions.Options;

namespace Zenatur.Tms.Infrastructure.LegacyBridge;

/// <summary>Injeta X-Api-Key outbound em toda chamada à LegacyBridge.</summary>
public sealed class BridgeApiKeyHandler : DelegatingHandler
{
    private readonly IOptionsMonitor<LegacyBridgeOptions> _options;

    public BridgeApiKeyHandler(IOptionsMonitor<LegacyBridgeOptions> options) => _options = options;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var key = _options.CurrentValue.ApiKey;
        if (!string.IsNullOrWhiteSpace(key) && !request.Headers.Contains("X-Api-Key"))
            request.Headers.Add("X-Api-Key", key);
        return base.SendAsync(request, ct);
    }
}
