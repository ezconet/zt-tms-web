using Zenatur.Ciot.Client.Favorecidos;
using Zenatur.Tms.Application.Favorecidos;
using Zenatur.Tms.Application.LegacyBridge;

namespace Zenatur.Tms.Tests.Favorecidos;

public class FavorecidoMergerTests
{
    [Fact]
    public void Merge_AmbasFontesNulas_RetornaNull()
    {
        Assert.Null(FavorecidoMerger.Merge(ciot: null, bridge: null, "12345678901"));
    }

    [Fact]
    public void Merge_SoCiot_RetornaFavorecidoCiot()
    {
        var ciot = new FindFavoredResponse
        {
            Nome           = "ROBERTO FARIA",
            RntrcCadastro  = "03456789",
            RntrcSituacao  = "Ativo",
            Contas         = [new ContaResponse("341", "1234", "5", "987654-3", "C/C", "Ativa", null, null, null)],
        };

        var result = FavorecidoMerger.Merge(ciot, bridge: null, "55566677788");

        Assert.NotNull(result);
        Assert.Equal("ROBERTO FARIA", result.Nome);
        Assert.Equal("03456789",      result.Rntrc);
        Assert.True(result.RntrcAtivo);
        Assert.Single(result.MeiosPagamento);
    }

    [Fact]
    public void Merge_SoBridge_RetornaFavorecidoBridge()
    {
        var bridge = new MotoristaLegadoResponse(
            Cpf:            "13294646720",
            Nome:           "JOAO DA SILVA",
            DataNascimento: new DateOnly(1980, 3, 15),
            Telefone:       "11999998888",
            Endereco:       null,
            Rntrc:          new RntrcLegado("12345678", true, null));

        var result = FavorecidoMerger.Merge(ciot: null, bridge, "13294646720");

        Assert.NotNull(result);
        Assert.Equal("JOAO DA SILVA", result.Nome);
        Assert.Equal("12345678",      result.Rntrc);
        Assert.True(result.RntrcAtivo);
        Assert.Empty(result.MeiosPagamento); // Bridge não tem meios
    }

    [Fact]
    public void Merge_BridgePrioridadeNome_BridgeVenceCiot()
    {
        var ciot = new FindFavoredResponse
        {
            Nome          = "JOAO DA SILVA CIOT", // CIOT tem outro nome
            RntrcCadastro = "01234567",
            RntrcSituacao = "Ativo",
        };
        var bridge = new MotoristaLegadoResponse(
            "12345678901",
            "JOAO DA SILVA BRIDGE", // Bridge é fonte de verdade cadastral
            null, null, null,
            new RntrcLegado("01234567", true, null));

        var result = FavorecidoMerger.Merge(ciot, bridge, "12345678901");

        Assert.NotNull(result);
        Assert.Equal("JOAO DA SILVA BRIDGE", result.Nome); // Bridge prevalece
    }

    [Fact]
    public void Merge_BridgeSemNome_FallbackCiot()
    {
        var ciot = new FindFavoredResponse
        {
            Nome          = "CARLOS MENDES CIOT",
            RntrcCadastro = "01234567",
            RntrcSituacao = "Ativo",
        };
        var bridge = new MotoristaLegadoResponse(
            "12345678901",
            "", // Bridge sem nome
            null, null, null, null);

        var result = FavorecidoMerger.Merge(ciot, bridge, "12345678901");

        Assert.NotNull(result);
        Assert.Equal("CARLOS MENDES CIOT", result.Nome); // fallback CIOT preencheu lacuna
    }

    [Fact]
    public void Merge_MeiosPagamentoSempreVemDoCiot()
    {
        var ciot = new FindFavoredResponse
        {
            Nome          = "X",
            RntrcCadastro = "1",
            RntrcSituacao = "Ativo",
            Cartoes       = [new CartaoResponse("4321111122223421", 1, 1)],
        };
        var bridge = new MotoristaLegadoResponse(
            "12345678901",
            "X",
            null, null, null,
            new RntrcLegado("1", true, null));

        var result = FavorecidoMerger.Merge(ciot, bridge, "12345678901");

        Assert.NotNull(result);
        Assert.Single(result.MeiosPagamento);
        Assert.Equal("Cartao", result.MeiosPagamento[0].Tipo);
        Assert.EndsWith("3421", result.MeiosPagamento[0].Descricao);
    }

    [Fact]
    public void Merge_BridgeRntrcInativo_MarcaInativo()
    {
        var bridge = new MotoristaLegadoResponse(
            "12345678901",
            "FULANO",
            null, null, null,
            new RntrcLegado("999", Ativo: false, null));

        var result = FavorecidoMerger.Merge(ciot: null, bridge, "12345678901");

        Assert.NotNull(result);
        Assert.False(result.RntrcAtivo);
        Assert.Equal("Inativo", result.RntrcSituacao);
    }
}
