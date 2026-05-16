using Microsoft.Extensions.Logging;
using Zenatur.Tms.Application.Audit;
using Zenatur.Tms.Infrastructure.Ciot;

namespace Zenatur.Tms.Infrastructure.Audit;

/// <summary>
/// Posta eventos de auditoria no CIOT API (POST /api/v1/audit).
/// Best-effort: qualquer falha é engolida (auditoria não pode quebrar
/// a emissão / busca de minuta).
/// </summary>
internal sealed class HttpAuditClient : IAuditClient
{
    private readonly CiotApiHttpClient _http;
    private readonly ILogger<HttpAuditClient> _logger;

    public HttpAuditClient(CiotApiHttpClient http, ILogger<HttpAuditClient> logger)
    {
        _http   = http;
        _logger = logger;
    }

    public async Task LogAsync(AuditEvent e, CancellationToken ct = default)
    {
        try
        {
            var body = new
            {
                correlationId   = e.CorrelationId,
                operation       = e.Operation,
                sucesso         = e.Sucesso,
                origem          = "TMS",
                contratanteCnpj = e.ContratanteCnpj,
                referenciaChave = e.ReferenciaChave,
                usuarioEmail    = e.UsuarioEmail,
                requestJson     = e.RequestJson,
                responseJson    = e.ResponseJson,
                pamcardCodigo   = e.PamcardCodigo,
                erroMensagem    = e.ErroMensagem,
                duracaoMs       = e.DuracaoMs,
            };

            var r = await _http.PostAsync<object, object>("/api/v1/audit", body, ct);
            if (r.IsFailed)
                _logger.LogWarning("Auditoria {Corr} não registrada: {Err}",
                    e.CorrelationId, string.Join("; ", r.Errors.Select(x => x.Message)));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Auditoria {Corr} falhou (ignorado)", e.CorrelationId);
        }
    }
}
