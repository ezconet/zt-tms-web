using Zenatur.Tms.Application.Financeiro;

namespace Zenatur.Tms.Infrastructure.Financeiro;

internal sealed class InMemoryFinanceiroService : IFinanceiroService
{
    private readonly List<ParcelaFinanceiraItem> _db;

    public InMemoryFinanceiroService()
    {
        var hoje = DateTime.Today;
        _db =
        [
            // CIOT-2026-00382 — Carlos Mendes, 3 parcelas, 1ª já paga, 2 a vencer
            new() { Id = "P001", Protocolo = "CIOT-2026-00382", Favorecido = "Carlos Mendes",
                    NumeroParcela = 1, TotalParcelas = 3, Valor = 1400m,
                    Vencimento = hoje.AddDays(-30), DataPagamento = hoje.AddDays(-28),
                    Status = "Paga", MeioPagamento = "Cartão Pamcard *3421" },
            new() { Id = "P002", Protocolo = "CIOT-2026-00382", Favorecido = "Carlos Mendes",
                    NumeroParcela = 2, TotalParcelas = 3, Valor = 1400m,
                    Vencimento = hoje.AddDays(2),
                    Status = "Pendente", MeioPagamento = "Cartão Pamcard *3421" },
            new() { Id = "P003", Protocolo = "CIOT-2026-00382", Favorecido = "Carlos Mendes",
                    NumeroParcela = 3, TotalParcelas = 3, Valor = 1400m,
                    Vencimento = hoje.AddDays(32),
                    Status = "Pendente", MeioPagamento = "Cartão Pamcard *3421" },

            // CIOT-2026-00381 — Antônio Silva, parcela única vencida
            new() { Id = "P004", Protocolo = "CIOT-2026-00381", Favorecido = "Antônio Silva",
                    NumeroParcela = 1, TotalParcelas = 1, Valor = 2850m,
                    Vencimento = hoje.AddDays(-5),
                    Status = "Pendente", MeioPagamento = "Itaú Ag.1234 C/C 56789-0" },

            // CIOT-2026-00380 — Maria Oliveira, 2 parcelas
            new() { Id = "P005", Protocolo = "CIOT-2026-00380", Favorecido = "Maria Oliveira",
                    NumeroParcela = 1, TotalParcelas = 2, Valor = 960m,
                    Vencimento = hoje.AddDays(-15), DataPagamento = hoje.AddDays(-14),
                    Status = "Paga", MeioPagamento = "Bradesco Ag.0001 C/C 12345-6" },
            new() { Id = "P006", Protocolo = "CIOT-2026-00380", Favorecido = "Maria Oliveira",
                    NumeroParcela = 2, TotalParcelas = 2, Valor = 960m,
                    Vencimento = hoje.AddDays(5),
                    Status = "Pendente", MeioPagamento = "Bradesco Ag.0001 C/C 12345-6" },

            // CIOT-2026-00378 — Fernanda Costa, paga
            new() { Id = "P007", Protocolo = "CIOT-2026-00378", Favorecido = "Fernanda Costa",
                    NumeroParcela = 1, TotalParcelas = 1, Valor = 5500m,
                    Vencimento = hoje.AddDays(-20), DataPagamento = hoje.AddDays(-20),
                    Status = "Paga", MeioPagamento = "Conta Própria" },

            // CIOT-2026-00377 — Paulo Rodrigues, pendente próxima
            new() { Id = "P008", Protocolo = "CIOT-2026-00377", Favorecido = "Paulo Rodrigues",
                    NumeroParcela = 1, TotalParcelas = 1, Valor = 3300m,
                    Vencimento = hoje.AddDays(7),
                    Status = "Pendente", MeioPagamento = "Itaú" },

            // CIOT-2026-00376 — Juliana Melo, parcela vencida pesada
            new() { Id = "P009", Protocolo = "CIOT-2026-00376", Favorecido = "Juliana Melo",
                    NumeroParcela = 1, TotalParcelas = 1, Valor = 7800m,
                    Vencimento = hoje.AddDays(-10),
                    Status = "Pendente", MeioPagamento = "Bradesco" },
        ];

        // Ajusta status Vencida automaticamente
        foreach (var p in _db)
            if (p.Status == "Pendente" && p.Vencimento.Date < DateTime.Today)
                p.Status = "Vencida";
    }

    public Task<IReadOnlyList<ParcelaFinanceiraItem>> ListarAsync(FiltroParcelas filtro, CancellationToken ct = default)
    {
        IEnumerable<ParcelaFinanceiraItem> q = _db;

        if (!string.IsNullOrEmpty(filtro.Status))
            q = q.Where(p => p.Status.Equals(filtro.Status, StringComparison.OrdinalIgnoreCase));

        if (filtro.DataDe is not null)
            q = q.Where(p => p.Vencimento.Date >= filtro.DataDe.Value.Date);

        if (filtro.DataAte is not null)
            q = q.Where(p => p.Vencimento.Date <= filtro.DataAte.Value.Date);

        if (!string.IsNullOrWhiteSpace(filtro.Termo))
        {
            var t = filtro.Termo.Trim();
            q = q.Where(p => p.Favorecido.Contains(t, StringComparison.OrdinalIgnoreCase)
                          || p.Protocolo.Contains(t, StringComparison.OrdinalIgnoreCase));
        }

        IReadOnlyList<ParcelaFinanceiraItem> result = q.OrderBy(p => p.Vencimento).ToList();
        return Task.FromResult(result);
    }

    public Task<ResumoFinanceiro> ObterResumoAsync(CancellationToken ct = default)
    {
        var hoje = DateTime.Today;
        var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);

        var resumo = new ResumoFinanceiro
        {
            TotalAPagar     = _db.Where(p => p.Status != "Paga").Sum(p => p.Valor),
            TotalVencido    = _db.Where(p => p.Status == "Vencida").Sum(p => p.Valor),
            TotalProximos7d = _db.Where(p => p.Status == "Pendente" && p.Vencimento.Date <= hoje.AddDays(7)).Sum(p => p.Valor),
            PagoNoMes       = _db.Where(p => p.Status == "Paga" && p.DataPagamento >= inicioMes).Sum(p => p.Valor),
            QtdPendentes    = _db.Count(p => p.Status == "Pendente"),
            QtdVencidas     = _db.Count(p => p.Status == "Vencida"),
        };
        return Task.FromResult(resumo);
    }

    public Task<bool> MarcarPagaAsync(string parcelaId, CancellationToken ct = default)
    {
        var p = _db.FirstOrDefault(x => x.Id == parcelaId);
        if (p is null || p.Status == "Paga") return Task.FromResult(false);
        p.Status = "Paga";
        p.DataPagamento = DateTime.Today;
        return Task.FromResult(true);
    }
}
