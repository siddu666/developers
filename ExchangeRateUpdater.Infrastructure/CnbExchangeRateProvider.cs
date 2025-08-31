using System.Globalization;
using ExchangeRateUpdater.Domain.Entities;
using Microsoft.Extensions.Logging;
using ExchangeRateUpdater.Domain.Interfaces;

namespace ExchangeRateUpdater.Infrastructure
{
    public class CnbExchangeRateProvider : IExchangeRateProvider
    {
        private const string CnbUrl = "https://api.cnb.cz/cnbapi/exrates/daily";
        private readonly HttpClient _httpClient;
        private readonly ILogger<CnbExchangeRateProvider> _logger;

        public CnbExchangeRateProvider(HttpClient httpClient, ILogger<CnbExchangeRateProvider> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IReadOnlyCollection<ExchangeRate>> GetExchangeRatesAsync(IEnumerable<Currency> requestedCurrencies, CancellationToken cancellationToken = default)
        {
            // Skeleton implementation: return empty list
            return new List<ExchangeRate>();
        }
    }
}
