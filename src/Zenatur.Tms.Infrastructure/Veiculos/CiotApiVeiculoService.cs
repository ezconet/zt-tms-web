using Microsoft.Extensions.Logging;
using Zenatur.Tms.Application.Veiculos;
using Zenatur.Tms.Infrastructure.Ciot;

namespace Zenatur.Tms.Infrastructure.Veiculos;

internal sealed class CiotApiVeiculoService : IVeiculoManagementService
{
    private readonly CiotApiHttpClient _http;
    private readonly ILogger<CiotApiVeiculoService> _logger;
    private const string ContratanteCnpj = "53717120000170";

    public CiotApiVeiculoService(CiotApiHttpClient http, ILogger<CiotApiVeiculoService> logger)
    {
        _http   = http;
        _logger = logger;
    }

    public async Task<IReadOnlyList<VeiculoListItem>> ListarAsync(string? termo = null, int? tipoTracao = null, CancellationToken ct = default)
    {
        var qs = $"?contratanteCnpj={ContratanteCnpj}&take=200";
        if (!string.IsNullOrWhiteSpace(termo)) qs += $"&q={Uri.EscapeDataString(termo.Trim())}";
        if (tipoTracao is int t) qs += $"&tipoTracao={t}";

        var r = await _http.GetAsync<List<VeiculoApiResponse>>($"/api/v1/veiculos/local{qs}", ct);
        if (r.IsFailed)
        {
            _logger.LogWarning("Falha listar veiculos: {Err}", string.Join("; ", r.Errors.Select(e => e.Message)));
            return [];
        }
        return r.Value.Select(Map).ToList();
    }

    public async Task<VeiculoListItem?> ObterAsync(string placa, CancellationToken ct = default)
    {
        var r = await _http.GetAsync<VeiculoApiResponse>($"/api/v1/veiculos/local/{placa}?contratanteCnpj={ContratanteCnpj}", ct);
        return r.IsFailed ? null : Map(r.Value);
    }

    public async Task<VeiculoListItem> SalvarAsync(VeiculoListItem v, CancellationToken ct = default)
    {
        var existente = await ObterAsync(v.Placa, ct);
        var body = new
        {
            contratanteCnpj  = ContratanteCnpj,
            placa            = v.Placa,
            categoriaVeiculo = v.CategoriaVeiculo,
            tipoTracao       = v.TipoTracao,
            renavam          = v.Renavam,
            eixos            = v.Eixos,
            anoFabricacao    = v.AnoFabricacao,
            anoModelo        = v.AnoModelo,
            marca            = v.Marca,
            modelo           = v.Modelo,
            tara             = v.Tara,
            capacidadeKg     = v.CapacidadeKg,
            rntrc            = v.Rntrc,
            rntrcSituacao    = v.RntrcSituacao,
            rntrcValidade    = v.RntrcValidade,
            proprietarioFavorecidoDocumento = v.ProprietarioFavorecidoDocumento,
        };

        if (existente is null)
        {
            var r = await _http.PostAsync<object, int>("/api/v1/veiculos", body, ct);
            if (r.IsFailed) throw new InvalidOperationException("Falha criar veículo: " + string.Join("; ", r.Errors.Select(e => e.Message)));
        }
        else
        {
            var r = await _http.PutAsync<object, object>($"/api/v1/veiculos/{v.Placa}", body, ct);
            if (r.IsFailed) throw new InvalidOperationException("Falha atualizar veículo: " + string.Join("; ", r.Errors.Select(e => e.Message)));
        }

        return await ObterAsync(v.Placa, ct) ?? v;
    }

    public async Task<bool> RemoverAsync(string placa, CancellationToken ct = default)
    {
        var r = await _http.DeleteAsync($"/api/v1/veiculos/{placa}?contratanteCnpj={ContratanteCnpj}", ct);
        return r.IsSuccess;
    }

    private static VeiculoListItem Map(VeiculoApiResponse r) => new()
    {
        Id               = r.id,
        Placa            = r.placa ?? "",
        Renavam          = r.renavam,
        CategoriaVeiculo = r.categoriaVeiculo ?? "",
        TipoTracao       = string.Equals(r.tipoTracao, "Reboque", StringComparison.OrdinalIgnoreCase) ? 2 : 1,
        Eixos            = r.eixos,
        AnoFabricacao    = r.anoFabricacao,
        AnoModelo        = r.anoModelo,
        Marca            = r.marca,
        Modelo           = r.modelo,
        Tara             = r.tara,
        CapacidadeKg     = r.capacidadeKg,
        Rntrc            = r.rntrc,
        RntrcSituacao    = r.rntrcSituacao,
        RntrcValidade    = r.rntrcValidade,
        ProprietarioFavorecidoDocumento = r.proprietarioFavorecidoDocumento,
        Ativo            = r.ativo,
        AtualizadoEm     = r.atualizadoEm,
    };

    private sealed record VeiculoApiResponse(
        int       id,
        string?   placa,
        string?   renavam,
        string?   categoriaVeiculo,
        string?   tipoTracao,
        int?      eixos,
        int?      anoFabricacao,
        int?      anoModelo,
        string?   marca,
        string?   modelo,
        decimal?  tara,
        decimal?  capacidadeKg,
        string?   rntrc,
        string?   rntrcSituacao,
        DateOnly? rntrcValidade,
        string?   proprietarioFavorecidoDocumento,
        bool      ativo,
        DateTime  atualizadoEm);
}
