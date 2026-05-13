using Microsoft.Extensions.Caching.Memory;

namespace Zenatur.Tms.Infrastructure.Security;

public sealed class NonceCache
{
    private readonly IMemoryCache _cache;

    public NonceCache(IMemoryCache cache) => _cache = cache;

    /// <summary>
    /// Retorna true e marca o nonce como usado.
    /// Retorna false se o nonce já foi utilizado nos últimos 10 minutos (replay attack).
    /// </summary>
    public bool TryConsume(string nonce)
    {
        if (_cache.TryGetValue(nonce, out _))
            return false;

        _cache.Set(nonce, true, TimeSpan.FromMinutes(10));
        return true;
    }
}
