using ExchangeRateUpdater.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using Microsoft.Extensions.Caching.Distributed;

namespace ExchangeRateUpdater.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add Memory Cache
            services.AddMemoryCache();

            // Optional Redis distributed cache
            var redisEnabled = configuration.GetValue<bool>("Redis:Enabled", false);
            if (redisEnabled)
            {
                var redisConfig = configuration["Redis:Configuration"] ?? "localhost:6379";
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConfig;
                });
            }

            // Configure HttpClient for CnbExchangeRateProvider with Polly for resilience
            services.AddHttpClient<CnbExchangeRateProvider>(client =>
            {
                client.BaseAddress = new Uri(configuration["ExchangeRateProvider:CnbExchangeRateUrl"] ?? "https://www.cnb.cz");
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy());

            // Register CachingExchangeRateProvider as IExchangeRateProvider
            services.AddScoped<IExchangeRateProvider>(provider =>
            {
                var cnbProvider = provider.GetRequiredService<CnbExchangeRateProvider>();
                var cacheExpirationMinutes = configuration.GetValue<int>("ExchangeRateProvider:CacheExpirationMinutes", 60);

                if (redisEnabled)
                {
                    var distributed = provider.GetRequiredService<IDistributedCache>();
                    var distLogger = provider.GetRequiredService<ILogger<DistributedCachingExchangeRateProvider>>();
                    return new DistributedCachingExchangeRateProvider(cnbProvider, distributed, distLogger, cacheExpirationMinutes);
                }
                else
                {
                    // Fallback to the non-cached provider if Redis is not enabled
                    return cnbProvider;
                }
            });

            return services;
        }

        static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        var reason = outcome.Result != null ? outcome.Result.StatusCode.ToString() : outcome.Exception?.Message;
                        Console.WriteLine($"Retrying due to {reason}. Delaying for {timespan.TotalSeconds} seconds. Retry attempt {retryAttempt}.");
                    });
        }

        static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (outcome, breakDelay) =>
                    {
                        var reason = outcome.Result != null ? outcome.Result.StatusCode.ToString() : outcome.Exception?.Message;
                        Console.WriteLine($"Circuit breaking for {breakDelay.TotalSeconds} seconds due to {reason}.");
                    },
                    onReset: () => Console.WriteLine("Circuit breaker reset."),
                    onHalfOpen: () => Console.WriteLine("Circuit breaker half-open: attempting next call."));
        }
    }
}
