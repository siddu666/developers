using System.Collections.Concurrent;
using System.Text.Json;
using ExchangeRateUpdater.Domain.Entities;
using ExchangeRateUpdater.Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace ExchangeRateUpdater.Infrastructure
{
    public class DistributedCachingExchangeRateProvider : IExchangeRateProvider
    {
        private readonly IExchangeRateProvider _innerProvider;
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<DistributedCachingExchangeRateProvider> _logger;
        private readonly int _cacheExpirationMinutes;

        public DistributedCachingExchangeRateProvider(
            IExchangeRateProvider innerProvider,
            IDistributedCache distributedCache,
            ILogger<DistributedCachingExchangeRateProvider> logger,
            int cacheExpirationMinutes)
        {
            _innerProvider = innerProvider ?? throw new ArgumentNullException(nameof(innerProvider));
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cacheExpirationMinutes = cacheExpirationMinutes;
        }

        public async Task<IReadOnlyCollection<ExchangeRate>> GetExchangeRatesAsync(
            IEnumerable<Currency> currencies,
            CancellationToken cancellationToken = default)
        {
            // Ensure currencies is not null and convert to a list
            var requestedCurrencies = currencies?.ToList() ?? new List<Currency>();
            if (!requestedCurrencies.Any())
            {
                _logger.LogWarning("No currencies requested.");
            }

            return new List<ExchangeRate>();
        }

    }
}
