using Microsoft.Extensions.Logging;
using Zenatur.Tms.Application.Motoristas;
using Zenatur.Tms.Infrastructure.Ciot;

namespace Zenatur.Tms.Infrastructure.Motoristas;

internal sealed class CiotApiMotoristaService : IMotoristaManagementService
{
    private readonly CiotApiHttpClient _http;
    private readonly ILogger<CiotApiMotoristaService> _logger;
    private const string ContratanteCnpj = "53717120000170";

    private static string SoDigitos(string? s) => new((s ?? "").Where(char.IsDigit).ToArray());

    public CiotApiMotoristaService(CiotApiHttpClient http, ILogger<CiotApiMotoristaService> logger)
    {
        _http   = http;
        _logger = logger;
    }

    public async Task<IReadOnlyList<MotoristaListItem>> ListarAsync(string? termo = null, CancellationToken ct = default)
    {
        var qs = $"?contratanteCnpj={ContratanteCnpj}&take=200";
        if (!string.IsNullOrWhiteSpace(termo)) qs += $"&q={Uri.EscapeDataString(termo.Trim())}";

        var r = await _http.GetAsync<List<MotoristaApiResponse>>($"/api/v1/motoristas/local{qs}", ct);
        if (r.IsFailed)
        {
            _logger.LogWarning("Falha listar motoristas: {Err}", string.Join("; ", r.Errors.Select(e => e.Message)));
            return [];
        }
        return r.Value.Select(Map).ToList();
    }

    public async Task<MotoristaListItem?> ObterAsync(string cpf, CancellationToken ct = default)
    {
        var r = await _http.GetAsync<MotoristaApiResponse>($"/api/v1/motoristas/local/{cpf}?contratanteCnpj={ContratanteCnpj}", ct);
        return r.IsFailed ? null : Map(r.Value);
    }

    public async Task<MotoristaListItem> SalvarAsync(MotoristaListItem m, bool exigirNovo = false, CancellationToken ct = default)
    {
        var existente = await ObterAsync(m.Cpf, ct);
        if (exigirNovo && existente is not null)
            throw new InvalidOperationException($"CPF {m.Cpf} já cadastrado como motorista.");

        var ddd  = SoDigitos(m.TelefoneDdd);
        ddd      = ddd.Length > 0 ? ddd.PadLeft(3, '0') : "";
        var fone = SoDigitos(m.TelefoneNumero);
        if (fone.Length > 9) fone = fone[^9..];   // coluna nvarchar(9)

        var body = new
        {
            contratanteCnpj = ContratanteCnpj,
            cpf             = m.Cpf,
            nome            = m.Nome,
            dataNascimento  = m.DataNascimento,
            email           = m.Email,
            telefoneDdd     = ddd,
            telefoneNumero  = fone,
            rntrc           = m.Rntrc,
            rntrcSituacao   = m.RntrcSituacao,
            rntrcValidade   = m.RntrcValidade,
            cnhNumero       = m.CnhNumero,
            cnhCategoria    = m.CnhCategoria,
            cnhValidade     = m.CnhValidade,
            rgNumero        = SoDigitos(m.RgNumero),
            rgUf            = m.RgUf,
            logradouro      = m.Logradouro,
            enderecoNumero  = m.EnderecoNumero,
            bairro          = m.Bairro,
            cidade          = m.EnderecoCidade,
            cidadeIbge      = int.TryParse(m.EnderecoCidadeIbge, out var ci) ? ci : (int?)null,
            enderecoUf      = m.EnderecoUf,
            cep             = SoDigitos(m.Cep),
        };

        if (existente is null)
        {
            var r = await _http.PostAsync<object, int>("/api/v1/motoristas", body, ct);
            if (r.IsFailed) throw new InvalidOperationException("Falha criar motorista: " + string.Join("; ", r.Errors.Select(e => e.Message)));
        }
        else
        {
            var r = await _http.PutAsync<object, object>($"/api/v1/motoristas/{m.Cpf}", body, ct);
            if (r.IsFailed) throw new InvalidOperationException("Falha atualizar motorista: " + string.Join("; ", r.Errors.Select(e => e.Message)));
        }

        return await ObterAsync(m.Cpf, ct) ?? m;
    }

    private static MotoristaListItem Map(MotoristaApiResponse r) => new()
    {
        Id             = r.id,
        Cpf            = r.cpf ?? "",
        Nome           = r.nome ?? "",
        DataNascimento = r.dataNascimento,
        Email          = r.email,
        Rntrc          = r.rntrc,
        RntrcSituacao  = r.rntrcSituacao,
        RntrcValidade  = r.rntrcValidade,
        TelefoneDdd    = r.telefoneDdd,
        TelefoneNumero = r.telefoneNumero,
        CnhNumero      = r.cnhNumero,
        CnhCategoria   = r.cnhCategoria,
        CnhValidade    = r.cnhValidade,
        RgNumero       = r.rgNumero,
        RgUf           = r.rgUf,
        Logradouro     = r.logradouro,
        EnderecoNumero = r.enderecoNumero,
        Bairro         = r.bairro,
        EnderecoCidade = r.cidade,
        EnderecoCidadeIbge = r.cidadeIbge?.ToString(),
        EnderecoUf     = r.enderecoUf,
        Cep            = r.cep,
        Ativo          = r.ativo,
        AtualizadoEm   = r.atualizadoEm,
    };

    private sealed record MotoristaApiResponse(
        int       id,
        string?   cpf,
        string?   nome,
        DateOnly? dataNascimento,
        string?   email,
        string?   rntrc,
        string?   rntrcSituacao,
        DateOnly? rntrcValidade,
        string?   telefoneDdd,
        string?   telefoneNumero,
        string?   cnhNumero,
        string?   cnhCategoria,
        DateOnly? cnhValidade,
        string?   rgNumero,
        string?   rgUf,
        string?   logradouro,
        int?      enderecoNumero,
        string?   bairro,
        string?   cidade,
        string?   enderecoUf,
        string?   cep,
        int?      cidadeIbge,
        bool      ativo,
        DateTime  atualizadoEm);
}
