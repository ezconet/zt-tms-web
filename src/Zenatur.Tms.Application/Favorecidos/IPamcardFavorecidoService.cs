namespace Zenatur.Tms.Application.Favorecidos;

public interface IPamcardFavorecidoService
{
    Task<FavorecidoDto?> FindAsync(string documento, CancellationToken ct = default);
}
