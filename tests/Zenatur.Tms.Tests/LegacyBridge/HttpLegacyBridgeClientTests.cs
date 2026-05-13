using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Zenatur.Tms.Infrastructure.LegacyBridge;

namespace Zenatur.Tms.Tests.LegacyBridge;

public class HttpLegacyBridgeClientTests : IDisposable
{
    private readonly WireMockServer        _server;
    private readonly HttpClient            _http;
    private readonly HttpLegacyBridgeClient _client;

    private const string FAKE_KEY = "test-bridge-key";

    public HttpLegacyBridgeClientTests()
    {
        _server = WireMockServer.Start();

        var monitor = new TestMonitor(new LegacyBridgeOptions { BaseUrl = _server.Url!, ApiKey = FAKE_KEY });
        var handler = new BridgeApiKeyHandler(monitor) { InnerHandler = new HttpClientHandler() };
        _http   = new HttpClient(handler) { BaseAddress = new Uri(_server.Url!), Timeout = TimeSpan.FromSeconds(5) };
        _client = new HttpLegacyBridgeClient(_http, NullLogger<HttpLegacyBridgeClient>.Instance);
    }

    [Fact]
    public async Task GetMotorista_Sucesso_DeserializaResponse()
    {
        _server
            .Given(Request.Create().WithPath("/v1/legacy/motoristas/13294646720").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""
                {
                    "cpf": "13294646720",
                    "nome": "JOAO DA SILVA",
                    "dataNascimento": "1980-03-15",
                    "telefone": "11999998888",
                    "endereco": {
                        "logradouro": "RUA X",
                        "numero": "100",
                        "bairro": "CENTRO",
                        "cidadeIbge": "3550308",
                        "uf": "SP",
                        "cep": "01310100"
                    },
                    "rntrc": { "numero": "12345678", "ativo": true, "validade": "2027-01-31" }
                }
                """));

        var result = await _client.GetMotoristaAsync("132.946.467-20");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("JOAO DA SILVA", result.Value.Nome);
        Assert.Equal("12345678", result.Value.Rntrc?.Numero);
        Assert.True(result.Value.Rntrc?.Ativo);
        Assert.Equal("SP", result.Value.Endereco?.Uf);
    }

    [Fact]
    public async Task GetMotorista_404_RetornaOkComValorNull()
    {
        _server
            .Given(Request.Create().WithPath("/v1/legacy/motoristas/99999999999").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(404));

        var result = await _client.GetMotoristaAsync("99999999999");

        Assert.True(result.IsSuccess); // 404 não é falha — apenas "não encontrado"
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task GetMotorista_SanitizaCpf_RemoveMascara()
    {
        _server
            .Given(Request.Create().WithPath("/v1/legacy/motoristas/13294646720").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("""
                { "cpf": "13294646720", "nome": "X" }
                """));

        await _client.GetMotoristaAsync("132.946.467-20"); // máscara

        // url chamada deve ter sido sem máscara
        Assert.Single(_server.LogEntries);
        Assert.Contains("/v1/legacy/motoristas/13294646720", _server.LogEntries.First().RequestMessage.Url);
    }

    [Fact]
    public async Task GetMotorista_EnviaApiKey()
    {
        _server
            .Given(Request.Create().WithPath("/v1/legacy/motoristas/12345678901").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("""{ "cpf":"12345678901", "nome": "X" }"""));

        await _client.GetMotoristaAsync("12345678901");

        var headers = _server.LogEntries.First().RequestMessage.Headers!;
        Assert.Equal(FAKE_KEY, headers["X-Api-Key"].First());
    }

    [Fact]
    public async Task GetVeiculo_Sucesso_DeserializaResponse()
    {
        _server
            .Given(Request.Create().WithPath("/v1/legacy/veiculos/RBI3C32").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""
                {
                    "placa": "RBI3C32",
                    "tipoVeiculo": 1,
                    "renavam": "00123456789",
                    "marca": "VOLVO",
                    "modelo": "FH 540",
                    "proprietario": { "documento": "12345678000199", "tipoDocumento": "CNPJ", "nome": "TRANSP X" }
                }
                """));

        var result = await _client.GetVeiculoAsync("rbi3c32"); // lowercase

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("RBI3C32", result.Value.Placa);
        Assert.Equal("VOLVO",   result.Value.Marca);
        Assert.Equal("CNPJ",    result.Value.Proprietario?.TipoDocumento);
    }

    public void Dispose()
    {
        _http.Dispose();
        _server.Stop();
        _server.Dispose();
    }

    private sealed class TestMonitor : IOptionsMonitor<LegacyBridgeOptions>
    {
        public TestMonitor(LegacyBridgeOptions current) => CurrentValue = current;
        public LegacyBridgeOptions CurrentValue { get; }
        public LegacyBridgeOptions Get(string? name) => CurrentValue;
        public IDisposable? OnChange(Action<LegacyBridgeOptions, string?> listener) => null;
    }
}
