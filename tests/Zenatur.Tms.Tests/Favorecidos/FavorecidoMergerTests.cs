using Zenatur.Ciot.Client.Favorecidos;
using Zenatur.Tms.Application.Favorecidos;

namespace Zenatur.Tms.Tests.Favorecidos;

public class FavorecidoMergerTests
{
    [Fact]
    public void Merge_AmbasFontesNulas_RetornaNull()
    {
        Assert.Null(FavorecidoMerger.Merge(null, null, "12345678901"));
    }

    [Fact]
    public void Merge_SoCiot_RetornaFavorecidoComDadosCiot()
    {
        var ciot = new FindFavoredResponse
        {
            Nome           = "ROBERTO FARIA",
            RntrcCadastro  = "03456789",
            RntrcSituacao  = "Ativo",
            Contas         = [new ContaResponse("341", "1234", "5", "987654-3", "C/C", "Ativa", null, null, null)],
        };

        var result = FavorecidoMerger.Merge(ciot, null, "55566677788");

        Assert.NotNull(result);
        Assert.Equal("55566677788",   result.Documento);
        Assert.Equal("ROBERTO FARIA", result.Nome);
        Assert.Equal("03456789",      result.Rntrc);
        Assert.True(result.RntrcAtivo);
        Assert.Single(result.MeiosPagamento);
        Assert.Equal("Conta", result.MeiosPagamento[0].Tipo);
    }

    [Fact]
    public void Merge_SoPamcard_RetornaFavorecidoPamcard()
    {
        var pamcard = new FavorecidoDto
        {
            Documento     = "11122233344",
            Nome          = "Maria Oliveira",
            Rntrc         = "05678901",
            RntrcSituacao = "Ativo",
            MeiosPagamento = [new MeioPagamentoDto("Conta", "Bradesco", "12345")],
        };

        var result = FavorecidoMerger.Merge(null, pamcard, "11122233344");

        Assert.NotNull(result);
        Assert.Equal("Maria Oliveira", result.Nome);
        Assert.Single(result.MeiosPagamento);
    }

    [Fact]
    public void Merge_PamcardEPriorizado_NomePamcardVence()
    {
        var ciot = new FindFavoredResponse
        {
            Nome          = "CARLOS MENDES",        // uppercase legado
            RntrcCadastro = "01234567",
            RntrcSituacao = "Ativo",
        };
        var pamcard = new FavorecidoDto
        {
            Documento     = "12345678901",
            Nome          = "Carlos Mendes",        // case correto Pamcard
            Rntrc         = "01234567",
            RntrcSituacao = "Ativo",
        };

        var result = FavorecidoMerger.Merge(ciot, pamcard, "12345678901");

        Assert.NotNull(result);
        Assert.Equal("Carlos Mendes", result.Nome); // Pamcard prevalece
    }

    [Fact]
    public void Merge_PamcardComLacuna_PreencheCiot()
    {
        var ciot = new FindFavoredResponse
        {
            Nome          = "DE BACKUP CIOT",
            RntrcCadastro = "99999999",
            RntrcSituacao = "Ativo",
        };
        var pamcard = new FavorecidoDto
        {
            Documento     = "99988877766",
            Nome          = "",                       // Pamcard sem nome
            Rntrc         = "99999999",
            RntrcSituacao = "Ativo",
        };

        var result = FavorecidoMerger.Merge(ciot, pamcard, "99988877766");

        Assert.NotNull(result);
        Assert.Equal("DE BACKUP CIOT", result.Nome); // fallback CIOT preencheu lacuna
    }

    [Fact]
    public void Merge_PamcardSemMeios_UsaMeiosCiot()
    {
        var ciot = new FindFavoredResponse
        {
            Nome          = "X",
            RntrcCadastro = "1",
            RntrcSituacao = "Ativo",
            Cartoes       = [new CartaoResponse("4321111122223421", 1, 1)],
        };
        var pamcard = new FavorecidoDto
        {
            Documento      = "11111111111",
            Nome           = "X",
            Rntrc          = "1",
            RntrcSituacao  = "Ativo",
            MeiosPagamento = [], // sem meios
        };

        var result = FavorecidoMerger.Merge(ciot, pamcard, "11111111111");

        Assert.NotNull(result);
        Assert.Single(result.MeiosPagamento);
        Assert.Equal("Cartao", result.MeiosPagamento[0].Tipo);
        Assert.EndsWith("3421", result.MeiosPagamento[0].Descricao);
    }
}
