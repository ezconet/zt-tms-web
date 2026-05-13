using System.Text.Json.Serialization;

namespace Zenatur.Tms.Application.Auth;

public record ExternalLoginPayload(
    [property: JsonPropertyName("userId")]     string UserId,
    [property: JsonPropertyName("email")]      string Email,
    [property: JsonPropertyName("externalId")] string ExternalId,
    [property: JsonPropertyName("issuedAt")]   DateTime IssuedAt,
    [property: JsonPropertyName("expiresAt")]  DateTime ExpiresAt,
    [property: JsonPropertyName("nonce")]      string Nonce
);
