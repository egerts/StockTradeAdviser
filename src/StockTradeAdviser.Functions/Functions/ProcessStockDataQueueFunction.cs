using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using StockTradeAdviser.Core.Models;
using StockTradeAdviser.Functions.Services;

namespace StockTradeAdviser.Functions.Functions;

public class ProcessStockDataQueueFunction
{
    private readonly ILogger<ProcessStockDataQueueFunction> _logger;
    private readonly IStockDataService _stockDataService;

    public ProcessStockDataQueueFunction(
        ILogger<ProcessStockDataQueueFunction> logger,
        IStockDataService stockDataService)
    {
        _logger = logger;
        _stockDataService = stockDataService;
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
}
