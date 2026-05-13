using Zenatur.Ciot.Client.Favorecidos;

namespace Zenatur.Tms.Application.Favorecidos;

public static class FavorecidoMerger
{
    /// <summary>
    /// Merge com Pamcard prioritário sobre CIOT. Lacunas do Pamcard preenchidas com CIOT.
    /// Retorna null se ambas as fontes vierem nulas.
    /// </summary>
    public static FavorecidoDto? Merge(FindFavoredResponse? ciot, FavorecidoDto? pamcard, string documento)
    {
        if (ciot is null && pamcard is null) return null;

        if (pamcard is not null)
        {
            return pamcard with
            {
                Nome          = Preferir(pamcard.Nome,          ciot?.Nome),
                Rntrc         = Preferir(pamcard.Rntrc,         ciot?.RntrcCadastro),
                RntrcSituacao = Preferir(pamcard.RntrcSituacao, ciot?.RntrcSituacao),
                MeiosPagamento = pamcard.MeiosPagamento.Count > 0
                    ? pamcard.MeiosPagamento
                    : MapearMeiosCiot(ciot),
            };
        }

        return new FavorecidoDto
        {
            Documento      = documento,
            Nome           = ciot!.Nome,
            Rntrc          = ciot.RntrcCadastro,
            RntrcSituacao  = ciot.RntrcSituacao,
            MeiosPagamento = MapearMeiosCiot(ciot),
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
