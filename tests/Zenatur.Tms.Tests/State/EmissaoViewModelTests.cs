using Zenatur.Tms.Application.State;

namespace Zenatur.Tms.Tests.State;

public class EmissaoViewModelTests
{
    [Fact]
    public void ValorLiquido_SomaFreteEPedagioMenosImpostos()
    {
        var vm = new EmissaoViewModel
        {
            ValorFrete    = 4000m,
            ValorPedagio  =  300m,
            ValorImpostos =  500m,
        };

        Assert.Equal(3800m, vm.ValorLiquido);
    }

    [Fact]
    public void ValorLiquido_SemImpostos_FretePedagioSomados()
    {
        var vm = new EmissaoViewModel { ValorFrete = 1000m, ValorPedagio = 200m };
        Assert.Equal(1200m, vm.ValorLiquido);
    }

    [Fact]
    public void ValorLiquido_ImpostosMaioresQueFrete_ResultadoNegativo()
    {
        var vm = new EmissaoViewModel { ValorFrete = 100m, ValorImpostos = 200m };
        Assert.Equal(-100m, vm.ValorLiquido);
    }

    [Fact]
    public void ParcelasQuantidade_DefaultEh1()
    {
        var vm = new EmissaoViewModel();
        Assert.Equal(1, vm.ParcelasQuantidade);
    }
}
