namespace Zenatur.Tms.Application.Favorecidos;

public interface IFavorecidoManagementService
{
    Task<IReadOnlyList<FavorecidoListItem>> ListarAsync(string? termo = null, CancellationToken ct = default);
    Task<FavorecidoListItem?>                ObterAsync(string documento, CancellationToken ct = default);
    Task<FavorecidoListItem>                 SalvarAsync(FavorecidoListItem favorecido, CancellationToken ct = default);
    Task<bool>                               RemoverAsync(string documento, CancellationToken ct = default);
}

public class FavorecidoListItem
{
    public string  Documento     { get; set; } = string.Empty;
    public int     DocumentoTipo { get; set; } = 1; // 1=CPF, 2=CNPJ
    public string  Nome          { get; set; } = string.Empty;
    public string  Rntrc         { get; set; } = string.Empty;
    public string  RntrcSituacao { get; set; } = "Ativo";
    public string? Email         { get; set; }
    public string? TelefoneDdd   { get; set; }
    public string? TelefoneNumero { get; set; }
    public DateTime CriadoEm     { get; set; } = DateTime.UtcNow;

    public List<ContaBancariaItem> Contas { get; set; } = [];

    public bool RntrcAtivo =>
        RntrcSituacao.Equals("Ativo", StringComparison.OrdinalIgnoreCase) ||
        RntrcSituacao.Equals("A",     StringComparison.OrdinalIgnoreCase);

    public FavorecidoListItem Clone() => new()
    {
        Documento      = Documento,
        DocumentoTipo  = DocumentoTipo,
        Nome           = Nome,
        Rntrc          = Rntrc,
        RntrcSituacao  = RntrcSituacao,
        Email          = Email,
        TelefoneDdd    = TelefoneDdd,
        TelefoneNumero = TelefoneNumero,
        CriadoEm       = CriadoEm,
        Contas         = Contas.Select(c => c.Clone()).ToList(),
    };
}

public class ContaBancariaItem
{
    public string  Banco    { get; set; } = string.Empty;
    public string  Agencia  { get; set; } = string.Empty;
    public string  Conta    { get; set; } = string.Empty;
    public string  Tipo     { get; set; } = "CC"; // CC=Corrente, CP=Poupança
    public string? PixChave { get; set; }
    public string? PixTipo  { get; set; }       // CPF, EMAIL, CELULAR, ALEATORIA

    public ContaBancariaItem Clone() => new()
    {
        Banco = Banco, Agencia = Agencia, Conta = Conta, Tipo = Tipo,
        PixChave = PixChave, PixTipo = PixTipo,
    };
}
