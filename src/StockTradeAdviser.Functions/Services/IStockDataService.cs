using StockTradeAdviser.Core.Models;

namespace StockTradeAdviser.Functions.Services;

public interface IStockDataService
{
    Task<StockData?> FetchStockDataAsync(string symbol);
    Task ProcessStockDataAsync(StockData stockData);
    Task<List<StockData>> FetchMultipleStockDataAsync(List<string> symbols);
}
