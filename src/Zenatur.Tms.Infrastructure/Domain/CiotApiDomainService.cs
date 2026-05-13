using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Zenatur.Tms.Application.Domain;
using Zenatur.Tms.Infrastructure.Ciot;

namespace Zenatur.Tms.Infrastructure.Domain;

/// <summary>
/// IDomainService consumindo CIOT API real (`/api/v1/dominios/*`).
/// Mantém cache IMemoryCache TTL 30min para evitar latência repetida nos Selects.
/// </summary>
internal sealed class CiotApiDomainService : IDomainService
{
    private readonly IMemoryCache                   _cache;
    private readonly CiotApiHttpClient              _http;
    private readonly ILogger<CiotApiDomainService>  _logger;
    private static readonly TimeSpan _ttl = TimeSpan.FromMinutes(30);

    public CiotApiDomainService(IMemoryCache cache, CiotApiHttpClient http, ILogger<CiotApiDomainService> logger)
    {
        _cache  = cache;
        _http   = http;
        _logger = logger;
    }

    public Task<IReadOnlyList<DomainItem>>        GetTiposVeiculoAsync(CancellationToken ct = default)  => FetchAsDomainItemsAsync("CategoriaVeiculo", ct);
    public Task<IReadOnlyList<DomainItem>>        GetTiposCargaAsync(CancellationToken ct = default)    => FetchAsDomainItemsAsync("TipoCargaANTT", ct);
    public Task<IReadOnlyList<DomainItem>>        GetMeiosPagamentoAsync(CancellationToken ct = default) => FetchAsDomainItemsAsync("TipoMeioPagamento", ct);

    public async Task<IReadOnlyList<UnidadeMedidaItem>> GetUnidadesMedidaAsync(string tipoCargaCodigo, CancellationToken ct = default)
    {
        // Não existe domínio "UnidadeMedida" no CIOT hoje — usa-se mapeamento fixo client-side
        // até CIOT API expor (D4 sub-task futura).
        var fallback = TipoCargaFallback.UnidadesPorTipo;
        await Task.CompletedTask;
        return fallback.TryGetValue(tipoCargaCodigo, out var lista)
            ? lista
            : [];
    }

    public Task<IReadOnlyList<CidadeIbgeItem>> GetCidadesAsync(CancellationToken ct = default)
    {
        // CidadesIbge ainda em tabela separada (não está em PamcardDominio). Fallback estático.
        return Task.FromResult<IReadOnlyList<CidadeIbgeItem>>(CidadesFallback.Lista);
    }

    public Task<IReadOnlyList<BancoItem>> GetBancosAsync(CancellationToken ct = default)
    {
        // Bancos ainda em tabela separada. Fallback estático.
        return Task.FromResult<IReadOnlyList<BancoItem>>(BancosFallback.Lista);
    }

    public void InvalidarCache()
    {
        foreach (var k in new[] { "CategoriaVeiculo", "TipoCargaANTT", "TipoMeioPagamento" })
            _cache.Remove($"domain:{k}");
        _logger.LogInformation("Cache de domínios CIOT invalidado.");
    }

    // ── helpers ──

    private async Task<IReadOnlyList<DomainItem>> FetchAsDomainItemsAsync(string nomeDominio, CancellationToken ct)
    {
        var key = $"domain:{nomeDominio}";
        if (_cache.TryGetValue<IReadOnlyList<DomainItem>>(key, out var cached) && cached is not null)
            return cached;

        var result = await _http.GetAsync<IReadOnlyList<DominioApiResponse>>($"/api/v1/dominios/{nomeDominio}", ct);
        if (result.IsFailed)
        {
            _logger.LogWarning("Falha ao carregar domínio {Nome}: {Errors}", nomeDominio, string.Join("; ", result.Errors.Select(e => e.Message)));
            return [];
        }

        var items = result.Value
            .Where(d => d.Ativo)
            .Select(d => new DomainItem(d.Codigo, d.Descricao))
            .ToList();

        _cache.Set(key, (IReadOnlyList<DomainItem>)items, _ttl);
        return items;
    }

    private record DominioApiResponse(
        [property: JsonPropertyName("nomeDominio")]    string  NomeDominio,
        [property: JsonPropertyName("codigo")]         string  Codigo,
        [property: JsonPropertyName("descricao")]      string  Descricao,
        [property: JsonPropertyName("valorAdicional")] string? ValorAdicional,
        [property: JsonPropertyName("ativo")]          bool    Ativo);
}

// ─── Fallbacks até domínios serem expostos pela CIOT API ───

internal static class TipoCargaFallback
{
    // Mapeia código CIOT (TipoCargaANTT) → unidades válidas
    public static readonly IReadOnlyDictionary<string, IReadOnlyList<UnidadeMedidaItem>> UnidadesPorTipo =
        new Dictionary<string, IReadOnlyList<UnidadeMedidaItem>>
        {
            ["1"]  = [new("TON","Tonelada","1"), new("KG","Quilo","1")],
            ["2"]  = [new("LT","Litro","2"),     new("M3","Metro Cúbico","2")],
            ["3"]  = [new("TON","Tonelada","3"), new("KG","Quilo","3")],
            ["4"]  = [new("C20","Container 20'","4"), new("C40","Container 40'","4")],
            ["5"]  = [new("TON","Tonelada","5"), new("VOL","Volumes","5"), new("UN","Unidades","5")],
        };
}

internal static class CidadesFallback
{
    public static readonly IReadOnlyList<CidadeIbgeItem> Lista =
    [
        new("3550308", "São Paulo",      "SP"),
        new("3304557", "Rio de Janeiro", "RJ"),
        new("3106200", "Belo Horizonte", "MG"),
        new("4106902", "Curitiba",       "PR"),
        new("4314902", "Porto Alegre",   "RS"),
        new("4205407", "Florianópolis",  "SC"),
        new("2927408", "Salvador",       "BA"),
        new("5300108", "Brasília",       "DF"),
        new("2304400", "Fortaleza",      "CE"),
        new("1302603", "Manaus",         "AM"),
        new("3205309", "Vitória",        "ES"),
        new("3509502", "Campinas",       "SP"),
        new("3170206", "Uberlândia",     "MG"),
        new("3303302", "Niterói",        "RJ"),
        new("4209102", "Joinville",      "SC"),
        new("5208707", "Goiânia",        "GO"),
    ];
}

internal static class BancosFallback
{
    public static readonly IReadOnlyList<BancoItem> Lista =
    [
        new("001", "Banco do Brasil"),
        new("033", "Santander"),
        new("104", "Caixa Econômica Federal"),
        new("237", "Bradesco"),
        new("341", "Itaú Unibanco"),
        new("260", "Nubank"),
        new("077", "Banco Inter"),
        new("336", "C6 Bank"),
        new("212", "Banco Original"),
        new("422", "Banco Safra"),
    ];
}
