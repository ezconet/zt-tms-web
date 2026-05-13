using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Zenatur.Tms.Application.Auth;

namespace Zenatur.Tms.Infrastructure.Security;

public sealed class TokenValidator : ITokenValidator
{
    private readonly IConfiguration _config;
    private readonly NonceCache     _nonces;

    public TokenValidator(IConfiguration config, NonceCache nonces)
    {
        _config = config;
        _nonces = nonces;
    }

    public ExternalLoginPayload? Validate(string token)
    {
        try
        {
            var keyBase64 = _config["TmsAuth:SharedKey"]
                ?? throw new InvalidOperationException("TmsAuth:SharedKey not configured.");

            var key  = Convert.FromBase64String(keyBase64);
            var json = AesCryptoHelper.Decrypt(token, key);

            var payload = JsonSerializer.Deserialize<ExternalLoginPayload>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (payload is null)                       return null;
            if (payload.ExpiresAt.ToUniversalTime() < DateTime.UtcNow) return null;
            if (!_nonces.TryConsume(payload.Nonce))   return null;

            return payload;
        }
        catch
        {
            return null;
        }
    }
}
