using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Zenatur.Ciot.Client.Favorecidos;
using Zenatur.Tms.Infrastructure.Ciot;

namespace Zenatur.Tms.Tests.Ciot;

/// <summary>
/// Testes de integração TMS ↔ CIOT API (A12) usando WireMock para simular a API.
/// Valida adapter HTTP: serialização, deserialização, parsing de erros, X-Api-Key header.
/// </summary>
public class HttpCiotClientIntegrationTests : IDisposable
{
    private readonly WireMockServer       _server;
    private readonly HttpClient           _httpClient;
    private readonly CiotApiHttpClient    _ciotHttp;
    private readonly HttpCiotClient       _client;

    private const string FAKE_API_KEY = "test-key-abc123";

    public HttpCiotClientIntegrationTests()
    {
        _server = WireMockServer.Start();

        var options = Options.Create(new CiotApiOptions
        {
            BaseUrl = _server.Url!,
            ApiKey  = FAKE_API_KEY,
        });

        // Composição manual: HttpClient + ApiKeyHandler → CiotApiHttpClient → HttpCiotClient
        var monitor = new TestOptionsMonitor<CiotApiOptions>(options.Value);
        var apiKeyHandler = new ApiKeyDelegatingHandler(monitor)
        {
            InnerHandler = new HttpClientHandler(),
        };
        _httpClient = new HttpClient(apiKeyHandler) { BaseAddress = new Uri(_server.Url!) };
        _ciotHttp   = new CiotApiHttpClient(_httpClient, NullLogger<CiotApiHttpClient>.Instance);
        _client     = new HttpCiotClient(_ciotHttp);
    }

    [Fact]
    public async Task FindFavored_Sucesso_DeserializaResponse()
    {
        _server
            .Given(Request.Create()
                .WithPath("/api/v1/favorecidos")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""
                {
                    "nome": "CARLOS MENDES",
                    "statusRntrc": "A",
                    "rntrcCadastro": "01234567",
                    "rntrcSituacao": "Ativo",
                    "anttRntrcTipo": "TAC",
                    "anttRntrcEquiparadoTac": "",
                    "numDependentes": 0,
                    "cartoes": [],
                    "contas": []
                }
                """));

        var result = await _client.Favorecidos.FindAsync(new FindFavoredRequest
        {
            ContratanteCnpj     = "12345678000199",
            FavorecidoDocNumero = "12345678901",
            FavorecidoDocTipo   = 1,
        });

        Assert.True(result.IsSuccess);
        Assert.Equal("CARLOS MENDES", result.Value.Nome);
        Assert.Equal("01234567",      result.Value.RntrcCadastro);
    }

    [Fact]
    public async Task FindFavored_EnviaApiKey_NoHeader()
    {
        _server
            .Given(Request.Create().WithPath("/api/v1/favorecidos").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""{ "nome": "X", "statusRntrc": "A", "rntrcCadastro": "1", "rntrcSituacao": "Ativo", "anttRntrcTipo": "", "anttRntrcEquiparadoTac": "", "numDependentes": 0, "cartoes": [], "contas": [] }"""));

        await _client.Favorecidos.FindAsync(new FindFavoredRequest
        {
            ContratanteCnpj     = "X",
            FavorecidoDocNumero = "X",
            FavorecidoDocTipo   = 1,
        });

        var requests = _server.LogEntries.ToList();
        Assert.Single(requests);
        var headers = requests[0].RequestMessage.Headers!;
        Assert.True(headers.ContainsKey("X-Api-Key"));
        Assert.Equal(FAKE_API_KEY, headers["X-Api-Key"].First());
    }

    [Fact]
    public async Task FindFavored_Erro422_ParseiaErrorsArray()
    {
        _server
            .Given(Request.Create().WithPath("/api/v1/favorecidos").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(422)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""{ "errors": ["Código 022 — RNTRC inválido", "Documento mal formatado"] }"""));

        var result = await _client.Favorecidos.FindAsync(new FindFavoredRequest
        {
            ContratanteCnpj     = "X",
            FavorecidoDocNumero = "X",
            FavorecidoDocTipo   = 1,
        });

        Assert.True(result.IsFailed);
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains(result.Errors, e => e.Message.Contains("RNTRC inválido"));
    }

    [Fact]
    public async Task FindFavored_Erro500_RetornaFailComStatusCode()
    {
        _server
            .Given(Request.Create().WithPath("/api/v1/favorecidos").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(500).WithBody("internal error"));

        var result = await _client.Favorecidos.FindAsync(new FindFavoredRequest
        {
            ContratanteCnpj     = "X",
            FavorecidoDocNumero = "X",
            FavorecidoDocTipo   = 1,
        });

        Assert.True(result.IsFailed);
        Assert.Contains(result.Errors, e => e.Message.Contains("500"));
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        _server.Stop();
        _server.Dispose();
    }

    // helper: implementação manual de IOptionsMonitor para isolar do DI
    private sealed class TestOptionsMonitor<T> : IOptionsMonitor<T>
    {
        public TestOptionsMonitor(T current) => CurrentValue = current;
        public T CurrentValue { get; }
        public T Get(string? name) => CurrentValue;
        public IDisposable? OnChange(Action<T, string?> listener) => null;
    }
}
