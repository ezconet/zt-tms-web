using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zenatur.Ciot.Client;
using Zenatur.Tms.Application.Auth;
using Zenatur.Tms.Application.Composicoes;
using Zenatur.Tms.Application.Domain;
using Zenatur.Tms.Application.Favorecidos;
using Zenatur.Tms.Application.Financeiro;
using Zenatur.Tms.Application.Motoristas;
using Zenatur.Tms.Application.Veiculos;
using Zenatur.Tms.Infrastructure.Auth;
using Zenatur.Tms.Infrastructure.Ciot;
using Zenatur.Tms.Infrastructure.Composicoes;
using Zenatur.Tms.Infrastructure.Domain;
using Zenatur.Tms.Infrastructure.Favorecidos;
using Zenatur.Tms.Infrastructure.Financeiro;
using Zenatur.Tms.Infrastructure.LegacyBridge;
using Zenatur.Tms.Infrastructure.Motoristas;
using Zenatur.Tms.Infrastructure.Veiculos;
using Zenatur.Tms.Infrastructure.Persistence;
using Zenatur.Tms.Infrastructure.Security;

namespace Zenatur.Tms.Infrastructure.DependencyInjection;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddTmsInfrastructure(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddMemoryCache();
        services.AddSingleton<NonceCache>();

        services.AddScoped<ITokenValidator,           TokenValidator>();
        services.AddScoped<IUserService,              TmsUserService>();

        // ─── Implementações HTTP reais (A8-A11) ───
        // CIOT API via HttpClient tipado + Polly + X-Api-Key. SDK fica de lado por enquanto.
        services.AddScoped<ICiotClient,             HttpCiotClient>();
        services.AddScoped<IFavorecidoManagementService, CiotApiFavorecidoService>();
        services.AddScoped<IMotoristaManagementService,  CiotApiMotoristaService>();
        services.AddScoped<IVeiculoManagementService,    CiotApiVeiculoService>();
        services.AddScoped<IComposicaoManagementService, CiotApiComposicaoService>();
        services.AddScoped<IFinanceiroService,      CiotApiFinanceiroService>();
        services.AddScoped<IDomainService,          CiotApiDomainService>();

        // IPamcardFavorecidoService removido em A13 — Pamcard é detalhe interno da CIOT API.
        // Segunda fonte do merge (LegacyBridge) entra em A14-A17.

        services.AddDbContext<TmsDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("TmsDb")));

        // HTTP client tipado para CIOT API (Polly retry + circuit breaker + ApiKey)
        services.AddCiotApiHttpClient(config);

        // HTTP client tipado para LegacyBridge (A14-A17)
        services.AddLegacyBridge(config);

        return services;
    }
}
