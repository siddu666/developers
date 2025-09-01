# Exchange Rate Provider

A .NET 8 service for fetching Czech National Bank (CNB) exchange rates with intelligent caching and monitoring. This service provides reliable, real-time exchange rate data with enterprise-grade features like distributed caching, circuit breakers, health monitoring, and comprehensive testing.

## Table of Contents

- [Prerequisites](#prerequisites)
- [Architecture](#architecture)
- [Features](#features)
- [Quick Start](#quick-start)
- [Configuration](#configuration)
- [API Reference](#api-reference)
- [Console Application](#console-application)
- [Troubleshooting](#troubleshooting)
- [Testing](#testing)
- [Docker Deployment](#docker-deployment)
- [Adding New Providers](#adding-new-providers)
- [Future Enhancements](#future-enhancements)

## Prerequisites

- .NET 8.0 SDK or later
- (Optional) Redis for distributed caching
- (Optional) Docker and Docker Compose for containerized deployment

## Architecture

This project follows **Clean Architecture** principles with clear separation of concerns. For detailed architectural diagrams, layer descriptions, and design decisions, refer to [`Architecture.md`](Architecture.md).

## Features

- **Real-time CNB Exchange Rates**: Fetches official rates from Czech National Bank API
- **Intelligent Caching Strategy**:
  - 5 minutes during CNB publication window (2:31-3:31 PM Prague time)
  - 1 hour on weekdays outside publication window
  - 12 hours on weekends (no new data published)
- **Distributed Caching**: Optional Redis support for multi-instance deployments
- **Resilience**: Circuit breaker and retry policies using Polly
- **Health Monitoring**: Comprehensive health checks for CNB API and Redis
- **Metrics**: Prometheus metrics for monitoring and alerting
- **Rate Limiting**: 100 requests per minute per IP address
- **Structured Logging**: Serilog integration with request/response logging
- **Comprehensive Testing**: 45+ unit and integration tests
- **Docker Support**: Multi-stage Dockerfiles for development and production

## Quick Start

### Running the API

1. **Clone and navigate to the project**:
   ```bash
   cd /path/to/CnbExchangeRateProvider
   ```

2. **Run the API**:
   ```bash
   dotnet run --project ExchangeRateProvider.Api
   ```

3. **Test the endpoint**:
   ```bash
   curl "http://localhost:5001/api/exchange-rates?currencyCodes=USD,EUR"
   ```

### Running the Console App

```bash
dotnet run --project ExchangeRateProvider.Console
```

### Running Tests

```bash
dotnet test
```

## Configuration

The application uses `appsettings.json` for configuration. Key settings:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ExchangeRateProvider": {
    "CnbExchangeRateUrl": "https://api.cnb.cz",
    "CacheExpirationMinutes": 60,
    "TimeoutSeconds": 30,
    "MaxCurrencies": 20
  },
  "Redis": {
    "Enabled": true,
    "Configuration": "localhost:6379",
    "InstanceName": "ExchangeRates"
  },
  "CnbCacheStrategy": {
    "PublicationWindowMinutes": 5,
    "WeekdayHours": 1,
    "WeekendHours": 12
  }
}
```

### Environment Variables

Override settings using environment variables with double underscores:
```bash
ExchangeRateProvider__CnbExchangeRateUrl=https://api.cnb.cz
Redis__Enabled=true
Redis__Configuration=redis:6379
```

## API Reference

### GET /api/exchange-rates

Retrieves exchange rates for specified currencies against CZK.

**Parameters:**
- `currencyCodes` (required): Comma-separated ISO 4217 currency codes (e.g., `USD,EUR,GBP`)

**Example Request:**
```bash
curl "http://localhost:5001/api/exchange-rates?currencyCodes=USD,EUR"
```

**Response:**
```json
[
  {
    "sourceCurrency": { "code": "USD" },
    "targetCurrency": { "code": "CZK" },
    "value": 22.5,
    "timestamp": "2024-01-15T14:30:00Z"
  },
  {
    "sourceCurrency": { "code": "EUR" },
    "targetCurrency": { "code": "CZK" },
    "value": 24.2,
    "timestamp": "2024-01-15T14:30:00Z"
  }
]
```

**Error Responses:**
- `400 Bad Request`: Invalid currency codes or too many currencies (max 20)
- `500 Internal Server Error`: Service unavailable or CNB API error

### Monitoring Endpoints

- `GET /health` - Health status (CNB API and Redis connectivity)
- `GET /metrics` - Prometheus metrics
- `GET /swagger` - API documentation

## Console Application

The console app demonstrates programmatic access to exchange rates:

```bash
dotnet run --project ExchangeRateProvider.Console
```

It fetches rates for a predefined set of currencies and displays them in the console.

## Testing

The project includes comprehensive test coverage with **45 test methods**:

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

**Test Categories:**
- **Unit Tests**: Business logic, validation, caching strategies
- **Integration Tests**: API endpoints, CNB API interaction
- **Infrastructure Tests**: Service registration, configuration
- **Provider Tests**: Exchange rate providers with mocking


## Troubleshooting

### Common Issues

**API returns 500 error:**
- Check CNB API availability: `curl https://api.cnb.cz/cnbapi/exrates/daily`
- Verify configuration in `appsettings.json`
- Check logs for detailed error messages

**Redis connection failed:**
- Ensure Redis is running: `docker ps | grep redis`
- Check Redis configuration in environment variables
- Verify network connectivity between containers

**Rate limiting triggered:**
- Reduce request frequency or implement exponential backoff
- Check rate limit configuration in `Program.cs`

### Logs

Logs are written to console and can be mounted in Docker:
```bash
# View logs
docker-compose logs api

# Follow logs
docker-compose logs -f api
```

### Health Checks

```bash
# Check API health
curl http://localhost:5001/health

# Check with Docker
docker-compose exec api curl http://localhost:8080/health
```

## Docker Deployment

### Development

```bash
# Start development environment with Redis
docker-compose -f docker-compose.yml -f docker-compose.dev.yml up

# API available at http://localhost:5001
# Redis available at localhost:6379
```

### Production

```bash
# Build and start production environment
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d

# API available at http://localhost:80
```

### Manual Docker Build

```bash
# Build API image
docker build -f ExchangeRateProvider.Api/Dockerfile -t exchange-rate-api .

# Run with Redis
docker run -d --name redis redis:7-alpine
docker run -p 5001:8080 --link redis:redis -e Redis__Configuration=redis:6379 exchange-rate-api
```

## Adding New Providers

To add a new exchange rate provider:

1. **Implement the interface**:
```csharp
public class NewProvider : IExchangeRateProvider
{
    public string Name => "NewProvider";
    public int Priority => 50; // Lower than CNB (100)

    public bool CanHandle(IEnumerable<Currency> currencies)
    {
        // Return true if this provider can handle the currencies
        return true;
    }

    public async Task<IReadOnlyCollection<ExchangeRate>> GetExchangeRatesAsync(
        IEnumerable<Currency> currencies,
        CancellationToken cancellationToken = default)
    {
        // Your implementation here
        // Return rates with CZK as target currency
    }
}
```

2. **Register the provider**:
```csharp
// In Infrastructure/ServiceCollectionExtensions.cs
services.AddScoped<NewProvider>();
services.AddScoped<IExchangeRateProvider, NewProvider>(provider =>
{
    var newProvider = provider.GetRequiredService<NewProvider>();
    // Add caching if needed
    return newProvider;
});
```


## Future Enhancements If we wish to consider

### Phase 1: Production Readiness

- **API Versioning**: Add version headers for backward compatibility
- **Basic Authentication**: Implement API key authentication for production use
- **Database Storage**: Add PostgreSQL for caching historical rates
- **Health Checks**: Enhanced monitoring endpoints for production deployment
- **Configuration Management**: Environment-specific configuration handling

### Phase 2: Enhanced Features

- **Multiple Providers** 
- **Rate History**: Store and retrieve historical exchange rate data
- **Bulk Operations**: Support for converting multiple currency pairs at once
- **WebSocket Updates**: Real-time rate change notifications
- **Better Caching**: Improved cache invalidation and refresh strategies

### Phase 3: Scale & Performance

- **Horizontal Scaling**: Support for multiple API instances with load balancing
- **Performance Monitoring**: APM integration and detailed performance metrics
- **CDN Integration**: Global distribution for better response times
- **Advanced Security**: OAuth2 integration and enhanced security measures
- **Analytics Dashboard**: Usage statistics and business intelligence features
