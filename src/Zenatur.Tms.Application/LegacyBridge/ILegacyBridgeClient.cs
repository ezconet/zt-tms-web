using FluentResults;

namespace Zenatur.Tms.Application.LegacyBridge;

/// <summary>
/// Cliente do Zenatur LegacyBridge — consulta dados cadastrais do SQL legado Zenatur.
/// Fonte de verdade para dados cadastrais (nome, endereço, telefone) — A14-A17.
/// </summary>
public interface ILegacyBridgeClient
{
    Task<Result<MotoristaLegadoResponse?>> GetMotoristaAsync(string cpf, CancellationToken ct = default);
    Task<Result<VeiculoLegadoResponse?>>   GetVeiculoAsync(string placa, CancellationToken ct = default);
}

public record MotoristaLegadoResponse(
    string                       Cpf,
    string                       Nome,
    DateOnly?                    DataNascimento,
    string?                      Telefone,
    EnderecoLegado?              Endereco,
    RntrcLegado?                 Rntrc);

public record EnderecoLegado(
    string?  Logradouro,
    string?  Numero,
    string?  Complemento,
    string?  Bairro,
    string?  CidadeIbge,
    string?  Uf,
    string?  Cep);

public record RntrcLegado(string Numero, bool Ativo, DateOnly? Validade);

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
