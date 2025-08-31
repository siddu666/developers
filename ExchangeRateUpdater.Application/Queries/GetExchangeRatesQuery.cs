using ExchangeRateUpdater.Domain.Entities;
using MediatR;

namespace ExchangeRateUpdater.Application.Queries
{
    public class GetExchangeRatesQuery : IRequest<IEnumerable<ExchangeRate>>
    {
        public IEnumerable<Currency> Currencies { get; }
        public Currency TargetCurrency { get; }
        
        public GetExchangeRatesQuery(IEnumerable<Currency> currencies, Currency targetCurrency)
        {
            Currencies = currencies;
            TargetCurrency = targetCurrency;
        }
    }
}
