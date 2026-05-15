using System.Net.Http.Json;
using System.Text.Json;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace Zenatur.Tms.Infrastructure.Ciot;

/// <summary>
/// Wrapper HTTP tipado de baixo nível para CIOT API.
/// Sub-clients (Favorecidos, Viagem, Conformidade, Dominios) consomem este.
/// </summary>
public sealed class CiotApiHttpClient
{
    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy        = JsonNamingPolicy.CamelCase,
    };

    private readonly HttpClient                 _http;
    private readonly ILogger<CiotApiHttpClient> _logger;

    public CiotApiHttpClient(HttpClient http, ILogger<CiotApiHttpClient> logger)
    {
        _http   = http;
        _logger = logger;
    }

    public async Task<Result<T>> GetAsync<T>(string url, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.GetAsync(url, ct);
            return await ParseResponseAsync<T>(response, url, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha GET {Url}", url);
            return Result.Fail<T>($"Falha na chamada CIOT: {ex.Message}");
        }
    }

    public async Task<Result<TResponse>> PostAsync<TRequest, TResponse>(
        string url, TRequest body, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.PostAsJsonAsync(url, body, _json, ct);
            return await ParseResponseAsync<TResponse>(response, url, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha POST {Url}", url);
            return Result.Fail<TResponse>($"Falha na chamada CIOT: {ex.Message}");
        }
    }

    public async Task<Result<TResponse>> PutAsync<TRequest, TResponse>(
        string url, TRequest body, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.PutAsJsonAsync(url, body, _json, ct);
            return await ParseResponseAsync<TResponse>(response, url, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha PUT {Url}", url);
            return Result.Fail<TResponse>($"Falha na chamada CIOT: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(string url, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.DeleteAsync(url, ct);
            if (response.IsSuccessStatusCode) return Result.Ok();
            var raw = await response.Content.ReadAsStringAsync(ct);
            _logger.LogWarning("CIOT API DELETE erro {Status} em {Url}: {Body}", (int)response.StatusCode, url, raw);
            return Result.Fail($"CIOT API retornou {(int)response.StatusCode}: {raw}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha DELETE {Url}", url);
            return Result.Fail($"Falha na chamada CIOT: {ex.Message}");
        }
    }

    private async Task<Result<T>> ParseResponseAsync<T>(
        HttpResponseMessage response, string url, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
        {
            // 204 NoContent ou Content-Length 0 → retorna default (T pode ser object)
            if (response.StatusCode == System.Net.HttpStatusCode.NoContent ||
                response.Content.Headers.ContentLength is 0)
                return Result.Ok(default(T)!);

            var data = await response.Content.ReadFromJsonAsync<T>(_json, ct);
            return data is null
                ? Result.Fail<T>("Resposta vazia da CIOT API.")
                : Result.Ok(data);
        }

        // Erros padronizados: { errors: [...] }
        var raw = await response.Content.ReadAsStringAsync(ct);
        _logger.LogWarning("CIOT API erro {Status} em {Url}: {Body}",
            (int)response.StatusCode, url, raw);

        try
        {
            var errBody = JsonSerializer.Deserialize<ApiErrorBody>(raw, _json);
            if (errBody?.Errors is { Length: > 0 })
                return Result.Fail<T>(errBody.Errors);
        }
        catch { /* corpo não-JSON ou diferente do formato esperado */ }

        return Result.Fail<T>($"CIOT API retornou {(int)response.StatusCode}: {raw}");
    }

    private record ApiErrorBody(string[]? Errors);
}
