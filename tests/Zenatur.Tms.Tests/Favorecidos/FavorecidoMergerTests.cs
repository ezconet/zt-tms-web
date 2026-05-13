using Zenatur.Ciot.Client.Favorecidos;
using Zenatur.Tms.Application.Favorecidos;

namespace Zenatur.Tms.Tests.Favorecidos;

public class FavorecidoMergerTests
{
    [Fact]
    public void Merge_CiotNulo_RetornaNull()
    {
        Assert.Null(FavorecidoMerger.Merge(null, "12345678901"));
    }

    [Fact]
    public void Merge_ComCiot_RetornaFavorecidoComDadosCiot()
    {
        var ciot = new FindFavoredResponse
        {
            Nome           = "ROBERTO FARIA",
            RntrcCadastro  = "03456789",
            RntrcSituacao  = "Ativo",
            Contas         = [new ContaResponse("341", "1234", "5", "987654-3", "C/C", "Ativa", null, null, null)],
        };

        var result = FavorecidoMerger.Merge(ciot, "55566677788");

        Assert.NotNull(result);
        Assert.Equal("55566677788",   result.Documento);
        Assert.Equal("ROBERTO FARIA", result.Nome);
        Assert.Equal("03456789",      result.Rntrc);
        Assert.True(result.RntrcAtivo);
        Assert.Single(result.MeiosPagamento);
        Assert.Equal("Conta", result.MeiosPagamento[0].Tipo);
    }

    [Fact]
    public void Merge_ComCartao_MapeiaUltimos4Digitos()
    {
        var ciot = new FindFavoredResponse
        {
            Nome          = "X",
            RntrcCadastro = "1",
            RntrcSituacao = "Ativo",
            Cartoes       = [new CartaoResponse("4321111122223421", 1, 1)],
        };

        var result = FavorecidoMerger.Merge(ciot, "11111111111");

        Assert.NotNull(result);
        Assert.Single(result.MeiosPagamento);
        Assert.Equal("Cartao", result.MeiosPagamento[0].Tipo);
        Assert.EndsWith("3421", result.MeiosPagamento[0].Descricao);
    }

    [Fact]
    public void Merge_RntrcInativo_RntrcAtivoFalse()
    {
        var ciot = new FindFavoredResponse
        {
            Nome          = "ANTONIO SILVA",
            RntrcCadastro = "09876543",
            RntrcSituacao = "Inativo",
        };

        var result = FavorecidoMerger.Merge(ciot, "98765432100");

        Assert.NotNull(result);
        Assert.False(result.RntrcAtivo);
    }
}
