using Microsoft.Extensions.Logging;
using Zenatur.Tms.Application.Composicoes;
using Zenatur.Tms.Infrastructure.Ciot;

namespace Zenatur.Tms.Infrastructure.Composicoes;

internal sealed class CiotApiComposicaoService : IComposicaoManagementService
{
    private readonly CiotApiHttpClient _http;
    private readonly ILogger<CiotApiComposicaoService> _logger;
    private const string ContratanteCnpj = "53717120000170";

    public CiotApiComposicaoService(CiotApiHttpClient http, ILogger<CiotApiComposicaoService> logger)
    {
        _http   = http;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ComposicaoListItem>> ListarAsync(string? tracionantePlaca = null, CancellationToken ct = default)
    {
        var qs = $"?contratanteCnpj={ContratanteCnpj}&take=200";
        if (!string.IsNullOrWhiteSpace(tracionantePlaca)) qs += $"&tracionante={Uri.EscapeDataString(tracionantePlaca.Trim())}";

        var r = await _http.GetAsync<List<ComposicaoApiResponse>>($"/api/v1/composicoes/local{qs}", ct);
        if (r.IsFailed)
        {
            _logger.LogWarning("Falha listar composicoes: {Err}", string.Join("; ", r.Errors.Select(e => e.Message)));
            return [];
        }
        return r.Value.Select(Map).ToList();
    }

    public async Task<ComposicaoListItem?> ObterAsync(int id, CancellationToken ct = default)
    {
        var r = await _http.GetAsync<ComposicaoApiResponse>($"/api/v1/composicoes/local/{id}?contratanteCnpj={ContratanteCnpj}", ct);
        return r.IsFailed ? null : Map(r.Value);
    }

    public async Task<ComposicaoListItem> SalvarAsync(ComposicaoListItem c, CancellationToken ct = default)
    {
        var body = new
        {
            contratanteCnpj            = ContratanteCnpj,
            id                         = c.Id,
            nome                       = c.Nome,
            tracionantePlaca           = c.TracionantePlaca,
            reboques                   = c.Reboques.Select(r => new { placa = r.Placa, ordem = r.Ordem }).ToArray(),
            favorecidoTitularDocumento = c.FavorecidoTitularDocumento,
        };

        if (c.Id == 0)
        {
            var r = await _http.PostAsync<object, int>("/api/v1/composicoes", body, ct);
            if (r.IsFailed) throw new InvalidOperationException("Falha criar composição: " + string.Join("; ", r.Errors.Select(e => e.Message)));
            return await ObterAsync(r.Value, ct) ?? c;
        }
        else
        {
            var r = await _http.PutAsync<object, object>($"/api/v1/composicoes/{c.Id}", body, ct);
            if (r.IsFailed) throw new InvalidOperationException("Falha atualizar composição: " + string.Join("; ", r.Errors.Select(e => e.Message)));
            return await ObterAsync(c.Id, ct) ?? c;
        }
    }

    public async Task<bool> RemoverAsync(int id, CancellationToken ct = default)
    {
        var r = await _http.DeleteAsync($"/api/v1/composicoes/{id}?contratanteCnpj={ContratanteCnpj}", ct);
        return r.IsSuccess;
    }

    private static ComposicaoListItem Map(ComposicaoApiResponse r) => new()
    {
        Id                         = r.id,
        Nome                       = r.nome ?? "",
        TracionantePlaca           = r.tracionantePlaca ?? "",
        Reboques                   = (r.reboques ?? []).Select(x => new ReboqueItem { Placa = x.placa ?? "", Ordem = x.ordem }).ToList(),
        FavorecidoTitularDocumento = r.favorecidoTitularDocumento,
        Ativo                      = r.ativo,
        AtualizadoEm               = r.atualizadoEm,
    };

    private sealed record ComposicaoApiResponse(
        int      id,
        string?  nome,
        string?  tracionantePlaca,
        List<ReboqueApiResponse>? reboques,
        string?  favorecidoTitularDocumento,
        bool     ativo,
        DateTime atualizadoEm);

    private sealed record ReboqueApiResponse(string? placa, int ordem);
}
