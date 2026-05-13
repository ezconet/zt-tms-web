using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Zenatur.Tms.Application.Auth;

namespace Zenatur.Tms.Web.Auth;

public static class ExternalLoginEndpoint
{
    public static async Task<IResult> Handle(
        HttpContext       httpContext,
        ITokenValidator   tokenValidator,
        IUserService      userService)
    {
        var form  = await httpContext.Request.ReadFormAsync();
        var token = form["token"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(token))
            return Results.Redirect("/auth/error?reason=missing_token");

        var payload = tokenValidator.Validate(token);
        if (payload is null)
            return Results.Redirect("/auth/error?reason=invalid_token");

        await userService.EnsureExistsAsync(payload);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, payload.UserId),
            new Claim(ClaimTypes.Email,          payload.Email),
            new Claim("ExternalId",              payload.ExternalId),
        };

        var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return Results.Redirect("/");
    }
}
