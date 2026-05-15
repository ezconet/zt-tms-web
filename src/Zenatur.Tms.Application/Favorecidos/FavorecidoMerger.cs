using Zenatur.Ciot.Client.Favorecidos;
using Zenatur.Tms.Application.LegacyBridge;

namespace Zenatur.Tms.Application.Favorecidos;

/// <summary>
/// Merge de duas fontes do favorecido:
/// 1. <see cref="ILegacyBridgeClient"/> — Bridge (SQL legado Zenatur): **prioridade em dados cadastrais** (Nome, RNTRC quando ativo).
/// 2. <see cref="IFavorecidosClient"/> — CIOT API: **prioridade em meios de pagamento** (cartões/contas) e backup quando Bridge não tem.
/// </summary>
public static class FavorecidoMerger
{
    /// <summary>
    /// Merge das duas fontes. Retorna <c>null</c> se ambas vierem nulas/sem resultado.
    /// </summary>
    public static FavorecidoDto? Merge(
        FindFavoredResponse?     ciot,
        MotoristaLegadoResponse? bridge,
        string                   documento)
    {
        if (ciot is null && bridge is null) return null;

        // Nome: Bridge prioritário (sistema operacional Zenatur), fallback CIOT
        var nome = Preferir(bridge?.Nome, ciot?.Nome);

        // RNTRC: número agora só vem da CIOT (Bridge removeu o objeto rntrc do payload).
        var rntrc = ciot?.RntrcCadastro ?? string.Empty;

        // Situação RNTRC: Bridge → AnttValidade futura == Ativo; sentinela 1900-01-01 == Inativo.
        // Fallback CIOT.StatusRntrc se Bridge ausente.
        var rntrcSituacao = DeriveRntrcSituacao(bridge?.AnttValidade) ?? ciot?.RntrcSituacao ?? "";

        // Meios pagamento: CIOT é fonte de verdade (Bridge legado não tem essa info)
        var meios = MapearMeiosCiot(ciot);

        var (ddd, numero) = ParseTelefone(bridge?.Telefone);

        return new FavorecidoDto
        {
            Documento          = documento,
            Nome               = nome,
            Rntrc              = rntrc,
            RntrcSituacao      = rntrcSituacao,
            TelefoneDdd        = ddd,
            TelefoneNumero     = numero,
            DataNascimento     = bridge?.DataNascimento,
            EnderecoLogradouro = bridge?.Endereco?.Logradouro,
            EnderecoNumero     = bridge?.Endereco?.Numero,
            EnderecoBairro     = bridge?.Endereco?.Bairro,
            EnderecoCidadeIbge = null, // Bridge não envia mais; resolução IBGE via lookup local quando necessário
            EnderecoUf         = bridge?.Endereco?.Uf,
            EnderecoCep        = bridge?.Endereco?.Cep,
            MeiosPagamento     = meios,
        };
    }

    private static string? DeriveRntrcSituacao(DateOnly? anttValidade)
    {
        if (anttValidade is null) return null;
        // Sentinela legado: "1900-01-01" = sem RNTRC válido
        if (anttValidade.Value.Year < 1950) return "Inativo";
        return anttValidade.Value >= DateOnly.FromDateTime(DateTime.Today) ? "Ativo" : "Inativo";
    }

    /// <summary>
    /// Aceita "(11) 98765-4321", "11987654321", "1198765-4321", "(31) 4002-8922" etc.
    /// Retorna (ddd, numero) com apenas dígitos. Bridge é única fonte hoje.
    /// </summary>
    private static (string? Ddd, string? Numero) ParseTelefone(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return (null, null);
        var digits = new string(raw.Where(char.IsDigit).ToArray());
        if (digits.Length < 10) return (null, null);
        if (digits.Length > 11) digits = digits[^11..]; // descarta prefixo país se vier
        return (digits[..2], digits[2..]);
    }

    private static string Preferir(string? primario, string? secundario) =>
        !string.IsNullOrWhiteSpace(primario) ? primario : secundario ?? string.Empty;

    private static IReadOnlyList<MeioPagamentoDto> MapearMeiosCiot(FindFavoredResponse? ciot)
    {
        if (ciot is null) return [];
        var lista = new List<MeioPagamentoDto>();
        foreach (var c in ciot.Cartoes)
            lista.Add(new("Cartao", $"Cartão *{c.Numero[^4..]}", c.Numero));
        foreach (var c in ciot.Contas)
            lista.Add(new("Conta",  $"Banco {c.Banco} C/C {c.Numero}", c.Numero));
        return lista;
    }
}
