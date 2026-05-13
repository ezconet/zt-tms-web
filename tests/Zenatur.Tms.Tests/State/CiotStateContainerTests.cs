using Zenatur.Tms.Application.State;

namespace Zenatur.Tms.Tests.State;

public class CiotStateContainerTests
{
    [Fact]
    public void IniciarEmissao_DefineFase1()
    {
        var state = new CiotStateContainer();
        state.IniciarEmissao(new EmissaoViewModel());

        Assert.NotNull(state.EmissaoEmAndamento);
        Assert.Equal(1, state.FaseAtual);
    }

    [Fact]
    public void AvancarFase_AtePosicao4_NaoUltrapassa()
    {
        var state = new CiotStateContainer();
        state.IniciarEmissao(new EmissaoViewModel());

        state.AvancarFase();
        state.AvancarFase();
        state.AvancarFase();
        state.AvancarFase(); // tenta passar de 4

        Assert.Equal(4, state.FaseAtual);
    }

    [Fact]
    public void RetrocederFase_AtePosicao1_NaoFicaAbaixo()
    {
        var state = new CiotStateContainer();
        state.IniciarEmissao(new EmissaoViewModel());

        state.RetrocederFase();
        state.RetrocederFase();

        Assert.Equal(1, state.FaseAtual);
    }

    [Fact]
    public void Limpar_ResetaEstado()
    {
        var state = new CiotStateContainer();
        state.IniciarEmissao(new EmissaoViewModel { FavorecidoNome = "Carlos" });
        state.AvancarFase();

        state.Limpar();

        Assert.Null(state.EmissaoEmAndamento);
        Assert.Equal(0, state.FaseAtual);
    }

    [Fact]
    public void OnChange_DisparadoEmIniciarAvancarRetrocederLimpar()
    {
        var state = new CiotStateContainer();
        var calls = 0;
        state.OnChange += () => calls++;

        state.IniciarEmissao(new EmissaoViewModel());
        state.AvancarFase();
        state.RetrocederFase();
        state.Limpar();

        Assert.Equal(4, calls);
    }
}
