using FluentResults;
using Zenatur.Ciot.Client;
using Zenatur.Ciot.Client.Conformidade;
using Zenatur.Ciot.Client.Favorecidos;
using Zenatur.Ciot.Client.Outbox;
using Zenatur.Ciot.Client.Roteirizacao;
using Zenatur.Ciot.Client.Viagem;

namespace Zenatur.Tms.Infrastructure.Ciot;

/// <summary>
/// Implementação HTTP do ICiotClient consumindo CIOT API REST.
/// Apenas endpoints usados pelo TMS hoje estão implementados. Outros retornam Result.Fail.
/// Quando a fase de iteração estabilizar, o SDK Zenatur.Ciot.Client publicará uma versão HTTP.
/// </summary>
internal sealed class HttpCiotClient : ICiotClient
{
    public HttpCiotClient(CiotApiHttpClient http)
    {
        Favorecidos  = new HttpFavorecidosSubClient(http);
        Viagem       = new HttpViagemSubClient(http);
        Conformidade = new NotImplementedConformidadeClient();
        Roteirizacao = new HttpRoteirizacaoSubClient(http);
        Outbox       = new HttpOutboxSubClient(http);
    }

    public IFavorecidosClient   Favorecidos  { get; }
    public IConformidadeClient  Conformidade { get; }
    public IRoteirizacaoClient  Roteirizacao { get; }
    public IViagemClient        Viagem       { get; }
    public IOutboxClient        Outbox       { get; }
}

// ─── Favorecidos (usado por Stepper Fase 1) ───
internal sealed class HttpFavorecidosSubClient : IFavorecidosClient
{
    private readonly CiotApiHttpClient _http;
    public HttpFavorecidosSubClient(CiotApiHttpClient http) => _http = http;

    public Task<Result<FindFavoredResponse>> FindAsync(FindFavoredRequest r, CancellationToken ct = default)
    {
        var qs = $"ContratanteCnpj={Uri.EscapeDataString(r.ContratanteCnpj)}" +
                 $"&FavorecidoDocNumero={Uri.EscapeDataString(r.FavorecidoDocNumero)}" +
                 $"&FavorecidoDocTipo={r.FavorecidoDocTipo}" +
                 $"&ObterCartao={r.ObterCartao}" +
                 $"&ObterConta={r.ObterConta}";
        return _http.GetAsync<FindFavoredResponse>($"/api/v1/favorecidos?{qs}", ct);
    }

    public Task<Result<string>> InsertAsync(InsertFavoredRequest r, CancellationToken ct = default) =>
        _http.PostAsync<InsertFavoredRequest, string>("/api/v1/favorecidos", r, ct);

    public Task<Result<FindFavoredAccountResponse>> FindAccountAsync(FindFavoredAccountRequest r, CancellationToken ct = default) =>
        _http.GetAsync<FindFavoredAccountResponse>(
            $"/api/v1/favorecidos/conta?ContratanteCnpj={Uri.EscapeDataString(r.ContratanteCnpj)}" +
            $"&FavorecidoDocNumero={Uri.EscapeDataString(r.FavorecidoDocNumero)}" +
            $"&FavorecidoDocTipo={r.FavorecidoDocTipo}", ct);

    public Task<Result<string>> InsertAccountAsync(InsertFavoredAccountRequest r, CancellationToken ct = default) =>
        _http.PostAsync<InsertFavoredAccountRequest, string>("/api/v1/favorecidos/conta", r, ct);

    public Task<Result<FindCardResponse>> FindCardAsync(FindCardRequest r, CancellationToken ct = default) =>
        _http.GetAsync<FindCardResponse>(
            $"/api/v1/favorecidos/cartao?ContratanteCnpj={Uri.EscapeDataString(r.ContratanteCnpj)}" +
            $"&CartaoNumero={Uri.EscapeDataString(r.CartaoNumero)}", ct);
}

// ─── Viagem (Stepper Fase 4 + Financeiro) ───
internal sealed class HttpViagemSubClient : IViagemClient
{
    private readonly CiotApiHttpClient _http;
    public HttpViagemSubClient(CiotApiHttpClient http) => _http = http;

    public Task<Result<InsertTripResponse>> InsertTripAsync(InsertTripRequest r, CancellationToken ct = default) =>
        _http.PostAsync<InsertTripRequest, InsertTripResponse>("/api/v1/viagem", r, ct);

    public Task<Result<FindParcelStatusResponse>> FindParcelStatusAsync(FindParcelStatusRequest r, CancellationToken ct = default) =>
        _http.PostAsync<FindParcelStatusRequest, FindParcelStatusResponse>("/api/v1/viagem/parcelas/status", r, ct);

    public Task<Result<PayParcelResponse>> PayParcelAsync(PayParcelRequest r, CancellationToken ct = default) =>
        _http.PostAsync<PayParcelRequest, PayParcelResponse>("/api/v1/viagem/parcelas/pagar", r, ct);

    // Não usados pelo TMS hoje
    public Task<Result<FindTripResponse>>               FindTripAsync(FindTripRequest r, CancellationToken ct = default)                                 => NotImpl<FindTripResponse>();
    public Task<Result<InsertFreightContractResponse>>  InsertFreightContractAsync(InsertFreightContractRequest r, CancellationToken ct = default)        => NotImpl<InsertFreightContractResponse>();
    public Task<Result<FindFreightContractResponse>>    FindFreightContractAsync(FindFreightContractRequest r, CancellationToken ct = default)            => NotImpl<FindFreightContractResponse>();
    public Task<Result<UpdateTripResponse>>             UpdateTripAsync(UpdateTripRequest r, CancellationToken ct = default)                              => NotImpl<UpdateTripResponse>();
    public Task<Result<string>>                         UpdateFreightContractAsync(UpdateFreightContractRequest r, CancellationToken ct = default)        => NotImpl<string>();
    public Task<Result<string>>                         UpdateValuesTripAsync(UpdateValuesTripRequest r, CancellationToken ct = default)                  => NotImpl<string>();
    public Task<Result<string>>                         UpdateValuesFreightContractAsync(UpdateValuesFreightContractRequest r, CancellationToken ct = default) => NotImpl<string>();
    public Task<Result<CancelTripResponse>>             CancelTripAsync(CancelTripRequest r, CancellationToken ct = default)                              => NotImpl<CancelTripResponse>();
    public Task<Result<CloseFreightContractResponse>>   CloseFreightContractAsync(CloseFreightContractRequest r, CancellationToken ct = default)          => NotImpl<CloseFreightContractResponse>();
    public Task<Result<InsertParcelResponse>>           InsertParcelAsync(InsertParcelRequest r, CancellationToken ct = default)                          => NotImpl<InsertParcelResponse>();
    public Task<Result<UpdateParcelStatusResponse>>     UpdateParcelStatusAsync(UpdateParcelStatusRequest r, CancellationToken ct = default)              => NotImpl<UpdateParcelStatusResponse>();
    public Task<Result<UpdateTollStatusResponse>>       UpdateTollStatusAsync(UpdateTollStatusRequest r, CancellationToken ct = default)                  => NotImpl<UpdateTollStatusResponse>();
    public Task<Result<PayTollResponse>>                PayTollAsync(PayTollRequest r, CancellationToken ct = default)                                    => NotImpl<PayTollResponse>();

    private static Task<Result<T>> NotImpl<T>() =>
        Task.FromResult(Result.Fail<T>("HttpCiotClient: endpoint não implementado para TMS ainda."));
}

internal sealed class NotImplementedConformidadeClient : IConformidadeClient
{
    public Task<Result<FindRntrcResponse>> FindRntrcAsync(FindRntrcRequest r, CancellationToken ct = default) =>
        Task.FromResult(Result.Fail<FindRntrcResponse>("HttpCiotClient: Conformidade não usada pelo TMS hoje."));
    public Task<Result<FindFleetResponse>> FindFleetAsync(FindFleetRequest r, CancellationToken ct = default) =>
        Task.FromResult(Result.Fail<FindFleetResponse>("HttpCiotClient: Conformidade não usada pelo TMS hoje."));
    public Task<Result<FindTagResponse>>   FindTagAsync(FindTagRequest r, CancellationToken ct = default) =>
        Task.FromResult(Result.Fail<FindTagResponse>("HttpCiotClient: Conformidade não usada pelo TMS hoje."));
}

internal sealed class HttpRoteirizacaoSubClient : IRoteirizacaoClient
{
    private readonly CiotApiHttpClient _http;
    public HttpRoteirizacaoSubClient(CiotApiHttpClient http) => _http = http;

    public Task<Result<RouterResponse>> RouterAsync(RouterRequest r, CancellationToken ct = default)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append($"ContratanteCnpj={Uri.EscapeDataString(r.ContratanteCnpj)}");
        sb.Append($"&VeiculoCategoria={Uri.EscapeDataString(r.VeiculoCategoria)}");
        if (r.Origem?.Ibge is int oibge)   sb.Append($"&Origem.Ibge={oibge}");
        if (r.Destino?.Ibge is int dibge)  sb.Append($"&Destino.Ibge={dibge}");
        sb.Append($"&ObterPraca={r.ObterPraca.ToString().ToLowerInvariant()}");
        sb.Append($"&ObterRota={r.ObterRota.ToString().ToLowerInvariant()}");
        sb.Append($"&IdaVolta={r.IdaVolta.ToString().ToLowerInvariant()}");
        sb.Append($"&ObterUf={r.ObterUf.ToString().ToLowerInvariant()}");
        sb.Append($"&ObterPostos={r.ObterPostos.ToString().ToLowerInvariant()}");
        if (r.PedagioCaminho is int pc) sb.Append($"&PedagioCaminho={pc}");
        return _http.GetAsync<RouterResponse>($"/api/v1/roteirizacao/rota?{sb}", ct);
    }

    public Task<Result<InsertRouteResponse>> InsertRouteAsync(InsertRouteRequest r, CancellationToken ct = default) =>
        _http.PostAsync<InsertRouteRequest, InsertRouteResponse>("/api/v1/roteirizacao/rota", r, ct);

    public Task<Result<FindMinimumFreightResponse>> FindMinimumFreightAsync(FindMinimumFreightRequest r, CancellationToken ct = default)
    {
        var unidadeTipo = string.IsNullOrEmpty(r.UnidadeDocTipo) ? "" : $"&UnidadeDocTipo={Uri.EscapeDataString(r.UnidadeDocTipo)}";
        var unidadeNum  = string.IsNullOrEmpty(r.UnidadeDocNumero) ? "" : $"&UnidadeDocNumero={Uri.EscapeDataString(r.UnidadeDocNumero)}";
        var qs = $"ContratanteCnpj={Uri.EscapeDataString(r.ContratanteCnpj)}" +
                 $"&VeiculoCategoria={Uri.EscapeDataString(r.VeiculoCategoria)}" +
                 $"&DistanciaKm={r.DistanciaKm.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                 $"&CargaTipo={r.CargaTipo}" +
                 $"&AltoDesempenho={r.AltoDesempenho.ToString().ToLowerInvariant()}" +
                 $"&ContratacaoTipo={Uri.EscapeDataString(r.ContratacaoTipo)}" +
                 unidadeTipo + unidadeNum;
        return _http.GetAsync<FindMinimumFreightResponse>($"/api/v1/roteirizacao/frete-minimo?{qs}", ct);
    }
}

internal sealed class HttpOutboxSubClient : IOutboxClient
{
    private readonly CiotApiHttpClient _http;
    public HttpOutboxSubClient(CiotApiHttpClient http) => _http = http;

    public Task<Result<IReadOnlyList<PendingOutboxMessage>>> GetPendingAsync(int size = 50, CancellationToken ct = default) =>
        _http.GetAsync<IReadOnlyList<PendingOutboxMessage>>($"/api/v1/outbox/pending?size={size}", ct);

    public async Task<Result> AckAsync(AckOutboxRequest request, CancellationToken ct = default)
    {
        var r = await _http.PostAsync<AckOutboxRequest, object>("/api/v1/outbox/ack", request, ct);
        return r.IsSuccess ? Result.Ok() : Result.Fail(r.Errors);
    }
}
