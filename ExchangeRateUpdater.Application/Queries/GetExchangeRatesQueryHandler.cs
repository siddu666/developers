using ExchangeRateUpdater.Domain.Entities;
using ExchangeRateUpdater.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ExchangeRateUpdater.Application.Queries
{
    public class GetExchangeRatesQueryHandler : IRequestHandler<GetExchangeRatesQuery, IEnumerable<ExchangeRate>>
    {
        private readonly IExchangeRateProvider _exchangeRateProvider;
        private readonly ILogger<GetExchangeRatesQueryHandler> _logger;

        public GetExchangeRatesQueryHandler(IExchangeRateProvider exchangeRateProvider, ILogger<GetExchangeRatesQueryHandler> logger)
        {
            _exchangeRateProvider = exchangeRateProvider;
            _logger = logger;
        }

        public async Task<IEnumerable<ExchangeRate>> Handle(GetExchangeRatesQuery request, CancellationToken cancellationToken)
        {
            // Skeleton implementation: return empty list
            return new List<ExchangeRate>();
        }
    }
}
