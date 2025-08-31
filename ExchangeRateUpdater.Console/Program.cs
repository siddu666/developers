using ExchangeRateUpdater.Application;
using ExchangeRateUpdater.Application.Queries;
using ExchangeRateUpdater.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ExchangeRateUpdater.Infrastructure; // This will be used for extension method

namespace ExchangeRateUpdater.ConsoleApp
{
    public class Program
    {
        private static readonly IEnumerable<Currency> s_currencies = new[]
        {
            new Currency("USD"),
            new Currency("EUR"),
            new Currency("CZK"),
            new Currency("JPY"),
            new Currency("GBP"),
            new Currency("AUD"),
            new Currency("CAD"),
            new Currency("CHF"),
            new Currency("CNY"),
            new Currency("SEK"),
            new Currency("NZD"),
            new Currency("MXN"),
            new Currency("SGD"),
            new Currency("HKD"),
            new Currency("NOK"),
        };

        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    var mediator = services.GetRequiredService<IMediator>();

                    logger.LogInformation("Starting Exchange Rate Updater application.");

                    var exchangeRates = await mediator.Send(new GetExchangeRatesQuery(s_currencies, new Currency("CZK")));

                    Console.WriteLine("Successfully retrieved exchange rates:");
                    foreach (var rate in exchangeRates)
                    {
                        Console.WriteLine(rate.ToString());
                    }
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while updating exchange rates.");
                    Console.WriteLine($"\nAn error occurred: {ex.Message}");
                }
            }

            Console.WriteLine("\nPress any key to exit.");
            Console.ReadKey();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, configuration) =>
                {
                    configuration.Sources.Clear();
                    configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                    configuration.AddEnvironmentVariables();
                    if (args != null)
                    {
                        configuration.AddCommandLine(args);
                    }
                })
                .ConfigureServices((hostContext, services) =>
                {
                    // Add logging
                    services.AddLogging(configure => configure.AddConsole());

                    // Register application and infrastructure services
                    services.AddApplicationServices();
                    services.AddInfrastructureServices(hostContext.Configuration);
                });
    }
}
