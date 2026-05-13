using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zenatur.Ciot.Client;
using Zenatur.Tms.Application.Auth;
using Zenatur.Tms.Application.Domain;
using Zenatur.Tms.Application.Favorecidos;
using Zenatur.Tms.Application.Financeiro;
using Zenatur.Tms.Infrastructure.Auth;
using Zenatur.Tms.Infrastructure.Ciot;
using Zenatur.Tms.Infrastructure.Domain;
using Zenatur.Tms.Infrastructure.Favorecidos;
using Zenatur.Tms.Infrastructure.Financeiro;
using Zenatur.Tms.Infrastructure.Pamcard;
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
        services.AddScoped<IPamcardFavorecidoService,  MockPamcardFavorecidoService>();
        services.AddSingleton<ICiotClient,             MockCiotClient>();
        services.AddSingleton<IFavorecidoManagementService, InMemoryFavorecidoManagementService>();
        services.AddSingleton<IFinanceiroService,           InMemoryFinanceiroService>();
        services.AddSingleton<IDomainService,               DomainCacheService>();

        services.AddDbContext<TmsDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("TmsDb")));

        // HTTP client tipado para CIOT API (Polly retry + circuit breaker + ApiKey)
        services.AddCiotApiHttpClient(config);

        return services;
    }
}
