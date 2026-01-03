using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using StockTradeAdviser.Functions.Services;
using Azure.Messaging.ServiceBus;

namespace StockTradeAdviser.Functions.Functions;

public class StockDataIngestionTimerFunction
{
    private readonly ILogger<StockDataIngestionTimerFunction> _logger;
    private readonly IStockDataService _stockDataService;
    private readonly ServiceBusClient _serviceBusClient;

    public StockDataIngestionTimerFunction(
        ILogger<StockDataIngestionTimerFunction> logger,
        IStockDataService stockDataService,
        ServiceBusClient serviceBusClient)
    {
        _logger = logger;
        _stockDataService = stockDataService;
        _serviceBusClient = serviceBusClient;
    }

    [Function("StockDataIngestionTimer")]
    public async Task RunTimer([TimerTrigger("0 0 * * * *")] TimerInfo timer, FunctionContext context)
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

    private async Task<List<string>> GetWatchlistSymbols()
    {
        var symbols = new List<string>
        {
            "AAPL", "MSFT"
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
