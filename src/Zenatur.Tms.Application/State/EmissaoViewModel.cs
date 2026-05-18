namespace Zenatur.Tms.Application.State;

public class EmissaoViewModel
{
    // Fase 0 — Busca por Minuta (pré-preenchimento via LegacyBridge)
    public string? NumeroMinuta { get; set; }
    public decimal? ValorFreteLegado { get; set; }
    public List<PontoParadaViewModel> PontosParada { get; set; } = [];

    // Fase 1 — Identificação
    public string? ContratanteCnpj { get; set; } = "53717120000170"; // Zenatur — user pode trocar
    public string? FavorecidoDocumento { get; set; }
    public string? FavorecidoNome { get; set; }
    public string? FavorecidoRntrc { get; set; }

    // Fase 1 — split PJ: quando favorecido é CNPJ, motorista (condutor PF) é obrigatório
    public string? MotoristaCpf  { get; set; }
    public string? MotoristaNome { get; set; }
    public bool FavorecidoEhPj => (FavorecidoDocumento?.Length ?? 0) == 14;

    // Fase 2 — Logística e Veículo
    public string? TipoVeiculo { get; set; }
    public string? VeiculoPlaca { get; set; }
    public List<string> ReboquesPlacas { get; set; } = [];
    public string? TipoCarga { get; set; }
    public string? UnidadeMedida { get; set; }
    public string? OrigemIbge { get; set; }
    public string? OrigemDescricao { get; set; }
    public string? DestinoIbge { get; set; }
    public string? DestinoDescricao { get; set; }

    // Fase 3 — Valores e Parcelas
    public decimal ValorFrete         { get; set; }
    public decimal ValorPedagio       { get; set; }
    public DateTime? DataVencimento   { get; set; }
    public int ParcelasQuantidade     { get; set; } = 1;
    public string? MeioPagamento      { get; set; }
    public List<ParcelaViewModel> Parcelas { get; set; } = [];

    // Roteirização + Frete Mínimo ANTT (calculados na transição Fase 2→3)
    public decimal? DistanciaKm       { get; set; }
    public decimal? FreteMinimoAntt   { get; set; }
    public string   ContratacaoTipo   { get; set; } = "1"; // 1=Carga Lotação, 2=Veículo Automotor
    public bool     AltoDesempenho    { get; set; }

    // Documentos da viagem (mínimo 1 obrigatório no Pamcard)
    public List<DocumentoViagemViewModel> DocumentosViagem { get; set; } = [];

    // Fase Viagem
    public DateTime? DataPartida { get; set; } = DateTime.Today;

    // Impostos (INSS/IRRF/SEST-SENAT) calculados pela Pamcard no fechamento
    // — não digitados nem deduzidos aqui.
    public decimal ValorLiquido => ValorFrete + ValorPedagio;

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

public class DocumentoViagemViewModel
{
    public int     Tipo   { get; set; } = 6; // default NOTA FISCAL
    public string  Numero { get; set; } = string.Empty;
}

public class PontoParadaViewModel
{
    public int     Ordem      { get; set; }
    public string  Cidade     { get; set; } = string.Empty;
    public string  Uf         { get; set; } = string.Empty;
    public string? Ibge       { get; set; }
    public string? Logradouro { get; set; }
    public string? Numero     { get; set; }
    public string? Tipo       { get; set; }

    public string Descricao => $"{Cidade}/{Uf}" +
        (string.IsNullOrWhiteSpace(Logradouro) ? "" : $" — {Logradouro}{(string.IsNullOrWhiteSpace(Numero) ? "" : ", " + Numero)}");
}
