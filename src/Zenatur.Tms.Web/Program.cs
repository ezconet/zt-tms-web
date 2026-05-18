using Microsoft.AspNetCore.Authentication.Cookies;
using MudBlazor;
using MudBlazor.Services;
using Zenatur.Tms.Application.State;
using Zenatur.Tms.Infrastructure.DependencyInjection;
using Zenatur.Tms.Web.Auth;
using Zenatur.Tms.Web.Components;
using Zenatur.Tms.Web.Localization;

var builder = WebApplication.CreateBuilder(args);

// DetailedErrors: dev sempre; prod só se DetailedErrors=true (diagnóstico
// homolog — "circuit failed to initialize" sem detalhe).
var detailedErrors = builder.Environment.IsDevelopment()
    || builder.Configuration.GetValue("DetailedErrors", false);
builder.Services.AddRazorComponents(o => o.DetailedErrors = detailedErrors)
    .AddInteractiveServerComponents(o => o.DetailedErrors = detailedErrors);

builder.Services.AddMudServices();
builder.Services.AddTransient<MudLocalizer, PtBrMudLocalizer>();
builder.Services.AddScoped<CiotStateContainer>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // LoginPath: rota pública AllowAnonymous. NÃO pode ser /auth/external-login
        // (que é POST-only do legado) — loop infinito.
        // Apresentação: se DevAutoLogin on, qualquer rota não autenticada
        // cai no login silencioso (sem token/DevTools). Off = fluxo normal.
        var loginPath = DevLoginEndpoint.Enabled(builder.Configuration)
            ? DevLoginEndpoint.Path
            : "/auth/required";
        options.LoginPath         = loginPath;
        options.AccessDeniedPath  = loginPath;
        options.ExpireTimeSpan    = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;

        // Cookie Path raiz: sob sub-caminho o default vira "/TMSWeb" e o
        // browser (case-sensitive em path) não reenvia o cookie ao acessar
        // "/tmsweb" → loop de login. "/" resolve independente de caixa.
        options.Cookie.Path     = "/";
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

        // Anti-loop: challenge a partir de /Error ou /auth/* não acumula
        // ReturnUrl (estourava o query string → 404.15).
        options.Events.OnRedirectToLogin = ctx =>
        {
            var path = ctx.Request.Path.Value ?? string.Empty;
            if (path.Contains("/Error", StringComparison.OrdinalIgnoreCase) ||
                path.Contains("/auth/", StringComparison.OrdinalIgnoreCase))
                // sem ReturnUrl: login puro, com prefixo PathBase
                ctx.Response.Redirect(ctx.Request.PathBase + ctx.Options.LoginPath);
            else
                ctx.Response.Redirect(ctx.RedirectUri);
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization(o =>
{
    // Padrão: toda página/endpoint exige autenticação. Exceções via [AllowAnonymous].
    o.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddTmsInfrastructure(builder.Configuration);

var app = builder.Build();

// Publicado sob sub-caminho no IIS (ex.: /TMSWeb). Sem isto roteamento e
// assets só funcionam na raiz. Ver também <base href> em App.razor.
var pathBase = builder.Configuration["PathBase"];
if (!string.IsNullOrWhiteSpace(pathBase))
    app.UsePathBase(pathBase);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapPost("/auth/external-login", ExternalLoginEndpoint.Handle)
   .DisableAntiforgery()
   .AllowAnonymous();

// Login silencioso de apresentação (gated por Auth:DevAutoLogin).
app.MapGet(DevLoginEndpoint.Path, DevLoginEndpoint.Handle)
   .AllowAnonymous();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
