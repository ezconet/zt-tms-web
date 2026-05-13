using Microsoft.Extensions.Caching.Memory;
using Zenatur.Tms.Infrastructure.Security;

namespace Zenatur.Tms.Tests.Security;

public class NonceCacheTests
{
    private static NonceCache Build() =>
        new(new MemoryCache(new MemoryCacheOptions()));

    [Fact]
    public void TryConsume_PrimeiraVez_RetornaTrue()
    {
        var cache = Build();
        Assert.True(cache.TryConsume("nonce-abc"));
    }

    [Fact]
    public void TryConsume_MesmoNonceDuasVezes_RetornaFalseNaSegunda()
    {
        var cache = Build();
        cache.TryConsume("nonce-xyz");

        Assert.False(cache.TryConsume("nonce-xyz"));
    }

    [Fact]
    public void TryConsume_NoncesDiferentes_AmbasRetornamTrue()
    {
        var cache = Build();
        Assert.True(cache.TryConsume("a"));
        Assert.True(cache.TryConsume("b"));
        Assert.True(cache.TryConsume("c"));
    }
}
