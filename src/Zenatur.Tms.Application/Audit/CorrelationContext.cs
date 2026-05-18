namespace Zenatur.Tms.Application.Audit;

/// <summary>
/// CorrelationId da operação em curso. Backing AsyncLocal: o valor flui pelo
/// mesmo fluxo async da chamada (inclui os DelegatingHandlers do
/// HttpClientFactory, cujo escopo de DI difere do circuito Blazor).
/// Propagado ao CIOT API via header X-Correlation-Id para vincular o XML
/// SOAP gravado lá ao mesmo código mostrado ao usuário.
/// </summary>
public sealed class CorrelationContext
{
    private static readonly AsyncLocal<string?> _current = new();

    public string? Current
    {
        get => _current.Value;
        set => _current.Value = value;
    }
}
