namespace Zenatur.Tms.Application.Audit;

/// <summary>
/// B02 — Cliente de auditoria. Posta eventos no CIOT API (sink único,
/// Opção A). Best-effort: nunca lança exceção que quebre a operação.
/// </summary>
public interface IAuditClient
{
    Task LogAsync(AuditEvent evento, CancellationToken ct = default);
}

public sealed record AuditEvent(
    string  CorrelationId,
    string  Operation,
    bool    Sucesso,
    string? ContratanteCnpj = null,
    string? ReferenciaChave = null,
    string? UsuarioEmail    = null,
    string? RequestJson     = null,
    string? ResponseJson    = null,
    int?    PamcardCodigo   = null,
    string? ErroMensagem    = null,
    int?    DuracaoMs       = null);

/// <summary>
/// Código de rastreio curto e legível: ZT-XXXX-XXXX (base32 Crockford
/// sem 0/O/1/I — ditável por telefone). Gerado por operação.
/// </summary>
public static class CorrelationId
{
    private const string Alfabeto = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ";

    public static string Novo()
    {
        Span<char> buf = stackalloc char[8];
        var rnd = Random.Shared;
        for (var i = 0; i < 8; i++)
            buf[i] = Alfabeto[rnd.Next(Alfabeto.Length)];
        return $"ZT-{new string(buf[..4])}-{new string(buf[4..])}";
    }
}
