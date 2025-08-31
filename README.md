# CNB Exchange Rate Updater Solution

This solution provides a robust and production-grade system for fetching, caching, and exposing exchange rates from the Czech National Bank (CNB). It is built with .NET and adheres to Clean Architecture principles, ensuring scalability, maintainability, and testability.

## Architecture Overview

The solution is structured into multiple projects, following Clean Architecture to separate concerns:

*   **`ExchangeRateUpdater.Domain`**: Defines core domain models (`Currency`, `ExchangeRate`) and interfaces (`IExchangeRateProvider`). This project has no external dependencies.
*   **`ExchangeRateUpdater.Application`**: Contains application-specific business logic, including queries and handlers (using MediatR), and orchestrates operations. It depends on `ExchangeRateUpdater.Domain` and `ExchangeRateUpdater.Infrastructure`.
*   **`ExchangeRateUpdater.Infrastructure`**: Implements concrete services and external integrations, such as the `CnbExchangeRateProvider` for fetching data from CNB, and caching mechanisms. It depends on `ExchangeRateUpdater.Domain`. Resilience policies (retry, circuit breaker) using Polly are also configured here.
*   **`ExchangeRateUpdater.Console`**: A console application that serves as an entry point to demonstrate fetching and displaying exchange rates. It depends on `ExchangeRateUpdater.Application`.
*   **`ExchangeRateUpdater.Api`**: An ASP.NET Core Web API project that exposes the exchange rates via a REST endpoint with Swagger documentation. It depends on `ExchangeRateUpdater.Application`.
*   **`ExchangeRateUpdater.Tests`**: (To be implemented) A project for unit and integration tests.

## Data Source

The exchange rate data is sourced from the official CNB TEXT feed:
`https://www.cnb.cz/en/financial-markets/foreign-exchange-market/central-bank-exchange-rate-fixing/central-bank-exchange-rate-fixing/daily.txt`

## Features

*   **Clean Architecture**: Segregated concerns for better maintainability and testability.
*   **Dependency Injection**: Services are registered and resolved using the built-in .NET Core DI container.
*   **Robust Error Handling**: Comprehensive `try-catch` blocks for network issues, TEXT parsing errors, and invalid data.
*   **Resilience Policies**: Implemented using Polly for HTTP client retries and circuit breaking to handle transient faults and prevent cascading failures.
*   **Distributed Caching**: Exchange rates are cached to reduce external API calls, improve performance, and provide resilience.
*   **Structured Logging**: Integration of `Microsoft.Extensions.Logging` for configurable and detailed logging.
*   **Command Line Interface**: A console application for direct execution.
*   **RESTful API**: An ASP.NET Core API with Swagger UI for easy interaction and documentation.

## How to Build and Run

**Prerequisites:**
*   .NET SDK 8.0 or later

1.  **Navigate to the solution root directory:**
    ```bash
    cd CnbExchangeRateProvider
    ```

2.  **Add project references and install NuGet packages for all projects:**
    You will need to manually add the NuGet packages and project references as described in the individual project sections above.
    For example, to add `ExchangeRateUpdater.Console` to the solution and add its project references:
    ```bash
    dotnet new sln -n CnbExchangeRateProvider
    dotnet new console -n ExchangeRateUpdater.Console
    mv ExchangeRateUpdater.Console/CnbExchangeRateProviderApp.csproj ExchangeRateUpdater.Console/ExchangeRateUpdater.Console.csproj # If you started with a default project
    dotnet sln add ExchangeRateUpdater.Console/ExchangeRateUpdater.Console.csproj
    dotnet add ExchangeRateUpdater.Console/ExchangeRateUpdater.Console.csproj reference ExchangeRateUpdater.Application/ExchangeRateUpdater.Application.csproj
    # ... and similarly for all other projects and their references.
    ```

3.  **Restore dependencies for the entire solution:**
    ```bash
    dotnet restore
    ```

4.  **Build the entire solution:**
    ```bash
    dotnet build
    ```

### Running the Console Application

1.  **Navigate to the console project directory:**
    ```bash
    cd ExchangeRateUpdater.Console
    ```
2.  **Run the application:**
    ```bash
    dotnet run
    ```
    The application will output the obtained exchange rates to the console.

### Running the API Application

1.  **Navigate to the API project directory:**
    ```bash
    cd ExchangeRateUpdater.Api
    ```
2.  **Run the application:**
    ```bash
    dotnet run
    ```
    The API will start (typically on `http://localhost:5000` or `https://localhost:5001`).
    You can access the Swagger UI at `http://localhost:<port>/swagger` (e.g., `http://localhost:5000/swagger`).

## Production-Grade Considerations

The solution incorporates several production-grade considerations:

*   **Error Handling**: Detailed logging of exceptions with specific types (`HttpRequestException`, `XmlException`, `ApplicationException`) and general `Exception` catch-all.
*   **Resilience**: HTTP client with Polly policies for retries and circuit breakers, making it robust against transient network issues or API unavailability.
*   **Configuration**: Externalized settings (`CnbExchangeRateUrl`, `CacheExpirationMinutes`, `LogLevel`) in `appsettings.json` for easy management across environments.
*   **Logging**: Utilizes `Microsoft.Extensions.Logging` with console and debug providers, easily extendable to structured logging (e.g., Serilog, NLog) and various sinks (file, database, cloud).
*   **Caching**: Distributed caching with configurable expiration, reducing load on the external CNB API and speeding up responses.
*   **Dependency Injection**: Promotes loose coupling and testability by injecting dependencies (e.g., `HttpClient`, `ILogger`, `IMemoryCache`).

## Future Enhancements

*   **Unit and Integration Testing**: Implement comprehensive tests for all layers of the application.
*   **Monitoring**: Add health checks, metrics, and distributed tracing for better operational visibility.
*   **API Key/Authentication**: If the CNB API required it, implement secure authentication.
*   **Deployment**: Containerize applications using Docker for consistent and scalable deployments.
*   **Multiple Providers**: Extend the system to fetch exchange rates from other national banks or financial data providers.
*   **Date-Specific Rates**: Enhance providers to fetch historical or date-specific exchange rates if supported by the source.
*   **Custom Currency Handling**: Allow the client to specify the target currency (currently fixed to CZK).
