using ExchangeRateUpdater.Application.Queries;
using ExchangeRateUpdater.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ExchangeRateUpdater.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExchangeRatesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ExchangeRatesController> _logger;
        private readonly IConfiguration _configuration;

        public ExchangeRatesController(
            IMediator mediator,
            ILogger<ExchangeRatesController> logger,
            IConfiguration configuration)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Gets exchange rates for a specified list of currencies against CZK.
        /// </summary>
        /// <param name="currencyCodes">A comma-separated list of currency codes (e.g., USD,EUR,GBP).</param>
        /// <returns>A list of exchange rates against CZK.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExchangeRate>>> GetExchangeRates(
            [FromQuery] string currencyCodes)
        {
            // Skeleton implementation: return empty list
            return Ok(new List<ExchangeRate>());
        }
    }
}
