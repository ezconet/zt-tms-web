using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentResults;
using Microsoft.Extensions.Logging;
using Zenatur.Tms.Application.LegacyBridge;

namespace Zenatur.Tms.Infrastructure.LegacyBridge;

internal sealed class HttpLegacyBridgeClient : ILegacyBridgeClient
{
    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy        = JsonNamingPolicy.CamelCase,
    };

    private readonly HttpClient                       _http;
    private readonly ILogger<HttpLegacyBridgeClient>  _logger;

    public HttpLegacyBridgeClient(HttpClient http, ILogger<HttpLegacyBridgeClient> logger)
    {
        _http   = http;
        _logger = logger;
    }

    public async Task<Result<MotoristaLegadoResponse?>> GetMotoristaAsync(string cpf, CancellationToken ct = default)
    {
        var digits = new string(cpf.Where(char.IsDigit).ToArray());
        return await GetOrNotFoundAsync<MotoristaLegadoResponse>($"/v1/legacy/motoristas/{digits}", ct);
    }

    public async Task<Result<VeiculoLegadoResponse?>> GetVeiculoAsync(string placa, CancellationToken ct = default)
    {
        var p = placa.Replace("-", "").Trim().ToUpperInvariant();
        return await GetOrNotFoundAsync<VeiculoLegadoResponse>($"/v1/legacy/veiculos/{p}", ct);
    }

    public async Task<Result<MinutaViagemResponse?>> GetMinutaAsync(string numeroMinuta, CancellationToken ct = default)
    {
        var n = Uri.EscapeDataString(numeroMinuta.Trim());
        return await GetOrNotFoundAsync<MinutaViagemResponse>($"/v1/legacy/minutas/{n}", ct);
    }

    private async Task<Result<T?>> GetOrNotFoundAsync<T>(string url, CancellationToken ct) where T : class
    {
        try
        {
            // path relativo (sem "/" inicial) preserva sub-caminho da BaseAddress
            var response = await _http.GetAsync(url.TrimStart('/'), ct);
            if (response.StatusCode == HttpStatusCode.NotFound)
                return Result.Ok<T?>(null);

            if (!response.IsSuccessStatusCode)
            {
                var raw = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("LegacyBridge erro {Status} em {Url}: {Body}", (int)response.StatusCode, url, raw);
                return Result.Fail<T?>($"Bridge retornou {(int)response.StatusCode}: {raw}");
            }

            var data = await response.Content.ReadFromJsonAsync<T>(_json, ct);
            return Result.Ok<T?>(data);
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("LegacyBridge timeout em {Url}", url);
            return Result.Fail<T?>("Bridge timeout");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LegacyBridge falha {Url}", url);
            return Result.Fail<T?>($"Bridge erro: {ex.Message}");
        }
    }
}
