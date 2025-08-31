using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ExchangeRateUpdater.Domain.Entities;

namespace ExchangeRateUpdater.Domain.Interfaces
{
    public interface IExchangeRateProvider
    {
        Task<IReadOnlyCollection<ExchangeRate>> GetExchangeRatesAsync(IEnumerable<Currency> currencies, CancellationToken cancellationToken = default);
    }
}
