using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Zenatur.Tms.Application.Auth;

namespace Zenatur.Tms.Web.Auth;

/// <summary>
/// Login silencioso APENAS para apresentação/demo. Sem token, sem DevTools:
/// abrir o site já autentica um usuário fixo. Gated por config
/// "Auth:DevAutoLogin" (default false). Reverter = flag false.
/// </summary>
public static class DevLoginEndpoint
{
    public const string Path = "/auth/dev-login";

    public static bool Enabled(IConfiguration config) =>
        config.GetValue("Auth:DevAutoLogin", false);

    public static async Task<IResult> Handle(
        HttpContext  httpContext,
        IUserService userService,
        IConfiguration config)
    {
        if (!Enabled(config))
            return Results.Redirect("/auth/required");

        var email      = config.GetValue("Auth:DevUserEmail", "demo@zenatur.local")!;
        var externalId = config.GetValue("Auth:DevUserExternalId", "DEMO_USER")!;

        var payload = new ExternalLoginPayload(
            UserId:     externalId,
            Email:      email,
            ExternalId: externalId,
            IssuedAt:   DateTime.UtcNow,
            ExpiresAt:  DateTime.UtcNow.AddHours(12),
            Nonce:      Guid.NewGuid().ToString("N"));

        await userService.EnsureExistsAsync(payload);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, payload.UserId),
            new Claim(ClaimTypes.Email,          payload.Email),
            new Claim("ExternalId",              payload.ExternalId),
        };
        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        return Results.Redirect("/");
    }
}
