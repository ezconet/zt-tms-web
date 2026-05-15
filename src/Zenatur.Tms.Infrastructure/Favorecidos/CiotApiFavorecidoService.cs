using Microsoft.Extensions.Logging;
using Zenatur.Tms.Application.Favorecidos;
using Zenatur.Tms.Infrastructure.Ciot;

namespace Zenatur.Tms.Infrastructure.Favorecidos;

/// <summary>
/// Implementação real: consulta/persiste favorecidos via CIOT API
/// (endpoints /api/v1/favorecidos/local* + POST/PUT/DELETE /api/v1/favorecidos).
/// Cada operação dispara fluxo CIOT API → Pamcard → outbox → Bridge → legado.
/// </summary>
internal sealed class CiotApiFavorecidoService : IFavorecidoManagementService
{
    private readonly CiotApiHttpClient _http;
    private readonly ILogger<CiotApiFavorecidoService> _logger;

    // Multi-tenant — TMS opera com um único contratante por enquanto.
    // Vir do config quando suportarmos multi-tenant real.
    private const string ContratanteCnpj = "53717120000170";

    public CiotApiFavorecidoService(CiotApiHttpClient http, ILogger<CiotApiFavorecidoService> logger)
    {
        _http   = http;
        _logger = logger;
    }

    public async Task<IReadOnlyList<FavorecidoListItem>> ListarAsync(string? termo = null, CancellationToken ct = default)
    {
        var qs = $"?contratanteCnpj={ContratanteCnpj}&take=200";
        if (!string.IsNullOrWhiteSpace(termo))
            qs += $"&q={Uri.EscapeDataString(termo.Trim())}";

        var r = await _http.GetAsync<List<FavorecidoListLocalApiResponse>>($"/api/v1/favorecidos/local{qs}", ct);
        if (r.IsFailed)
        {
            _logger.LogWarning("Falha ao listar favorecidos: {Errors}", string.Join("; ", r.Errors.Select(e => e.Message)));
            return [];
        }

        return r.Value.Select(MapList).ToList();
    }

    public async Task<FavorecidoListItem?> ObterAsync(string documento, CancellationToken ct = default)
    {
        var r = await _http.GetAsync<FavorecidoFullApiResponse>(
            $"/api/v1/favorecidos/local/{documento}?contratanteCnpj={ContratanteCnpj}", ct);
        if (r.IsFailed) return null;
        return MapFull(r.Value);
    }

    public async Task<FavorecidoListItem> SalvarAsync(FavorecidoListItem favorecido, CancellationToken ct = default)
    {
        // Tenta obter pra decidir POST vs PUT
        var existente = await ObterAsync(favorecido.Documento, ct);

        if (existente is null)
        {
            // POST — cria via Pamcard InsertFavored + outbox Favorecido.Criado
            var insertBody = new
            {
                contratanteCnpj  = ContratanteCnpj,
                documentos       = new[] { new { tipo = favorecido.DocumentoTipo == 1 ? 2 : 1, numero = favorecido.Documento } }, // TMS 1=CPF, Pamcard 2=CPF
                nome             = favorecido.Nome,
                dataNascimento   = favorecido.DataNascimento,
                logradouro       = favorecido.EnderecoLogradouro,
                enderecoNumero   = int.TryParse(favorecido.EnderecoNumero, out var n) ? n : (int?)null,
                bairro           = favorecido.EnderecoBairro,
                cidade           = (string?)null,
                enderecoUf       = favorecido.EnderecoUf,
                cep              = favorecido.EnderecoCep,
                cidadeIbge       = int.TryParse(favorecido.EnderecoCidadeIbge, out var i) ? i : (int?)null,
                telefoneDdd      = favorecido.TelefoneDdd ?? "",
                telefoneNumero   = favorecido.TelefoneNumero ?? "",
            };
            var r = await _http.PostAsync<object, string>("/api/v1/favorecidos", insertBody, ct);
            if (r.IsFailed) throw new InvalidOperationException("Falha ao criar favorecido: " + string.Join("; ", r.Errors.Select(e => e.Message)));
        }
        else
        {
            // PUT — atualiza local + outbox Favorecido.Atualizado
            var updateBody = new
            {
                contratanteCnpj = ContratanteCnpj,
                documento       = favorecido.Documento,
                nome            = favorecido.Nome,
                dataNascimento  = favorecido.DataNascimento,
                email           = favorecido.Email,
                logradouro      = favorecido.EnderecoLogradouro,
                enderecoNumero  = int.TryParse(favorecido.EnderecoNumero, out var n) ? n : (int?)null,
                bairro          = favorecido.EnderecoBairro,
                cidade          = (string?)null,
                enderecoUf      = favorecido.EnderecoUf,
                cep             = favorecido.EnderecoCep,
                cidadeIbge      = int.TryParse(favorecido.EnderecoCidadeIbge, out var i) ? i : (int?)null,
                telefoneDdd     = favorecido.TelefoneDdd,
                telefoneNumero  = favorecido.TelefoneNumero,
            };
            var r = await _http.PutAsync<object, object>($"/api/v1/favorecidos/{favorecido.Documento}", updateBody, ct);
            if (r.IsFailed) throw new InvalidOperationException("Falha ao atualizar favorecido: " + string.Join("; ", r.Errors.Select(e => e.Message)));
        }

        // Re-fetch pra refletir id + outros campos preenchidos pelo backend
        var atualizado = await ObterAsync(favorecido.Documento, ct);
        return atualizado ?? favorecido;
    }

    public async Task<bool> RemoverAsync(string documento, CancellationToken ct = default)
    {
        var r = await _http.DeleteAsync($"/api/v1/favorecidos/{documento}?contratanteCnpj={ContratanteCnpj}", ct);
        return r.IsSuccess;
    }

    // ── Mapping helpers ──

    private static FavorecidoListItem MapList(FavorecidoListLocalApiResponse r) => new()
    {
        Documento      = r.documento ?? "",
        DocumentoTipo  = r.documentoTipo == 2 ? 1 : 2,  // Pamcard 2=CPF -> TMS 1=CPF
        Nome           = r.nome ?? "",
        Email          = r.email,
        Rntrc          = r.rntrc ?? "",
        RntrcSituacao  = r.rntrcSituacao ?? "",
        TelefoneDdd    = r.telefoneDdd,
        TelefoneNumero = r.telefoneNumero,
        CriadoEm       = r.atualizadoEm,
    };

    private static FavorecidoListItem MapFull(FavorecidoFullApiResponse r) => new()
    {
        Documento          = r.documento ?? "",
        DocumentoTipo      = r.documentoTipo == 2 ? 1 : 2,
        Nome               = r.nome ?? "",
        DataNascimento     = r.dataNascimento,
        Email              = r.email,
        Rntrc              = r.rntrc ?? "",
        RntrcSituacao      = r.rntrcSituacao ?? "",
        TelefoneDdd        = r.telefoneDdd,
        TelefoneNumero     = r.telefoneNumero,
        EnderecoLogradouro = r.logradouro,
        EnderecoNumero     = r.enderecoNumero?.ToString(),
        EnderecoBairro     = r.bairro,
        EnderecoCidadeIbge = r.cidadeIbge?.ToString(),
        EnderecoUf         = r.enderecoUf,
        EnderecoCep        = r.cep,
        CriadoEm           = r.criadoEm,
        Contas             = (r.contas ?? []).Select(c => new ContaBancariaItem
        {
            Banco    = c.banco.ToString(),
            Agencia  = c.agencia ?? "",
            Conta    = c.numero ?? "",
            Tipo     = c.tipo == 1 ? "CC" : "CP",
            PixChave = c.chavePix,
        }).ToList(),
    };

    private sealed record FavorecidoListLocalApiResponse(
        int       id,
        string?   documento,
        int       documentoTipo,
        string?   nome,
        string?   email,
        string?   rntrc,
        string?   rntrcSituacao,
        string?   telefoneDdd,
        string?   telefoneNumero,
        bool      ativo,
        DateTime  atualizadoEm);

    private sealed record FavorecidoFullApiResponse(
        int        id,
        string?    documento,
        int        documentoTipo,
        string?    nome,
        DateOnly?  dataNascimento,
        string?    email,
        string?    rntrc,
        string?    rntrcSituacao,
        string?    telefoneDdd,
        string?    telefoneNumero,
        string?    logradouro,
        int?       enderecoNumero,
        string?    bairro,
        string?    cidade,
        string?    enderecoUf,
        string?    cep,
        int?       cidadeIbge,
        bool       ativo,
        DateTime   criadoEm,
        DateTime   atualizadoEm,
        List<ContaApiResponse>? contas,
        List<object>?           cartoes,
        List<object>?           motoristasVinculados);

    private sealed record ContaApiResponse(
        int      id,
        int      banco,
        string?  agencia,
        string?  agenciaDigito,
        string?  numero,
        int      tipo,
        string?  status,
        int?     chavePixTipo,
        string?  chavePix);
}
