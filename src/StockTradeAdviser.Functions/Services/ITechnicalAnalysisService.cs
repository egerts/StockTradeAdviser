using StockTradeAdviser.Core.Models;

namespace StockTradeAdviser.Functions.Services;

public interface ITechnicalAnalysisService
{
    Task<TechnicalIndicators> CalculateTechnicalIndicatorsAsync(string symbol);
    Task<decimal> CalculateRSIAsync(List<decimal> prices, int period = 14);
    Task<(decimal macd, decimal signal, decimal histogram)> CalculateMACDAsync(List<decimal> prices);
    Task<decimal> CalculateSMAAsync(List<decimal> prices, int period);
    Task<decimal> CalculateEMAAsync(List<decimal> prices, int period);
    Task<(decimal upper, decimal middle, decimal lower)> CalculateBollingerBandsAsync(List<decimal> prices, int period = 20, decimal stdDev = 2m);
}
