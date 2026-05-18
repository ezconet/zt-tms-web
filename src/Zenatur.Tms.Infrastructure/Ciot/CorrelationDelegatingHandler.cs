using Zenatur.Tms.Application.Audit;

namespace Zenatur.Tms.Infrastructure.Ciot;

/// <summary>
/// Injeta header X-Correlation-Id em toda chamada ao CIOT API quando há um
/// CorrelationId ativo — permite o CIOT vincular o XML SOAP ao mesmo código.
/// </summary>
public sealed class CorrelationDelegatingHandler : DelegatingHandler
{
    private readonly CorrelationContext _ctx;

    public CorrelationDelegatingHandler(CorrelationContext ctx) => _ctx = ctx;

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_ctx.Current) &&
            !request.Headers.Contains("X-Correlation-Id"))
            request.Headers.Add("X-Correlation-Id", _ctx.Current);

        return base.SendAsync(request, cancellationToken);
    }
}
