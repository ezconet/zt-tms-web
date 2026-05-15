namespace Zenatur.Tms.Application.Composicoes;

public interface IComposicaoManagementService
{
    Task<IReadOnlyList<ComposicaoListItem>> ListarAsync(string? tracionantePlaca = null, CancellationToken ct = default);
    Task<ComposicaoListItem?>               ObterAsync(int id, CancellationToken ct = default);
    Task<ComposicaoListItem>                SalvarAsync(ComposicaoListItem composicao, CancellationToken ct = default);
    Task<bool>                              RemoverAsync(int id, CancellationToken ct = default);
}

public class ComposicaoListItem
{
    public int    Id                          { get; set; }
    public string Nome                        { get; set; } = "";
    public string TracionantePlaca            { get; set; } = "";
    public List<ReboqueItem> Reboques         { get; set; } = [];
    public string? FavorecidoTitularDocumento { get; set; }
    public bool   Ativo                       { get; set; } = true;
    public DateTime AtualizadoEm              { get; set; }
}

public class ReboqueItem
{
    public string Placa { get; set; } = "";
    public int    Ordem { get; set; }
}
