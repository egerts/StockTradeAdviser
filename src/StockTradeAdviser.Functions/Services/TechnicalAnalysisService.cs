using Microsoft.Extensions.Logging;
using StockTradeAdviser.Core.Models;
using System.Text.Json;
using System.Net.Http.Headers;

namespace StockTradeAdviser.Functions.Services;

public class TechnicalAnalysisService : ITechnicalAnalysisService
{
    private readonly ILogger<TechnicalAnalysisService> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _alphaVantageApiKey;

    public TechnicalAnalysisService(
        ILogger<TechnicalAnalysisService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _httpClient = new HttpClient();
        _alphaVantageApiKey = configuration["AlphaVantage:ApiKey"] ?? throw new InvalidOperationException("Alpha Vantage API key is missing");
        
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "StockTradeAdviser/1.0");
    }

    public async Task<TechnicalIndicators> CalculateTechnicalIndicatorsAsync(string symbol)
    {
        try
        {
            _logger.LogInformation($"Calculating technical indicators for {symbol}");

            var dailyPrices = await FetchDailyPricesAsync(symbol);
            if (dailyPrices.Count < 200)
            {
                _logger.LogWarning($"Insufficient data for {symbol}. Only {dailyPrices.Count} data points available.");
                return new TechnicalIndicators();
            }

            var indicators = new TechnicalIndicators
            {
                Rsi = await CalculateRSIAsync(dailyPrices.TakeLast(14).ToList()),
                Sma20 = CalculateSMAAsync(dailyPrices.TakeLast(20).ToList(), 20),
                Sma50 = CalculateSMAAsync(dailyPrices.TakeLast(50).ToList(), 50),
                Sma200 = CalculateSMAAsync(dailyPrices.TakeLast(200).ToList(), 200),
                Ema12 = CalculateEMAAsync(dailyPrices.TakeLast(12).ToList(), 12),
                Ema26 = CalculateEMAAsync(dailyPrices.TakeLast(26).ToList(), 26)
            };

            var macdResult = await CalculateMACDAsync(dailyPrices.TakeLast(26).ToList());
            indicators.Macd = macdResult.macd;
            indicators.MacdSignal = macdResult.signal;
            indicators.MacdHistogram = macdResult.histogram;

            var bollingerResult = CalculateBollingerBandsAsync(dailyPrices.TakeLast(20).ToList());
            indicators.BollingerUpper = bollingerResult.upper;
            indicators.BollingerMiddle = bollingerResult.middle;
            indicators.BollingerLower = bollingerResult.lower;

            indicators.VolumeSma = CalculateSMAAsync(dailyPrices.TakeLast(20).ToList(), 20);

            return indicators;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error calculating technical indicators for {symbol}");
            return new TechnicalIndicators();
        }
    }

    public async Task<decimal> CalculateRSIAsync(List<decimal> prices, int period = 14)
    {
        if (prices.Count < period + 1)
            return 0;

        var gains = new List<decimal>();
        var losses = new List<decimal>();

        for (int i = 1; i < prices.Count; i++)
        {
            var change = prices[i] - prices[i - 1];
            gains.Add(change > 0 ? change : 0);
            losses.Add(change < 0 ? Math.Abs(change) : 0);
        }

        var avgGain = gains.Take(period).Average();
        var avgLoss = losses.Take(period).Average();

        if (avgLoss == 0)
            return 100;

        var rs = avgGain / avgLoss;
        var rsi = 100 - (100 / (1 + rs));

        return Math.Round(rsi, 2);
    }

    public async Task<(decimal macd, decimal signal, decimal histogram)> CalculateMACDAsync(List<decimal> prices)
    {
        if (prices.Count < 26)
            return (0, 0, 0);

        var ema12 = CalculateEMAAsync(prices.TakeLast(12).ToList(), 12);
        var ema26 = CalculateEMAAsync(prices.TakeLast(26).ToList(), 26);
        var macd = ema12 - ema26;

        var signalLine = CalculateEMAAsync(new List<decimal> { macd }, 9);
        var histogram = macd - signalLine;

        return (Math.Round(macd, 4), Math.Round(signalLine, 4), Math.Round(histogram, 4));
    }

    public decimal CalculateSMAAsync(List<decimal> prices, int period)
    {
        if (prices.Count < period)
            return 0;

        return Math.Round(prices.TakeLast(period).Average(), 2);
    }

    public decimal CalculateEMAAsync(List<decimal> prices, int period)
    {
        if (prices.Count == 0)
            return 0;

        var multiplier = 2m / (period + 1);
        var ema = prices[0];

        for (int i = 1; i < prices.Count; i++)
        {
            ema = (prices[i] * multiplier) + (ema * (1 - multiplier));
        }

        return Math.Round(ema, 2);
    }

    public (decimal upper, decimal middle, decimal lower) CalculateBollingerBandsAsync(List<decimal> prices, int period = 20, decimal stdDev = 2m)
    {
        if (prices.Count < period)
            return (0, 0, 0);

        var recentPrices = prices.TakeLast(period).ToList();
        var middle = recentPrices.Average();
        var variance = recentPrices.Select(x => Math.Pow((double)(x - middle), 2)).Average();
        var standardDeviation = (decimal)Math.Sqrt(variance);

        var upper = middle + (standardDeviation * stdDev);
        var lower = middle - (standardDeviation * stdDev);

        return (Math.Round(upper, 2), Math.Round(middle, 2), Math.Round(lower, 2));
    }

    private async Task<List<decimal>> FetchDailyPricesAsync(string symbol)
    {
        try
        {
            var url = $"https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol={symbol}&apikey={_alphaVantageApiKey}&outputsize=compact";
            var response = await _httpClient.GetStringAsync(url);
            var json = JsonDocument.Parse(response);

            if (json.RootElement.TryGetProperty("Time Series (Daily)", out var timeSeries))
            {
                var prices = new List<decimal>();
                foreach (var day in timeSeries.EnumerateObject())
                {
                    var closePrice = decimal.Parse(day.Value.GetProperty("4. close").GetString() ?? "0");
                    prices.Add(closePrice);
                }

                return prices.Reverse().ToList();
            }

            return new List<decimal>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching daily prices for {symbol}");
            return new List<decimal>();
        }
    }
}
