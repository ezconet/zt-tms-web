namespace Zenatur.Tms.Application.Domain;

public interface IDomainService
{
    Task<IReadOnlyList<DomainItem>>            GetTiposVeiculoAsync(CancellationToken ct = default);
    Task<IReadOnlyList<DomainItem>>            GetTiposCargaAsync(CancellationToken ct = default);
    Task<IReadOnlyList<UnidadeMedidaItem>>     GetUnidadesMedidaAsync(string tipoCargaCodigo, CancellationToken ct = default);
    Task<IReadOnlyList<CidadeIbgeItem>>        GetCidadesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<BancoItem>>             GetBancosAsync(CancellationToken ct = default);
    Task<IReadOnlyList<DomainItem>>            GetMeiosPagamentoAsync(CancellationToken ct = default);

    void InvalidarCache();
}

public record DomainItem(string Codigo, string Descricao);

public record UnidadeMedidaItem(string Codigo, string Descricao, string TipoCargaCodigo);

public record CidadeIbgeItem(string Ibge, string Cidade, string Uf)
{
    public string Descricao => $"{Cidade} / {Uf}";
}

public record BancoItem(string Codigo, string Nome, string? Ispb = null);
