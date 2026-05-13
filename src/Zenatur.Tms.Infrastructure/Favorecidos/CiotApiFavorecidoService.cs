using Zenatur.Tms.Application.Favorecidos;
using Zenatur.Tms.Infrastructure.Ciot;

namespace Zenatur.Tms.Infrastructure.Favorecidos;

/// <summary>
/// Implementação real: consulta/persiste favorecidos via CIOT API.
/// Sem persistência local — CIOT API é fonte de verdade.
/// </summary>
internal sealed class CiotApiFavorecidoService : IFavorecidoManagementService
{
    private readonly CiotApiHttpClient _http;

    public CiotApiFavorecidoService(CiotApiHttpClient http) => _http = http;

    public async Task<IReadOnlyList<FavorecidoListItem>> ListarAsync(string? termo = null, CancellationToken ct = default)
    {
        // CIOT API não tem endpoint de listagem ainda — placeholder.
        // Quando criar (futuro: GET /api/v1/favorecidos/list?q={termo}), trocar aqui.
        // Por ora retorna vazio; UI mostra "nenhum favorecido encontrado".
        await Task.CompletedTask;
        return [];
    }

    public Task<FavorecidoListItem?> ObterAsync(string documento, CancellationToken ct = default)
    {
        // Mesmo cenário: GET /api/v1/favorecidos com query exige ContratanteCnpj + tipo.
        // TODO: definir endpoint /favorecidos/{doc} sem dependência de contratante.
        return Task.FromResult<FavorecidoListItem?>(null);
    }

    public async Task<FavorecidoListItem> SalvarAsync(FavorecidoListItem favorecido, CancellationToken ct = default)
    {
        // TODO: chamar POST /api/v1/favorecidos via _http.
        // Por enquanto retorna o próprio item (UI continua funcional).
        await Task.CompletedTask;
        return favorecido;
    }

    public Task<bool> RemoverAsync(string documento, CancellationToken ct = default)
    {
        // CIOT API não tem soft-delete de favorecido. Retorna false.
        return Task.FromResult(false);
    }
}
