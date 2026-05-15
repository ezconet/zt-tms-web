namespace Zenatur.Tms.Application.Motoristas;

public interface IMotoristaManagementService
{
    Task<IReadOnlyList<MotoristaListItem>> ListarAsync(string? termo = null, CancellationToken ct = default);
    Task<MotoristaListItem?>               ObterAsync(string cpf, CancellationToken ct = default);
    Task<MotoristaListItem>                SalvarAsync(MotoristaListItem motorista, bool exigirNovo = false, CancellationToken ct = default);
}

public class MotoristaListItem
{
    public int       Id             { get; set; }
    public string    Cpf            { get; set; } = "";
    public string    Nome           { get; set; } = "";
    public DateOnly? DataNascimento { get; set; }
    public string?   Email          { get; set; }
    public string?   Rntrc          { get; set; }
    public string?   RntrcSituacao  { get; set; }
    public DateOnly? RntrcValidade  { get; set; }
    public string?   TelefoneDdd    { get; set; }
    public string?   TelefoneNumero { get; set; }
    public string?   CnhNumero      { get; set; }
    public string?   CnhCategoria   { get; set; }
    public DateOnly? CnhValidade    { get; set; }
    public string?   RgNumero           { get; set; }
    public string?   RgUf               { get; set; }
    public string?   Logradouro         { get; set; }
    public int?      EnderecoNumero     { get; set; }
    public string?   Bairro             { get; set; }
    public string?   EnderecoCidade     { get; set; }
    public string?   EnderecoCidadeIbge { get; set; }
    public string?   EnderecoUf         { get; set; }
    public string?   Cep                { get; set; }
    public bool      Ativo          { get; set; } = true;
    public DateTime  AtualizadoEm   { get; set; }

    public MotoristaListItem Clone() => new()
    {
        Id = Id, Cpf = Cpf, Nome = Nome, DataNascimento = DataNascimento, Email = Email,
        Rntrc = Rntrc, RntrcSituacao = RntrcSituacao, RntrcValidade = RntrcValidade,
        TelefoneDdd = TelefoneDdd, TelefoneNumero = TelefoneNumero,
        CnhNumero = CnhNumero, CnhCategoria = CnhCategoria, CnhValidade = CnhValidade,
        RgNumero = RgNumero, RgUf = RgUf,
        Logradouro = Logradouro, EnderecoNumero = EnderecoNumero, Bairro = Bairro,
        EnderecoCidade = EnderecoCidade, EnderecoCidadeIbge = EnderecoCidadeIbge,
        EnderecoUf = EnderecoUf, Cep = Cep,
        Ativo = Ativo, AtualizadoEm = AtualizadoEm,
    };
}
