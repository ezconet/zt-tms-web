using FluentResults;

namespace Zenatur.Tms.Application.LegacyBridge;

/// <summary>
/// Cliente do Zenatur LegacyBridge — consulta dados cadastrais do SQL legado Zenatur.
/// Endpoint /motoristas/{doc} aceita CPF e CNPJ (digits only).
/// </summary>
public interface ILegacyBridgeClient
{
    Task<Result<MotoristaLegadoResponse?>> GetMotoristaAsync(string cpfOuCnpj, CancellationToken ct = default);
    Task<Result<VeiculoLegadoResponse?>>   GetVeiculoAsync(string placa, CancellationToken ct = default);
}

public record MotoristaLegadoResponse(
    string                              Cpf,
    string                              Nome,
    DateOnly?                           DataNascimento,
    string?                             Telefone,
    EnderecoLegado?                     Endereco,
    DateOnly?                           AnttValidade,
    RntrcLegado?                        Rntrc,
    IReadOnlyList<VeiculoMotoristaItem>? Veiculos);

public record EnderecoLegado(
    string?  Logradouro,
    string?  Numero,
    string?  Complemento,
    string?  Bairro,
    string?  CidadeIbge,
    string?  Uf,
    string?  Cep);

public record RntrcLegado(string Numero, bool? Ativo, DateOnly? Validade);

/// <summary>
/// Veículo vinculado ao motorista (vem inline na resposta /motoristas/{doc}).
/// `TipoVeiculo` usa codificação **legada Zenatur** — não bate com SEFAZ/CategoriaVeiculo CIOT.
/// De-para legado→CIOT futuramente via `TipoVeiculoLegadoMapper`.
/// </summary>
public record VeiculoMotoristaItem(
    string                Placa,
    TipoVeiculoLegado?    TipoVeiculo,
    string?               Renavam,
    int?                  AnoFabricacao,
    int?                  AnoModelo,
    string?               Marca,
    string?               Modelo,
    decimal?              Tara,
    decimal?              CapacidadeKg,
    string?               Rntrc);

public record TipoVeiculoLegado(int Id, string Descricao);

public record VeiculoLegadoResponse(
    string                Placa,
    int                   TipoVeiculo,
    string?               Renavam,
    int?                  AnoFabricacao,
    int?                  AnoModelo,
    string?               Marca,
    string?               Modelo,
    int?                  Tara,
    int?                  CapacidadeKg,
    ProprietarioLegado?   Proprietario);

public record ProprietarioLegado(string Documento, string TipoDocumento, string Nome);
