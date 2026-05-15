using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

// ─────────────────────────────────────────────────────────────────────────────
// Zenatur TMS — Token Generator (Dev / Handoff Tool)
//
// Uso:
//   dotnet run -- <email> <userId> <externalId>
//   dotnet run -- "joao@zenatur.com.br" "123" "USR_LEGACY_123"
//
// A chave abaixo é a MESMA de appsettings.Development.json > TmsAuth:SharedKey
// Em produção, trocar por variável de ambiente ou argumento --key <base64>
// ─────────────────────────────────────────────────────────────────────────────

const string DEV_KEY_BASE64 = "/KgoXgcjUo56Kc/SieLNGImWi+dhpcBqtSxNbZK0JEA=";

string email      = args.Length > 0 ? args[0] : "dev@zenatur.com.br";
string userId     = args.Length > 1 ? args[1] : "1";
string externalId = args.Length > 2 ? args[2] : "USR_LEGACY_1";

var payload = new TokenPayload(
    UserId:     userId,
    Email:      email,
    ExternalId: externalId,
    IssuedAt:   DateTime.UtcNow,
    ExpiresAt:  DateTime.UtcNow.AddMinutes(5),
    Nonce:      Guid.NewGuid().ToString()
);

var key   = Convert.FromBase64String(DEV_KEY_BASE64);
var token = Encrypt(payload, key);

Console.WriteLine("=== ZENATUR TMS — TOKEN GERADO ===");
Console.WriteLine($"Email:      {payload.Email}");
Console.WriteLine($"UserId:     {payload.UserId}");
Console.WriteLine($"ExternalId: {payload.ExternalId}");
Console.WriteLine($"Nonce:      {payload.Nonce}");
Console.WriteLine($"Expira:     {payload.ExpiresAt:u}");
Console.WriteLine();
Console.WriteLine("TOKEN RAW (base64 — NÃO url-encodar manualmente):");
Console.WriteLine(token);
Console.WriteLine();
Console.WriteLine("CURL de teste:");
Console.WriteLine($"curl -X POST https://localhost:7088/auth/external-login -d \"token={Uri.EscapeDataString(token)}\"");
Console.WriteLine();
Console.WriteLine("DEVTOOLS — cole no console (token RAW, encodeURIComponent encoda 1x):");
Console.WriteLine("fetch('/auth/external-login', {");
Console.WriteLine("  method: 'POST',");
Console.WriteLine("  headers: { 'Content-Type': 'application/x-www-form-urlencoded' },");
Console.WriteLine($"  body: 'token=' + encodeURIComponent('{token}'),");
Console.WriteLine("  redirect: 'follow',");
Console.WriteLine("  credentials: 'include'");
Console.WriteLine("}).then(r => location.href = '/');");

static string Encrypt<T>(T obj, byte[] key)
{
    var json  = JsonSerializer.Serialize(obj);
    var plain = Encoding.UTF8.GetBytes(json);

    using var aes = Aes.Create();
    aes.Key = key;
    aes.GenerateIV();

    using var enc    = aes.CreateEncryptor();
    var       cipher = enc.TransformFinalBlock(plain, 0, plain.Length);

    var result = new byte[aes.IV.Length + cipher.Length];
    aes.IV.CopyTo(result, 0);
    cipher.CopyTo(result, aes.IV.Length);

    return Convert.ToBase64String(result);
}

record TokenPayload(
    [property: JsonPropertyName("userId")]     string   UserId,
    [property: JsonPropertyName("email")]      string   Email,
    [property: JsonPropertyName("externalId")] string   ExternalId,
    [property: JsonPropertyName("issuedAt")]   DateTime IssuedAt,
    [property: JsonPropertyName("expiresAt")]  DateTime ExpiresAt,
    [property: JsonPropertyName("nonce")]      string   Nonce
);
