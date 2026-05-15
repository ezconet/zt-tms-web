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

    /// <summary>
    /// Busca todos os dados de uma viagem do legado pelo número da Minuta
    /// (favorecido, motorista, veículo, documentos, origem/destino, pontos de
    /// parada e valor do frete). Retorna null quando a minuta não existe.
    /// Contrato: specs/tms/new-items/to-validate/08-minuta-bridge-response.example.json
    /// </summary>
    Task<Result<MinutaViagemResponse?>> GetMinutaAsync(string numeroMinuta, CancellationToken ct = default);
}

public record MinutaViagemResponse(
    string                              NumeroMinuta,
    MinutaFavorecidoLegado?             Favorecido,
    MinutaMotoristaLegado?              Motorista,
    MinutaVeiculoLegado?                Veiculo,
    IReadOnlyList<MinutaDocumentoLegado> Documentos,
    MinutaLocalLegado?                  Origem,
    MinutaLocalLegado?                  Destino,
    IReadOnlyList<MinutaPontoParadaLegado> PontosParada,
    decimal?                            ValorFreteLegado);

public record MinutaFavorecidoLegado(
    string          Documento,
    int             DocumentoTipo,
    string          Nome,
    DateOnly?       DataNascimento,
    string?         Rntrc,
    string?         TelefoneDdd,
    string?         TelefoneNumero,
    EnderecoLegado? Endereco);

public record MinutaMotoristaLegado(
    string    Cpf,
    string    Nome,
    DateOnly? DataNascimento,
    string?   CnhNumero,
    string?   CnhCategoria,
    DateOnly? CnhValidade,
    string?   RgNumero,
    string?   RgUf,
    string?   TelefoneDdd,
    string?   TelefoneNumero);

public record MinutaVeiculoLegado(
    string                          Placa,
    TipoVeiculoLegado?              TipoVeiculoLegado,
    string?                         Renavam,
    int?                            AnoFabricacao,
    int?                            AnoModelo,
    string?                         Marca,
    string?                         Modelo,
    string?                         Rntrc,
    IReadOnlyList<MinutaReboqueLegado> Reboques);

public record MinutaReboqueLegado(int Ordem, string Placa, TipoVeiculoLegado? TipoVeiculoLegado);

public record MinutaDocumentoLegado(
    string?  Tipo,
    int      TipoCodigo,
    string   Numero,
    string?  Serie,
    string?  Chave,
    decimal? Valor);

public record MinutaLocalLegado(string Cidade, string Uf, int? Ibge);

public record MinutaPontoParadaLegado(
    int     Ordem,
    string  Cidade,
    string  Uf,
    int?    Ibge,
    string? Logradouro,
    string? Numero,
    string? Tipo);

public record MotoristaLegadoResponse(
    string                              Documento,
    string                              TipoDocumento,
    string                              Nome,
    DateOnly?                           DataNascimento,
    string?                             Telefone,
    EnderecoLegado?                     Endereco,
    DateOnly?                           AnttValidade,
    IReadOnlyList<VeiculoMotoristaItem>? Veiculos);

public record EnderecoLegado(
    string?  Logradouro,
    string?  Numero,
    string?  Complemento,
    string?  Bairro,
    string?  Uf,
    string?  Cep);

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
