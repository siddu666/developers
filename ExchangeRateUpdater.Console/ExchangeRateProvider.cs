using System.Xml.Linq;
using System.Xml; // Added for XmlException

public class ExchangeRateProvider
{
    private const string CnbExchangeRateUrl = "https://www.cnb.cz/en/financial-markets/foreign-exchange-market/central-bank-exchange-rate-fixing/central-bank-exchange-rate-fixing/daily.txt";
    private readonly HttpClient _httpClient;

    public ExchangeRateProvider()
    {
        _httpClient = new HttpClient();
    }

    public async Task<Dictionary<string, decimal>> GetExchangeRatesAsync()
    {
        var exchangeRates = new Dictionary<string, decimal>();

        try
        {
            var response = await _httpClient.GetStringAsync(CnbExchangeRateUrl);
            var document = XDocument.Parse(response);

            foreach (var currencyElement in document.Descendants("currency"))
            {
                var code = currencyElement.Attribute("code")?.Value;
                var rate = currencyElement.Element("rate")?.Value;
                var amount = currencyElement.Element("amount")?.Value;

                if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(rate) || string.IsNullOrEmpty(amount))
                {
                    Console.WriteLine($"Warning: Skipping currency element due to missing data: Code={code}, Rate={rate}, Amount={amount}");
                    continue;
                }

                if (decimal.TryParse(rate, out var rateValue) && int.TryParse(amount, out var amountValue))
                {
                    if (amountValue != 0)
                    {
                        exchangeRates[code] = rateValue / amountValue;
                    }
                    else
                    {
                        Console.WriteLine($"Warning: Skipping currency '{code}' due to zero amount value.");
                    }
                }
                else
                {
                    Console.WriteLine($"Warning: Could not parse rate or amount for currency '{code}'. Rate: '{rate}', Amount: '{amount}'");
                }
            }
        }
        catch (HttpRequestException httpEx)
        {
            Console.WriteLine($"Error fetching exchange rates: {httpEx.Message}");
            throw new Exception("Failed to retrieve exchange rates due to network issues.", httpEx);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            throw;
        }

        return exchangeRates;
    }
}
