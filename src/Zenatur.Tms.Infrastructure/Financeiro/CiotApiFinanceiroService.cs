using Zenatur.Tms.Application.Financeiro;
using Zenatur.Tms.Infrastructure.Ciot;

namespace Zenatur.Tms.Infrastructure.Financeiro;

/// <summary>
/// Implementação real: parcelas e pagamentos via CIOT API (`/api/v1/viagem/parcelas/*`).
/// Sem persistência local — CIOT API é fonte de verdade.
/// </summary>
internal sealed class CiotApiFinanceiroService : IFinanceiroService
{
    private readonly CiotApiHttpClient _http;

    public CiotApiFinanceiroService(CiotApiHttpClient http) => _http = http;

    public async Task<IReadOnlyList<ParcelaFinanceiraItem>> ListarAsync(FiltroParcelas filtro, CancellationToken ct = default)
    {
        // Endpoint atual CIOT API exige ViagemId (consulta por viagem específica).
        // Para listagem global agregada, falta endpoint dedicado.
        // TODO: definir GET /api/v1/financeiro/parcelas?status=&from=&to= no lado CIOT.
        await Task.CompletedTask;
        return [];
    }

    public Task<ResumoFinanceiro> ObterResumoAsync(CancellationToken ct = default)
    {
        // TODO: GET /api/v1/financeiro/resumo na CIOT API
        return Task.FromResult(new ResumoFinanceiro());
    }

    public Task<bool> MarcarPagaAsync(string parcelaId, CancellationToken ct = default)
    {
        // TODO: chamar POST /api/v1/viagem/parcelas/pagar (PayParcelRequest)
        // Mapeamento parcelaId → ViagemId+NumeroParcela ainda não definido
        return Task.FromResult(false);
    }
}
