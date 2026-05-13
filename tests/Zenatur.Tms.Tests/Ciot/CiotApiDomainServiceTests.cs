using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Zenatur.Tms.Infrastructure.Ciot;
using Zenatur.Tms.Infrastructure.Domain;

namespace Zenatur.Tms.Tests.Ciot;

/// <summary>
/// Testes do CiotApiDomainService (A11): consumo de /api/v1/dominios/{nome} + cache 30min.
/// </summary>
public class CiotApiDomainServiceTests : IDisposable
{
    private readonly WireMockServer        _server;
    private readonly HttpClient            _httpClient;
    private readonly CiotApiHttpClient     _ciotHttp;
    private readonly IMemoryCache          _cache;

    public CiotApiDomainServiceTests()
    {
        _server = WireMockServer.Start();

        var monitor = new TestMonitor(new CiotApiOptions { BaseUrl = _server.Url!, ApiKey = "k" });
        var handler = new ApiKeyDelegatingHandler(monitor) { InnerHandler = new HttpClientHandler() };
        _httpClient = new HttpClient(handler) { BaseAddress = new Uri(_server.Url!) };
        _ciotHttp   = new CiotApiHttpClient(_httpClient, NullLogger<CiotApiHttpClient>.Instance);
        _cache      = new MemoryCache(new MemoryCacheOptions());
    }

    [Fact]
    public async Task GetTiposCarga_ChamaEndpointCorreto_DeserializaItens()
    {
        _server
            .Given(Request.Create().WithPath("/api/v1/dominios/TipoCargaANTT").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""
                [
                    { "nomeDominio": "TipoCargaANTT", "codigo": "1", "descricao": "Granel solido", "valorAdicional": null, "ativo": true },
                    { "nomeDominio": "TipoCargaANTT", "codigo": "2", "descricao": "Granel liquido", "valorAdicional": null, "ativo": true },
                    { "nomeDominio": "TipoCargaANTT", "codigo": "99", "descricao": "Obsoleto", "valorAdicional": null, "ativo": false }
                ]
                """));

        var svc = new CiotApiDomainService(_cache, _ciotHttp, NullLogger<CiotApiDomainService>.Instance);
        var tipos = await svc.GetTiposCargaAsync();

        Assert.Equal(2, tipos.Count); // descarta inativo
        Assert.Equal("Granel solido",  tipos[0].Descricao);
        Assert.Equal("Granel liquido", tipos[1].Descricao);
    }

    [Fact]
    public async Task GetTiposCarga_SegundaChamada_UsaCacheSemBaterApi()
    {
        _server
            .Given(Request.Create().WithPath("/api/v1/dominios/TipoCargaANTT").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""[ { "nomeDominio": "TipoCargaANTT", "codigo": "1", "descricao": "X", "valorAdicional": null, "ativo": true } ]"""));

        var svc = new CiotApiDomainService(_cache, _ciotHttp, NullLogger<CiotApiDomainService>.Instance);

        await svc.GetTiposCargaAsync();
        await svc.GetTiposCargaAsync();
        await svc.GetTiposCargaAsync();

        // 1 chamada HTTP, demais vieram do cache
        Assert.Single(_server.LogEntries);
    }

    [Fact]
    public async Task InvalidarCache_ForceNovaChamadaHttp()
    {
        _server
            .Given(Request.Create().WithPath("/api/v1/dominios/CategoriaVeiculo").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""[ { "nomeDominio": "CategoriaVeiculo", "codigo": "1", "descricao": "Truck", "valorAdicional": null, "ativo": true } ]"""));

        var svc = new CiotApiDomainService(_cache, _ciotHttp, NullLogger<CiotApiDomainService>.Instance);

        await svc.GetTiposVeiculoAsync();
        svc.InvalidarCache();
        await svc.GetTiposVeiculoAsync();

        Assert.Equal(2, _server.LogEntries.Count());
    }

    [Fact]
    public async Task GetCidades_RetornaListaFallback()
    {
        var svc = new CiotApiDomainService(_cache, _ciotHttp, NullLogger<CiotApiDomainService>.Instance);
        var cidades = await svc.GetCidadesAsync();

        Assert.NotEmpty(cidades);
        Assert.Contains(cidades, c => c.Cidade == "São Paulo" && c.Uf == "SP");
    }

    [Fact]
    public async Task GetBancos_RetornaListaFallback()
    {
        var svc = new CiotApiDomainService(_cache, _ciotHttp, NullLogger<CiotApiDomainService>.Instance);
        var bancos = await svc.GetBancosAsync();

        Assert.NotEmpty(bancos);
        Assert.Contains(bancos, b => b.Codigo == "341"); // Itaú
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        _server.Stop();
        _server.Dispose();
        _cache.Dispose();
    }

    private sealed class TestMonitor : IOptionsMonitor<CiotApiOptions>
    {
        public TestMonitor(CiotApiOptions current) => CurrentValue = current;
        public CiotApiOptions CurrentValue { get; }
        public CiotApiOptions Get(string? name) => CurrentValue;
        public IDisposable? OnChange(Action<CiotApiOptions, string?> listener) => null;
    }
}
