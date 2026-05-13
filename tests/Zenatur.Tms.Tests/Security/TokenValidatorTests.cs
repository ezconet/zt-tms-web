using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Zenatur.Tms.Application.Auth;
using Zenatur.Tms.Infrastructure.Security;

namespace Zenatur.Tms.Tests.Security;

public class TokenValidatorTests
{
    private const string KEY = "/KgoXgcjUo56Kc/SieLNGImWi+dhpcBqtSxNbZK0JEA=";
    private static readonly byte[] _keyBytes = Convert.FromBase64String(KEY);

    private static TokenValidator BuildValidator(out NonceCache nonceCache)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["TmsAuth:SharedKey"] = KEY })
            .Build();
        nonceCache = new NonceCache(new MemoryCache(new MemoryCacheOptions()));
        return new TokenValidator(config, nonceCache);
    }

    private static ExternalLoginPayload NovoPayload(string nonce, DateTime? expiraEm = null) =>
        new(
            UserId:     "42",
            Email:      "joao@email.com",
            ExternalId: "EXT_42",
            IssuedAt:   DateTime.UtcNow,
            ExpiresAt:  expiraEm ?? DateTime.UtcNow.AddMinutes(5),
            Nonce:      nonce);

    [Fact]
    public void Validate_TokenValido_RetornaPayload()
    {
        var validator = BuildValidator(out _);
        var payload   = NovoPayload("n1");
        var token     = AesCryptoHelper.Encrypt(payload, _keyBytes);

        var result = validator.Validate(token);

        Assert.NotNull(result);
        Assert.Equal("42",            result.UserId);
        Assert.Equal("EXT_42",        result.ExternalId);
        Assert.Equal("joao@email.com", result.Email);
    }

    [Fact]
    public void Validate_TokenExpirado_RetornaNull()
    {
        var validator = BuildValidator(out _);
        var payload   = NovoPayload("n2", expiraEm: DateTime.UtcNow.AddMinutes(-1));
        var token     = AesCryptoHelper.Encrypt(payload, _keyBytes);

        Assert.Null(validator.Validate(token));
    }

    [Fact]
    public void Validate_TokenReusado_RetornaNullNaSegunda()
    {
        var validator = BuildValidator(out _);
        var payload   = NovoPayload("nonce-replay");
        var token     = AesCryptoHelper.Encrypt(payload, _keyBytes);

        Assert.NotNull(validator.Validate(token)); // primeira OK
        Assert.Null(validator.Validate(token));    // replay bloqueado
    }

    [Fact]
    public void Validate_TokenInvalido_RetornaNull()
    {
        var validator = BuildValidator(out _);
        Assert.Null(validator.Validate("não-é-um-token-base64-valido"));
    }

    [Fact]
    public void Validate_ChaveDiferente_RetornaNull()
    {
        var validator = BuildValidator(out _);
        var payload   = NovoPayload("n3");
        var outraKey  = new byte[32]; Random.Shared.NextBytes(outraKey);
        var token     = AesCryptoHelper.Encrypt(payload, outraKey);

        Assert.Null(validator.Validate(token));
    }
}
