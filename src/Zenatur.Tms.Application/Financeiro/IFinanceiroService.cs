namespace Zenatur.Tms.Application.Financeiro;

public interface IFinanceiroService
{
    Task<IReadOnlyList<ParcelaFinanceiraItem>> ListarAsync(FiltroParcelas filtro, CancellationToken ct = default);
    Task<ResumoFinanceiro>                     ObterResumoAsync(CancellationToken ct = default);
    Task<bool>                                 MarcarPagaAsync(string parcelaId, CancellationToken ct = default);
}

public class FiltroParcelas
{
    public string? Status   { get; set; }       // "Pendente", "Paga", "Vencida"
    public DateTime? DataDe { get; set; }
    public DateTime? DataAte { get; set; }
    public string? Termo    { get; set; }       // busca por favorecido/protocolo
}

public class ParcelaFinanceiraItem
{
    public string   Id              { get; set; } = string.Empty;
    public string   Protocolo       { get; set; } = string.Empty;
    public string   Favorecido      { get; set; } = string.Empty;
    public int      NumeroParcela   { get; set; }
    public int      TotalParcelas   { get; set; }
    public decimal  Valor           { get; set; }
    public DateTime Vencimento      { get; set; }
    public DateTime? DataPagamento  { get; set; }
    public string   Status          { get; set; } = "Pendente"; // Pendente, Paga, Vencida
    public string?  MeioPagamento   { get; set; }

    public int DiasAteVencimento => (Vencimento.Date - DateTime.Today).Days;
    public bool Vencida           => Status == "Pendente" && Vencimento.Date < DateTime.Today;
}

public class ResumoFinanceiro
{
    public decimal TotalAPagar      { get; set; }
    public decimal TotalVencido     { get; set; }
    public decimal TotalProximos7d  { get; set; }
    public decimal PagoNoMes        { get; set; }
    public int     QtdPendentes     { get; set; }
    public int     QtdVencidas      { get; set; }
}
