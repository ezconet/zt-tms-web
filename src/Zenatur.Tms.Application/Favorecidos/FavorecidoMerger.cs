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

        // RNTRC: Bridge tem dado mais atualizado (ANTT direto), fallback CIOT
        var rntrc         = Preferir(bridge?.Rntrc?.Numero, ciot?.RntrcCadastro);
        var rntrcSituacao = bridge?.Rntrc?.Ativo == true ? "Ativo"
                          : bridge?.Rntrc?.Ativo == false ? "Inativo"
                          : ciot?.RntrcSituacao ?? "";

        // Meios pagamento: CIOT é fonte de verdade (Bridge legado não tem essa info)
        var meios = MapearMeiosCiot(ciot);

        return new FavorecidoDto
        {
            Documento      = documento,
            Nome           = nome,
            Rntrc          = rntrc,
            RntrcSituacao  = rntrcSituacao,
            MeiosPagamento = meios,
        };
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
