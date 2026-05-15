namespace Zenatur.Tms.Application.Veiculos;

public interface IVeiculoManagementService
{
    Task<IReadOnlyList<VeiculoListItem>> ListarAsync(string? termo = null, int? tipoTracao = null, CancellationToken ct = default);
    Task<VeiculoListItem?>               ObterAsync(string placa, CancellationToken ct = default);
    Task<VeiculoListItem>                SalvarAsync(VeiculoListItem veiculo, CancellationToken ct = default);
    Task<bool>                           RemoverAsync(string placa, CancellationToken ct = default);
}

public class VeiculoListItem
{
    public int       Id               { get; set; }
    public string    Placa            { get; set; } = "";
    public string?   Renavam          { get; set; }
    public string    CategoriaVeiculo { get; set; } = "";
    public int       TipoTracao       { get; set; } = 1; // 1=Tracionante 2=Reboque
    public int?      Eixos            { get; set; }
    public int?      AnoFabricacao    { get; set; }
    public int?      AnoModelo        { get; set; }
    public string?   Marca            { get; set; }
    public string?   Modelo           { get; set; }
    public decimal?  Tara             { get; set; }
    public decimal?  CapacidadeKg     { get; set; }
    public string?   Rntrc            { get; set; }
    public string?   RntrcSituacao    { get; set; }
    public DateOnly? RntrcValidade    { get; set; }
    public string?   ProprietarioFavorecidoDocumento { get; set; }
    public bool      Ativo            { get; set; } = true;
    public DateTime  AtualizadoEm     { get; set; }

    public VeiculoListItem Clone() => (VeiculoListItem)MemberwiseClone();
}
