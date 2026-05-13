using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Zenatur.Tms.Infrastructure.Security;

public static class AesCryptoHelper
{
    /// <summary>Decripta token Base64(IV[16] + CipherText) com chave AES-256.</summary>
    public static string Decrypt(string token, byte[] key)
    {
        var data   = Convert.FromBase64String(token);
        var iv     = data[..16];
        var cipher = data[16..];

        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV  = iv;

        using var decryptor = aes.CreateDecryptor();
        var plain = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
        return Encoding.UTF8.GetString(plain);
    }

    /// <summary>Criptografa objeto como JSON em Base64(IV[16] + CipherText).</summary>
    public static string Encrypt<T>(T payload, byte[] key)
    {
        var json  = JsonSerializer.Serialize(payload);
        var plain = Encoding.UTF8.GetBytes(json);

        using var aes = Aes.Create();
        aes.Key = key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var cipher = encryptor.TransformFinalBlock(plain, 0, plain.Length);

        var result = new byte[aes.IV.Length + cipher.Length];
        aes.IV.CopyTo(result, 0);
        cipher.CopyTo(result, aes.IV.Length);

        return Convert.ToBase64String(result);
    }
}
