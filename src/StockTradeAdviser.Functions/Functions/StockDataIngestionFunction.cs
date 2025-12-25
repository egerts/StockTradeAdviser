using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using StockTradeAdviser.Core.Models;
using StockTradeAdviser.Functions.Services;

namespace StockTradeAdviser.Functions.Functions;

public class StockDataIngestionFunction
{
    private readonly ILogger<StockDataIngestionFunction> _logger;
    private readonly IStockDataService _stockDataService;
    private readonly ServiceBusClient _serviceBusClient;

    public StockDataIngestionFunction(
        ILogger<StockDataIngestionFunction> logger,
        IStockDataService stockDataService,
        ServiceBusClient serviceBusClient)
    {
        _logger = logger;
        _stockDataService = stockDataService;
        _serviceBusClient = serviceBusClient;
    }

    [Function("StockDataIngestionTimer")]
    public async Task RunTimer([TimerTrigger("0 */5 * * * *")] TimerInfo timer, FunctionContext context)
    {
        _logger.LogInformation($"Stock data ingestion timer trigger function executed at: {DateTime.UtcNow}");

        try
        {
            var symbols = await GetWatchlistSymbols();
            await ProcessStockData(symbols);
            
            _logger.LogInformation($"Successfully processed {symbols.Count} stock symbols");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during stock data ingestion");
            throw;
        }
    }

    [Function("ProcessStockDataQueue")]
    public async Task ProcessQueueMessage(
        [ServiceBusTrigger("stock-data-queue", Connection = "ServiceBus:ConnectionString")] string message,
        FunctionContext context)
    {
        _logger.LogInformation($"Processing stock data queue message: {message}");

        try
        {
            var stockData = JsonSerializer.Deserialize<StockData>(message, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (stockData != null)
            {
                await _stockDataService.ProcessStockDataAsync(stockData);
                _logger.LogInformation($"Successfully processed stock data for {stockData.Symbol}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing queue message: {Message}", message);
            throw;
        }
    }

    private async Task<List<string>> GetWatchlistSymbols()
    {
        var symbols = new List<string>
        {
            "AAPL", "MSFT", "GOOGL", "AMZN", "TSLA", "META", "NVDA", "JPM", 
            "JNJ", "V", "PG", "UNH", "HD", "MA", "BAC", "XOM", "PFE", "CSCO",
            "ADBE", "CRM", "NFLX", "ACN", "NKE", "KO", "PEP", "T", "CVX",
            "ABT", "MRK", "DHR", "WMT", "MDT", "COST", "LIN", "TXN", "NEE",
            "AVGO", "LOW", "NOW", "QCOM", "UNP", "HON", "SBUX", "SCHW", "AMD",
            "INTC", "RTX", "AMGN", "BA", "CAT", "GE", "DIS", "VZ", "IBM", "GS"
        };

        return symbols;
    }

    private async Task ProcessStockData(List<string> symbols)
    {
        var sender = _serviceBusClient.CreateSender("stock-data-queue");
        var batchSize = 10;

        for (int i = 0; i < symbols.Count; i += batchSize)
        {
            var batch = symbols.Skip(i).Take(batchSize).ToList();
            var tasks = batch.Select(async symbol =>
            {
                try
                {
                    var stockData = await _stockDataService.FetchStockDataAsync(symbol);
                    if (stockData != null)
                    {
                        var message = JsonSerializer.Serialize(stockData);
                        var serviceBusMessage = new ServiceBusMessage(message);
                        await sender.SendMessageAsync(serviceBusMessage);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching stock data for symbol: {Symbol}", symbol);
                }
            });

            await Task.WhenAll(tasks);
            
            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        await sender.DisposeAsync();
    }
}
