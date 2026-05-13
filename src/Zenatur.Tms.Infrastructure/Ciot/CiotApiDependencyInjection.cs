using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace Zenatur.Tms.Infrastructure.Ciot;

public static class CiotApiDependencyInjection
{
    public static IServiceCollection AddCiotApiHttpClient(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.Configure<CiotApiOptions>(config.GetSection(CiotApiOptions.SectionName));
        services.AddTransient<ApiKeyDelegatingHandler>();

        services.AddHttpClient<CiotApiHttpClient>((sp, http) =>
            {
                var opts = config.GetSection(CiotApiOptions.SectionName).Get<CiotApiOptions>()
                           ?? new CiotApiOptions();
                if (!string.IsNullOrEmpty(opts.BaseUrl))
                    http.BaseAddress = new Uri(opts.BaseUrl);
                http.Timeout = TimeSpan.FromSeconds(opts.TimeoutSeconds);
            })
            .AddHttpMessageHandler<ApiKeyDelegatingHandler>()
            .AddPolicyHandler((sp, _) => BuildRetryPolicy(config))
            .AddPolicyHandler(BuildCircuitBreakerPolicy());

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> BuildRetryPolicy(IConfiguration config)
    {
        var retries = config.GetSection(CiotApiOptions.SectionName).Get<CiotApiOptions>()?.RetryCount ?? 3;

        return HttpPolicyExtensions
            .HandleTransientHttpError() // 5xx + 408 + HttpRequestException
            .WaitAndRetryAsync(
                retryCount: retries,
                sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(200 * Math.Pow(2, attempt)));
    }

    private static IAsyncPolicy<HttpResponseMessage> BuildCircuitBreakerPolicy() =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(handledEventsAllowedBeforeBreaking: 5, durationOfBreak: TimeSpan.FromSeconds(30));
}
