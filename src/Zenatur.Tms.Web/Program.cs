using Microsoft.AspNetCore.Authentication.Cookies;
using MudBlazor;
using MudBlazor.Services;
using Zenatur.Tms.Application.State;
using Zenatur.Tms.Infrastructure.DependencyInjection;
using Zenatur.Tms.Web.Auth;
using Zenatur.Tms.Web.Components;
using Zenatur.Tms.Web.Localization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents(o => o.DetailedErrors = builder.Environment.IsDevelopment())
    .AddInteractiveServerComponents(o => o.DetailedErrors = builder.Environment.IsDevelopment());

builder.Services.AddMudServices();
builder.Services.AddTransient<MudLocalizer, PtBrMudLocalizer>();
builder.Services.AddScoped<CiotStateContainer>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // LoginPath: rota pública AllowAnonymous. NÃO pode ser /auth/external-login
        // (que é POST-only do legado) — loop infinito.
        options.LoginPath         = "/auth/required";
        options.AccessDeniedPath  = "/auth/required";
        options.ExpireTimeSpan    = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
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

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
