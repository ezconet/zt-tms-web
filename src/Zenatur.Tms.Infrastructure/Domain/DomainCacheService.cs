using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Zenatur.Tms.Application.Domain;

namespace Zenatur.Tms.Infrastructure.Domain;

/// <summary>
/// Cache em memória para domínios estáticos. Eliminação de delay de rede nos Selects.
/// Hoje retorna dados mock; quando houver API real, troca implementações privadas LoadXxxAsync.
/// </summary>
internal sealed class DomainCacheService : IDomainService
{
    private readonly IMemoryCache             _cache;
    private readonly ILogger<DomainCacheService> _logger;
    private static readonly TimeSpan _ttl = TimeSpan.FromMinutes(30);

    private const string K_VEICULO   = "domain:tipos_veiculo";
    private const string K_CARGA     = "domain:tipos_carga";
    private const string K_UM        = "domain:unidades_medida";
    private const string K_CIDADE    = "domain:cidades_ibge";
    private const string K_BANCO     = "domain:bancos";
    private const string K_MP        = "domain:meios_pagamento";

    public DomainCacheService(IMemoryCache cache, ILogger<DomainCacheService> logger)
    {
        _cache  = cache;
        _logger = logger;
    }

    public Task<IReadOnlyList<DomainItem>> GetTiposVeiculoAsync(CancellationToken ct = default) =>
        GetOrLoadAsync(K_VEICULO, LoadTiposVeiculoAsync, ct);

    public Task<IReadOnlyList<DomainItem>> GetTiposCargaAsync(CancellationToken ct = default) =>
        GetOrLoadAsync(K_CARGA, LoadTiposCargaAsync, ct);

    public async Task<IReadOnlyList<UnidadeMedidaItem>> GetUnidadesMedidaAsync(string tipoCargaCodigo, CancellationToken ct = default)
    {
        var todas = await GetOrLoadAsync(K_UM, LoadUnidadesMedidaAsync, ct);
        return todas.Where(u => u.TipoCargaCodigo == tipoCargaCodigo).ToList();
    }

    public Task<IReadOnlyList<CidadeIbgeItem>> GetCidadesAsync(CancellationToken ct = default) =>
        GetOrLoadAsync(K_CIDADE, LoadCidadesAsync, ct);

    public Task<IReadOnlyList<BancoItem>> GetBancosAsync(CancellationToken ct = default) =>
        GetOrLoadAsync(K_BANCO, LoadBancosAsync, ct);

    public Task<IReadOnlyList<DomainItem>> GetMeiosPagamentoAsync(CancellationToken ct = default) =>
        GetOrLoadAsync(K_MP, LoadMeiosPagamentoAsync, ct);

    public void InvalidarCache()
    {
        foreach (var k in new[] { K_VEICULO, K_CARGA, K_UM, K_CIDADE, K_BANCO, K_MP })
            _cache.Remove(k);
        _logger.LogInformation("Cache de domínios invalidado manualmente.");
    }

    private async Task<IReadOnlyList<T>> GetOrLoadAsync<T>(
        string key,
        Func<CancellationToken, Task<IReadOnlyList<T>>> loader,
        CancellationToken ct)
    {
        if (_cache.TryGetValue<IReadOnlyList<T>>(key, out var cached) && cached is not null)
            return cached;

        _logger.LogDebug("Cache miss para {Key} — carregando...", key);
        var data = await loader(ct);
        _cache.Set(key, data, _ttl);
        return data;
    }

    // ───────── Loaders mock (substituir por chamadas reais quando integrar) ─────────

    private static Task<IReadOnlyList<DomainItem>> LoadTiposVeiculoAsync(CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<DomainItem>>(
        [
            new("TRUCK",   "Truck (Toco)"),
            new("CARRETA", "Carreta"),
            new("BITREM",  "Bitrem"),
            new("VAN",     "Van / Utilitário"),
            new("RODOTREM","Rodotrem"),
        ]);

    private static Task<IReadOnlyList<DomainItem>> LoadTiposCargaAsync(CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<DomainItem>>(
        [
            new("GS", "Granel Sólido"),
            new("GL", "Granel Líquido"),
            new("FR", "Frigorificada"),
            new("CG", "Carga Geral"),
            new("CT", "Container"),
        ]);

    private static Task<IReadOnlyList<UnidadeMedidaItem>> LoadUnidadesMedidaAsync(CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<UnidadeMedidaItem>>(
        [
            new("TON", "Tonelada",      "GS"),
            new("KG",  "Quilo",         "GS"),
            new("TON", "Tonelada",      "FR"),
            new("KG",  "Quilo",         "FR"),
            new("LT",  "Litro",         "GL"),
            new("M3",  "Metro Cúbico",  "GL"),
            new("TON", "Tonelada",      "CG"),
            new("VOL", "Volumes",       "CG"),
            new("UN",  "Unidades",      "CG"),
            new("C20", "Container 20'", "CT"),
            new("C40", "Container 40'", "CT"),
        ]);

    private static Task<IReadOnlyList<CidadeIbgeItem>> LoadCidadesAsync(CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<CidadeIbgeItem>>(
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
        ]);

    private static Task<IReadOnlyList<BancoItem>> LoadBancosAsync(CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<BancoItem>>(
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
        ]);

    private static Task<IReadOnlyList<DomainItem>> LoadMeiosPagamentoAsync(CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<DomainItem>>(
        [
            new("CARTAO_PAMCARD", "Cartão Pamcard"),
            new("CONTA_BANCARIA", "Conta Bancária (TED/PIX)"),
            new("BOLETO",         "Boleto Bancário"),
        ]);
}
