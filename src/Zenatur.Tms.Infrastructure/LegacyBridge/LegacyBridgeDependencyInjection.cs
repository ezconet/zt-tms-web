using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Zenatur.Tms.Application.LegacyBridge;

namespace Zenatur.Tms.Infrastructure.LegacyBridge;

public static class LegacyBridgeDependencyInjection
{
    public static IServiceCollection AddLegacyBridge(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<LegacyBridgeOptions>(config.GetSection(LegacyBridgeOptions.SectionName));
        services.AddTransient<BridgeApiKeyHandler>();

        services.AddHttpClient<ILegacyBridgeClient, HttpLegacyBridgeClient>((sp, http) =>
            {
                var opts = config.GetSection(LegacyBridgeOptions.SectionName).Get<LegacyBridgeOptions>()
                           ?? new LegacyBridgeOptions();
                if (!string.IsNullOrEmpty(opts.BaseUrl))
                    http.BaseAddress = new Uri(opts.BaseUrl);
                http.Timeout = TimeSpan.FromSeconds(opts.TimeoutSeconds);
            })
            .AddHttpMessageHandler<BridgeApiKeyHandler>()
            .AddPolicyHandler(HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(2, attempt => TimeSpan.FromMilliseconds(200 * attempt)));

        return services;
    }
}
