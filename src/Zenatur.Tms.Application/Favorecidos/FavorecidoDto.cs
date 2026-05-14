namespace Zenatur.Tms.Application.Favorecidos;

public record FavorecidoDto
{
    public string Documento     { get; init; } = string.Empty;
    public string Nome          { get; init; } = string.Empty;
    public string Rntrc         { get; init; } = string.Empty;
    public string RntrcSituacao { get; init; } = string.Empty;
    public string? TelefoneDdd    { get; init; }
    public string? TelefoneNumero { get; init; }

    public bool RntrcAtivo =>
        RntrcSituacao.Equals("Ativo", StringComparison.OrdinalIgnoreCase) ||
        RntrcSituacao.Equals("A",     StringComparison.OrdinalIgnoreCase);

    public IReadOnlyList<MeioPagamentoDto> MeiosPagamento { get; init; } = [];
}

public record MeioPagamentoDto(string Tipo, string Descricao, string Identificador);
