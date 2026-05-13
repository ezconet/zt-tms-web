using System.Security.Cryptography;
using System.Text.Json;
using Zenatur.Tms.Infrastructure.Security;

namespace Zenatur.Tms.Tests.Security;

public class AesCryptoHelperTests
{
    private static readonly byte[] _key = Convert.FromBase64String("/KgoXgcjUo56Kc/SieLNGImWi+dhpcBqtSxNbZK0JEA=");

    [Fact]
    public void RoundTrip_PayloadComplexo_DecodificaIgual()
    {
        var original = new TestPayload("user123", "user@email.com", "EXT_001", DateTime.UtcNow, Guid.NewGuid().ToString());

        var encrypted = AesCryptoHelper.Encrypt(original, _key);
        var decryptedJson = AesCryptoHelper.Decrypt(encrypted, _key);
        var decoded = JsonSerializer.Deserialize<TestPayload>(decryptedJson);

        Assert.NotNull(decoded);
        Assert.Equal(original.UserId,     decoded.UserId);
        Assert.Equal(original.Email,      decoded.Email);
        Assert.Equal(original.ExternalId, decoded.ExternalId);
        Assert.Equal(original.Nonce,      decoded.Nonce);
    }

    [Fact]
    public void Decrypt_TokenTruncado_LancaCryptographicException()
    {
        // AES-CBC com PKCS7 sem AEAD detecta tamanho/padding inválido,
        // mas não detecta bit-flip no meio do cipher (limitação do modo).
        var payload   = new TestPayload("X", "x@y.com", "E", DateTime.UtcNow, Guid.NewGuid().ToString());
        var encrypted = AesCryptoHelper.Encrypt(payload, _key);
        var bytes     = Convert.FromBase64String(encrypted);

        // Remove o último bloco completo → padding fica inválido
        var truncado  = bytes.Take(bytes.Length - 16).ToArray();
        var tampered  = Convert.ToBase64String(truncado);

        Assert.Throws<CryptographicException>(() => AesCryptoHelper.Decrypt(tampered, _key));
    }

    [Fact]
    public void Decrypt_ChaveErrada_LancaCryptographicException()
    {
        var payload   = new TestPayload("X", "x@y.com", "E", DateTime.UtcNow, "N1");
        var encrypted = AesCryptoHelper.Encrypt(payload, _key);
        var keyErrada = new byte[32];
        RandomNumberGenerator.Fill(keyErrada);

        Assert.Throws<CryptographicException>(() => AesCryptoHelper.Decrypt(encrypted, keyErrada));
    }

    private record TestPayload(string UserId, string Email, string ExternalId, DateTime IssuedAt, string Nonce);
}
