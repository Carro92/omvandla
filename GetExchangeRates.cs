using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;  // Lägg till denna rad
using System.Net.Http;
using System.Threading.Tasks;

namespace Omvandla_app
{
    public static class GetExchangeRates
    {
        private static readonly HttpClient client = new ();

        [Function("GetExchangeRates")]
        public static async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req,
            FunctionContext context)
        {
            var log = context.GetLogger("GetExchangeRates");

            // Läs API-nyckel från miljövariabeln
            var apiKey = Environment.GetEnvironmentVariable("API_KEY");
            var url = $"https://api.exchangerate-api.com/v4/latest/USD?apikey={apiKey}";

            try
            {
                // Hämta data från API
                var response = await client.GetStringAsync(url);
                var exchangeRates = JsonConvert.DeserializeObject<ExchangeRateResponse>(response);

                // Kontrollera om exchangeRates eller Rates är null
                if (exchangeRates == null || exchangeRates.Rates == null)
                {
                    log.LogError("Exchange rates data is null.");
                    var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                    await errorResponse.WriteStringAsync("Exchange rates data is not available.");
                    return errorResponse;
                }

                // Lista de valutakoder som vi vill visa
                var popularCurrencies = new string[] { "USD", "EUR", "SEK", "GBP", "AUD", "JPY", "CHF", "CAD", "NOK", "DKK" };

                // Hämta endast de 10 mest populära valutorna
                var topRates = popularCurrencies.ToDictionary(currency => currency, currency => exchangeRates.Rates[currency]);

                // Logga resultatet (valutor och deras växelkurser)
                log.LogInformation("Exchange Rates: {ExchangeRates}", string.Join(", ", topRates.Select(kvp => $"{kvp.Key}: {kvp.Value}")));

                // Skicka tillbaka resultatet som JSON
                var responseMessageJson = req.CreateResponse();
                await responseMessageJson.WriteAsJsonAsync(topRates);
                return responseMessageJson;
            }
            catch (Exception ex)
            {
                // Hantera fel vid API-anrop
                log.LogError("Error fetching exchange rates: {ErrorMessage}", ex.Message);
                var responseMessage = req.CreateResponse();
                responseMessage.StatusCode = HttpStatusCode.InternalServerError;
                await responseMessage.WriteStringAsync($"Error fetching exchange rates: {ex.Message}");
                return responseMessage;
            }
        }
    }

    // Klass för att deserialisera API-responsen
    public class ExchangeRateResponse
    {
        public string Base { get; set; } = "";  // Default value to avoid null errors
        public Dictionary<string, decimal> Rates { get; set; } = new Dictionary<string, decimal>();  // Initialize as empty dictionary
    }
}
