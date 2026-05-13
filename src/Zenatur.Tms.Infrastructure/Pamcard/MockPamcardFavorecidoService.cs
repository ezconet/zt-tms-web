using Zenatur.Tms.Application.Favorecidos;

namespace Zenatur.Tms.Infrastructure.Pamcard;

internal sealed class MockPamcardFavorecidoService : IPamcardFavorecidoService
{
    private static readonly Dictionary<string, FavorecidoDto> _db = new()
    {
        ["12345678901"] = new FavorecidoDto
        {
            Documento     = "12345678901",
            Nome          = "Carlos Mendes",
            Rntrc         = "01234567",
            RntrcSituacao = "Ativo",
            MeiosPagamento =
            [
                new("Cartao", "Cartão Pamcard *3421",           "3421"),
                new("Conta",  "Itaú Ag.1234 C/C 56789-0",      "56789-0"),
            ]
        },
        ["98765432100"] = new FavorecidoDto
        {
            Documento     = "98765432100",
            Nome          = "Antônio Silva",
            Rntrc         = "09876543",
            RntrcSituacao = "Inativo",
            MeiosPagamento = []
        },
        ["11122233344"] = new FavorecidoDto
        {
            Documento     = "11122233344",
            Nome          = "Maria Oliveira",
            Rntrc         = "05678901",
            RntrcSituacao = "Ativo",
            MeiosPagamento =
            [
                new("Conta", "Bradesco Ag.0001 C/C 12345-6", "12345-6"),
            ]
        },
    };

    public Task<FavorecidoDto?> FindAsync(string documento, CancellationToken ct = default)
    {
        var doc = documento.Replace(".", "").Replace("-", "").Replace("/", "").Trim();
        _db.TryGetValue(doc, out var result);
        return Task.FromResult(result);
    }
}
