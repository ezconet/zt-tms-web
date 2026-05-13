using Zenatur.Tms.Application.State;

namespace Zenatur.Tms.Tests.State;

public class ParcelaCalculatorTests
{
    [Fact]
    public void Calcular_ValorZero_RetornaListaVazia()
    {
        var parcelas = ParcelaCalculator.Calcular(0m, 3, DateTime.Today);
        Assert.Empty(parcelas);
    }

    [Fact]
    public void Calcular_QtdZero_RetornaListaVazia()
    {
        var parcelas = ParcelaCalculator.Calcular(1000m, 0, DateTime.Today);
        Assert.Empty(parcelas);
    }

    [Fact]
    public void Calcular_QtdAcima12_LimitaEm12()
    {
        var parcelas = ParcelaCalculator.Calcular(1200m, 50, DateTime.Today);
        Assert.Equal(12, parcelas.Count);
    }

    [Fact]
    public void Calcular_ValorDivisivel_TodasParcelasIguais()
    {
        var parcelas = ParcelaCalculator.Calcular(3000m, 3, new DateTime(2026, 5, 15));

        Assert.Equal(3, parcelas.Count);
        Assert.All(parcelas, p => Assert.Equal(1000m, p.Valor));
    }

    [Fact]
    public void Calcular_ValorNaoDivisivel_RestoVaiNaUltimaParcela()
    {
        // 1000 / 3 = 333.33...  → 333.33 nas primeiras, 333.34 na última
        var parcelas = ParcelaCalculator.Calcular(1000m, 3, new DateTime(2026, 5, 15));

        Assert.Equal(3, parcelas.Count);
        Assert.Equal(333.33m, parcelas[0].Valor);
        Assert.Equal(333.33m, parcelas[1].Valor);
        Assert.Equal(333.34m, parcelas[2].Valor);

        // Invariante crítico: soma EXATA (sem perder centavos)
        Assert.Equal(1000m, parcelas.Sum(p => p.Valor));
    }

    [Fact]
    public void Calcular_VencimentosSequenciaisMensais()
    {
        var primeiro = new DateTime(2026, 1, 31);
        var parcelas = ParcelaCalculator.Calcular(300m, 3, primeiro);

        Assert.Equal(new DateTime(2026, 1, 31), parcelas[0].Vencimento);
        Assert.Equal(new DateTime(2026, 2, 28), parcelas[1].Vencimento); // AddMonths ajusta para fim do mês
        Assert.Equal(new DateTime(2026, 3, 31), parcelas[2].Vencimento);
    }

    [Fact]
    public void Calcular_NumeroSequencial1aN()
    {
        var parcelas = ParcelaCalculator.Calcular(500m, 5, DateTime.Today);

        Assert.Equal([1, 2, 3, 4, 5], parcelas.Select(p => p.Numero));
    }

    [Fact]
    public void Calcular_ParcelaUnica_ValorIntegral()
    {
        var parcelas = ParcelaCalculator.Calcular(2850m, 1, new DateTime(2026, 5, 20));

        Assert.Single(parcelas);
        Assert.Equal(2850m, parcelas[0].Valor);
        Assert.Equal(1,     parcelas[0].Numero);
    }
}
