using Zenatur.Tms.Application.Favorecidos;

namespace Zenatur.Tms.Infrastructure.Favorecidos;

internal sealed class InMemoryFavorecidoManagementService : IFavorecidoManagementService
{
    private readonly List<FavorecidoListItem> _db =
    [
        new()
        {
            Documento = "12345678901", DocumentoTipo = 1, Nome = "Carlos Mendes",
            Rntrc = "01234567", RntrcSituacao = "Ativo",
            Email = "carlos.mendes@email.com", TelefoneDdd = "11", TelefoneNumero = "987654321",
            CriadoEm = DateTime.UtcNow.AddDays(-90),
            Contas = [ new() { Banco = "Itaú",     Agencia = "1234", Conta = "56789-0", Tipo = "CC" } ]
        },
        new()
        {
            Documento = "98765432100", DocumentoTipo = 1, Nome = "Antônio Silva",
            Rntrc = "09876543", RntrcSituacao = "Inativo",
            Email = "antonio.silva@email.com", TelefoneDdd = "21", TelefoneNumero = "912345678",
            CriadoEm = DateTime.UtcNow.AddDays(-30),
            Contas = []
        },
        new()
        {
            Documento = "11122233344", DocumentoTipo = 1, Nome = "Maria Oliveira",
            Rntrc = "05678901", RntrcSituacao = "Ativo",
            Email = "maria@transportes.com.br", TelefoneDdd = "41", TelefoneNumero = "988776655",
            CriadoEm = DateTime.UtcNow.AddDays(-180),
            Contas = [ new() { Banco = "Bradesco", Agencia = "0001", Conta = "12345-6", Tipo = "CC", PixChave = "11122233344", PixTipo = "CPF" } ]
        },
        new()
        {
            Documento = "55566677788", DocumentoTipo = 1, Nome = "Roberto Faria",
            Rntrc = "03456789", RntrcSituacao = "Ativo",
            Email = "roberto.faria@email.com", TelefoneDdd = "31", TelefoneNumero = "977665544",
            CriadoEm = DateTime.UtcNow.AddDays(-10),
            Contas = [ new() { Banco = "Itaú", Agencia = "1234", Conta = "987654-3", Tipo = "CC" } ]
        },
    ];

    public Task<IReadOnlyList<FavorecidoListItem>> ListarAsync(string? termo = null, CancellationToken ct = default)
    {
        IEnumerable<FavorecidoListItem> q = _db;
        if (!string.IsNullOrWhiteSpace(termo))
        {
            var t = termo.Trim();
            q = q.Where(f => f.Nome.Contains(t, StringComparison.OrdinalIgnoreCase)
                          || f.Documento.Contains(t)
                          || f.Rntrc.Contains(t));
        }
        IReadOnlyList<FavorecidoListItem> result = q.OrderByDescending(f => f.CriadoEm).ToList();
        return Task.FromResult(result);
    }

    public Task<FavorecidoListItem?> ObterAsync(string documento, CancellationToken ct = default)
    {
        var doc = Limpar(documento);
        return Task.FromResult(_db.FirstOrDefault(f => Limpar(f.Documento) == doc));
    }

    public Task<FavorecidoListItem> SalvarAsync(FavorecidoListItem favorecido, CancellationToken ct = default)
    {
        var doc = Limpar(favorecido.Documento);
        favorecido.Documento = doc;

        var idx = _db.FindIndex(f => Limpar(f.Documento) == doc);
        if (idx >= 0)
        {
            _db[idx] = favorecido;
        }
        else
        {
            favorecido.CriadoEm = DateTime.UtcNow;
            _db.Add(favorecido);
        }

        return Task.FromResult(favorecido);
    }

    public Task<bool> RemoverAsync(string documento, CancellationToken ct = default)
    {
        var doc = Limpar(documento);
        var idx = _db.FindIndex(f => Limpar(f.Documento) == doc);
        if (idx < 0) return Task.FromResult(false);
        _db.RemoveAt(idx);
        return Task.FromResult(true);
    }

    private static string Limpar(string doc) =>
        doc.Replace(".", "").Replace("-", "").Replace("/", "").Trim();
}
