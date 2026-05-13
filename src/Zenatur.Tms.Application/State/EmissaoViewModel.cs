namespace Zenatur.Tms.Application.State;

public class EmissaoViewModel
{
    // Fase 1 — Identificação
    public string? ContratanteCnpj { get; set; }
    public string? FavorecidoDocumento { get; set; }
    public string? FavorecidoNome { get; set; }
    public string? FavorecidoRntrc { get; set; }

    // Fase 2 — Logística e Veículo
    public string? TipoVeiculo { get; set; }
    public string? VeiculoPlaca { get; set; }
    public string? TipoCarga { get; set; }
    public string? UnidadeMedida { get; set; }
    public string? OrigemIbge { get; set; }
    public string? OrigemDescricao { get; set; }
    public string? DestinoIbge { get; set; }
    public string? DestinoDescricao { get; set; }

    // Fase 3 — Valores e Parcelas
    public decimal ValorFrete         { get; set; }
    public decimal ValorPedagio       { get; set; }
    public decimal ValorImpostos      { get; set; }
    public DateTime? DataVencimento   { get; set; }
    public int ParcelasQuantidade     { get; set; } = 1;
    public string? MeioPagamento      { get; set; }
    public List<ParcelaViewModel> Parcelas { get; set; } = [];

    public decimal ValorLiquido => ValorFrete + ValorPedagio - ValorImpostos;

    // Resultado (pós-emissão)
    public string? CiotNumero    { get; set; }
    public string? CiotProtocolo { get; set; }
}

public class ParcelaViewModel
{
    public int       Numero      { get; set; }
    public decimal   Valor       { get; set; }
    public DateTime  Vencimento  { get; set; }
}
