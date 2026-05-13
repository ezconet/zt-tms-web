namespace Zenatur.Tms.Application.State;

public static class ParcelaCalculator
{
    /// <summary>
    /// Divide valor em N parcelas com 2 casas. Resto vai para última parcela.
    /// Vencimentos sequenciais mensais a partir de <paramref name="primeiroVencimento"/>.
    /// Retorna lista vazia se valor inválido.
    /// </summary>
    public static List<ParcelaViewModel> Calcular(decimal valor, int qtd, DateTime primeiroVencimento)
    {
        if (valor <= 0 || qtd < 1) return [];
        qtd = Math.Min(qtd, 12);

        var basePorPp = Math.Floor(valor / qtd * 100m) / 100m;
        var soma      = basePorPp * qtd;
        var resto     = valor - soma;

        var lista = new List<ParcelaViewModel>(qtd);
        for (int i = 0; i < qtd; i++)
        {
            lista.Add(new ParcelaViewModel
            {
                Numero     = i + 1,
                Valor      = i == qtd - 1 ? basePorPp + resto : basePorPp,
                Vencimento = primeiroVencimento.AddMonths(i),
            });
        }
        return lista;
    }
}
