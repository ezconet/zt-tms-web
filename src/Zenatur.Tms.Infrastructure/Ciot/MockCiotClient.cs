using FluentResults;
using Zenatur.Ciot.Client;
using Zenatur.Ciot.Client.Conformidade;
using Zenatur.Ciot.Client.Favorecidos;
using Zenatur.Ciot.Client.Outbox;
using Zenatur.Ciot.Client.Roteirizacao;
using Zenatur.Ciot.Client.Viagem;

namespace Zenatur.Tms.Infrastructure.Ciot;

internal sealed class MockCiotClient : ICiotClient
{
    public IFavorecidosClient  Favorecidos  { get; } = new MockFavorecidosClient();
    public IConformidadeClient Conformidade { get; } = new MockConformidadeClient();
    public IRoteirizacaoClient Roteirizacao { get; } = new MockRoteirizacaoClient();
    public IViagemClient       Viagem       { get; } = new MockViagemClient();
    public IOutboxClient       Outbox       { get; } = new MockOutboxClient();
}

// ─── Favorecidos: mock com dados realistas ───
internal sealed class MockFavorecidosClient : IFavorecidosClient
{
    private static readonly Dictionary<string, FindFavoredResponse> _db = new()
    {
        // CIOT conhece os mesmos CPFs do Pamcard, mas com dados parciais
        ["12345678901"] = new FindFavoredResponse
        {
            Nome           = "CARLOS MENDES",
            StatusRntrc    = "A",
            RntrcCadastro  = "01234567",
            RntrcSituacao  = "Ativo",
            AnttRntrcTipo  = "TAC",
            Cartoes        = [new CartaoResponse("4321111122223421", 1, 1)],
            Contas         = [],
        },
        ["98765432100"] = new FindFavoredResponse
        {
            Nome           = "ANTONIO SILVA",
            StatusRntrc    = "I",
            RntrcCadastro  = "09876543",
            RntrcSituacao  = "Inativo",
            Cartoes        = [],
            Contas         = [],
        },
        // CPF só conhecido pelo CIOT (não está no Pamcard)
        ["55566677788"] = new FindFavoredResponse
        {
            Nome           = "ROBERTO FARIA",
            StatusRntrc    = "A",
            RntrcCadastro  = "03456789",
            RntrcSituacao  = "Ativo",
            Cartoes        = [],
            Contas         = [new ContaResponse("341", "1234", "5", "987654-3", "C/C", "Ativa", null, null, null)],
        },
    };

    public Task<Result<FindFavoredResponse>> FindAsync(FindFavoredRequest request, CancellationToken ct = default)
    {
        var doc = request.FavorecidoDocNumero.Replace(".", "").Replace("-", "").Replace("/", "").Trim();
        return Task.FromResult(_db.TryGetValue(doc, out var f)
            ? Result.Ok(f)
            : Result.Fail<FindFavoredResponse>("Favorecido não encontrado."));
    }

    public Task<Result<string>>                     InsertAsync(InsertFavoredRequest request, CancellationToken ct = default) =>
        Task.FromResult(Result.Fail<string>("MockCiotClient: InsertAsync não implementado."));

    public Task<Result<FindFavoredAccountResponse>> FindAccountAsync(FindFavoredAccountRequest request, CancellationToken ct = default) =>
        Task.FromResult(Result.Fail<FindFavoredAccountResponse>("MockCiotClient: FindAccountAsync não implementado."));

    public Task<Result<string>>                     InsertAccountAsync(InsertFavoredAccountRequest request, CancellationToken ct = default) =>
        Task.FromResult(Result.Fail<string>("MockCiotClient: InsertAccountAsync não implementado."));

    public Task<Result<FindCardResponse>>           FindCardAsync(FindCardRequest request, CancellationToken ct = default) =>
        Task.FromResult(Result.Fail<FindCardResponse>("MockCiotClient: FindCardAsync não implementado."));
}

// ─── Stubs (T12+ implementam quando necessário) ───
internal sealed class MockConformidadeClient : IConformidadeClient
{
    public Task<Result<FindRntrcResponse>> FindRntrcAsync(FindRntrcRequest r, CancellationToken ct = default) =>
        Task.FromResult(Result.Fail<FindRntrcResponse>("MockCiotClient: não implementado."));
    public Task<Result<FindFleetResponse>> FindFleetAsync(FindFleetRequest r, CancellationToken ct = default) =>
        Task.FromResult(Result.Fail<FindFleetResponse>("MockCiotClient: não implementado."));
    public Task<Result<FindTagResponse>>   FindTagAsync(FindTagRequest r, CancellationToken ct = default) =>
        Task.FromResult(Result.Fail<FindTagResponse>("MockCiotClient: não implementado."));
}

internal sealed class MockRoteirizacaoClient : IRoteirizacaoClient
{
    public Task<Result<RouterResponse>>             RouterAsync(RouterRequest r, CancellationToken ct = default) =>
        Task.FromResult(Result.Fail<RouterResponse>("MockCiotClient: não implementado."));
    public Task<Result<InsertRouteResponse>>        InsertRouteAsync(InsertRouteRequest r, CancellationToken ct = default) =>
        Task.FromResult(Result.Fail<InsertRouteResponse>("MockCiotClient: não implementado."));
    public Task<Result<FindMinimumFreightResponse>> FindMinimumFreightAsync(FindMinimumFreightRequest r, CancellationToken ct = default) =>
        Task.FromResult(Result.Fail<FindMinimumFreightResponse>("MockCiotClient: não implementado."));
}

internal sealed class MockViagemClient : IViagemClient
{
    public async Task<Result<InsertTripResponse>> InsertTripAsync(InsertTripRequest r, CancellationToken ct = default)
    {
        // Simula latência da rede
        await Task.Delay(1500, ct);

        // 10% chance de erro pra exercitar fluxo de falha
        if (Random.Shared.Next(0, 10) == 0)
            return Result.Fail<InsertTripResponse>("Código 022 — RNTRC do favorecido encontra-se inválido para emissão (simulado).");

        var viagemId = Random.Shared.Next(100_000, 999_999).ToString();
        var digito   = Random.Shared.Next(0, 10).ToString();

        return Result.Ok(new InsertTripResponse
        {
            ViagemId          = viagemId,
            ViagemDigito      = digito,
            OrigemCidadeNome  = "Mock Origem",
            DestinoCidadeNome = "Mock Destino",
            RotaNome          = "Rota Direta",
            PedagioKm         = 482.5m,
            PedagioValor      = r.PedagioValor ?? 0m,
        });
    }
    public Task<Result<InsertFreightContractResponse>>  InsertFreightContractAsync(InsertFreightContractRequest r, CancellationToken ct = default) =>
        Task.FromResult(Result.Fail<InsertFreightContractResponse>("MockCiotClient: não implementado."));
    public Task<Result<FindTripResponse>>               FindTripAsync(FindTripRequest r, CancellationToken ct = default) =>
        Task.FromResult(Result.Fail<FindTripResponse>("MockCiotClient: não implementado."));
    public Task<Result<FindFreightContractResponse>>    FindFreightContractAsync(FindFreightContractRequest r, CancellationToken ct = default) =>
        Task.FromResult(Result.Fail<FindFreightContractResponse>("MockCiotClient: não implementado."));
    public Task<Result<UpdateTripResponse>>             UpdateTripAsync(UpdateTripRequest r, CancellationToken ct = default) =>
        Task.FromResult(Result.Fail<UpdateTripResponse>("MockCiotClient: não implementado."));
    public Task<Result<string>>                         UpdateFreightContractAsync(UpdateFreightContractRequest r, CancellationToken ct = default) =>
        Task.FromResult(Result.Fail<string>("MockCiotClient: não implementado."));
    public Task<Result<string>>                         UpdateValuesTripAsync(UpdateValuesTripRequest r, CancellationToken ct = default) =>
        Task.FromResult(Result.Fail<string>("MockCiotClient: não implementado."));
    public Task<Result<string>>                         UpdateValuesFreightContractAsync(UpdateValuesFreightContractRequest r, CancellationToken ct = default) =>
        Task.FromResult(Result.Fail<string>("MockCiotClient: não implementado."));
    public Task<Result<CancelTripResponse>>             CancelTripAsync(CancelTripRequest r, CancellationToken ct = default) =>
        Task.FromResult(Result.Fail<CancelTripResponse>("MockCiotClient: não implementado."));
    public Task<Result<CloseFreightContractResponse>>   CloseFreightContractAsync(CloseFreightContractRequest r, CancellationToken ct = default) =>
        Task.FromResult(Result.Fail<CloseFreightContractResponse>("MockCiotClient: não implementado."));
    public Task<Result<InsertParcelResponse>>           InsertParcelAsync(InsertParcelRequest r, CancellationToken ct = default) =>
        Task.FromResult(Result.Fail<InsertParcelResponse>("MockCiotClient: não implementado."));
    public Task<Result<FindParcelStatusResponse>>       FindParcelStatusAsync(FindParcelStatusRequest r, CancellationToken ct = default) =>
        Task.FromResult(Result.Fail<FindParcelStatusResponse>("MockCiotClient: não implementado."));
    public Task<Result<UpdateParcelStatusResponse>>     UpdateParcelStatusAsync(UpdateParcelStatusRequest r, CancellationToken ct = default) =>
        Task.FromResult(Result.Fail<UpdateParcelStatusResponse>("MockCiotClient: não implementado."));
    public Task<Result<PayParcelResponse>>              PayParcelAsync(PayParcelRequest r, CancellationToken ct = default) =>
        Task.FromResult(Result.Fail<PayParcelResponse>("MockCiotClient: não implementado."));
    public Task<Result<UpdateTollStatusResponse>>       UpdateTollStatusAsync(UpdateTollStatusRequest r, CancellationToken ct = default) =>
        Task.FromResult(Result.Fail<UpdateTollStatusResponse>("MockCiotClient: não implementado."));
    public Task<Result<PayTollResponse>>                PayTollAsync(PayTollRequest r, CancellationToken ct = default) =>
        Task.FromResult(Result.Fail<PayTollResponse>("MockCiotClient: não implementado."));
}

internal sealed class MockOutboxClient : IOutboxClient
{
    public Task<Result<IReadOnlyList<PendingOutboxMessage>>> GetPendingAsync(int size = 50, CancellationToken ct = default) =>
        Task.FromResult(Result.Ok<IReadOnlyList<PendingOutboxMessage>>([]));
    public Task<Result> AckAsync(AckOutboxRequest request, CancellationToken ct = default) =>
        Task.FromResult(Result.Ok());
}
