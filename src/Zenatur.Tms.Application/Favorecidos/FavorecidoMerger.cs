using Zenatur.Ciot.Client.Favorecidos;

namespace Zenatur.Tms.Application.Favorecidos;

/// <summary>
/// Merge de dados do favorecido a partir das fontes disponíveis.
/// Hoje: 1 fonte (CIOT API). Futuro (A14-A17): + LegacyBridge com prioridade em dados cadastrais.
/// </summary>
public static class FavorecidoMerger
{
    /// <summary>
    /// Converte resposta CIOT em <see cref="FavorecidoDto"/>. Retorna null se input nulo.
    /// </summary>
    public static FavorecidoDto? Merge(FindFavoredResponse? ciot, string documento)
    {
        if (ciot is null) return null;

        return new FavorecidoDto
        {
            Documento      = documento,
            Nome           = ciot.Nome,
            Rntrc          = ciot.RntrcCadastro,
            RntrcSituacao  = ciot.RntrcSituacao,
            MeiosPagamento = MapearMeiosCiot(ciot),
        };
    }

    private static IReadOnlyList<MeioPagamentoDto> MapearMeiosCiot(FindFavoredResponse ciot)
    {
        var lista = new List<MeioPagamentoDto>();
        foreach (var c in ciot.Cartoes)
            lista.Add(new("Cartao", $"Cartão *{c.Numero[^4..]}", c.Numero));
        foreach (var c in ciot.Contas)
            lista.Add(new("Conta",  $"Banco {c.Banco} C/C {c.Numero}", c.Numero));
        return lista;
    }
}
