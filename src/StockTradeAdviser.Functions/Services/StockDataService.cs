using Microsoft.Extensions.Logging;
using StockTradeAdviser.Core.Models;
using StockTradeAdviser.Data.Services;
using System.Text.Json;
using System.Net.Http.Headers;

namespace StockTradeAdviser.Functions.Services;

public class StockDataService : IStockDataService
{
    private readonly ILogger<StockDataService> _logger;
    private readonly ICosmosDbService _cosmosDbService;
    private readonly ITechnicalAnalysisService _technicalAnalysisService;
    private readonly HttpClient _httpClient;
    private readonly string _alphaVantageApiKey;

    public StockDataService(
        ILogger<StockDataService> logger,
        ICosmosDbService cosmosDbService,
        ITechnicalAnalysisService technicalAnalysisService,
        IConfiguration configuration)
    {
        _logger = logger;
        _cosmosDbService = cosmosDbService;
        _technicalAnalysisService = technicalAnalysisService;
        _httpClient = new HttpClient();
        _alphaVantageApiKey = configuration["AlphaVantage:ApiKey"] ?? throw new InvalidOperationException("Alpha Vantage API key is missing");
        
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "StockTradeAdviser/1.0");
    }

    public async Task<StockData?> FetchStockDataAsync(string symbol)
    {
        try
        {
            _logger.LogInformation($"Fetching stock data for symbol: {symbol}");

            var quoteData = await FetchQuoteDataAsync(symbol);
            var fundamentalsData = await FetchFundamentalsDataAsync(symbol);
            var technicalData = await FetchTechnicalDataAsync(symbol);

            if (quoteData == null)
            {
                _logger.LogWarning($"No quote data found for symbol: {symbol}");
                return null;
            }

            var stockData = new StockData
            {
                Symbol = symbol.ToUpper(),
                CompanyName = quoteData.CompanyName ?? symbol,
                Price = quoteData.Price,
                PriceChange = quoteData.PriceChange,
                PriceChangePercentage = quoteData.PriceChangePercentage,
                Volume = quoteData.Volume,
                DayHigh = quoteData.DayHigh,
                DayLow = quoteData.DayLow,
                Week52High = quoteData.Week52High,
                Week52Low = quoteData.Week52Low,
                PeRatio = quoteData.PeRatio,
                DividendYield = quoteData.DividendYield,
                Beta = quoteData.Beta,
                Eps = quoteData.Eps,
                MarketCap = quoteData.MarketCap,
                AverageVolume = quoteData.AverageVolume,
                Sector = fundamentalsData?.Sector ?? "Unknown",
                Industry = fundamentalsData?.Industry ?? "Unknown",
                TechnicalIndicators = technicalData ?? new TechnicalIndicators(),
                Fundamentals = fundamentalsData?.Fundamentals ?? new Fundamentals()
            };

            return stockData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching stock data for symbol: {symbol}");
            return null;
        }
    }

    public async Task ProcessStockDataAsync(StockData stockData)
    {
        try
        {
            _logger.LogInformation($"Processing stock data for {stockData.Symbol}");

            await _cosmosDbService.UpdateStockDataAsync(stockData);

            _logger.LogInformation($"Successfully processed and stored stock data for {stockData.Symbol}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing stock data for {stockData.Symbol}");
            throw;
        }
    }

    public async Task<List<StockData>> FetchMultipleStockDataAsync(List<string> symbols)
    {
        var stockDataList = new List<StockData>();
        var batchSize = 5;

        for (int i = 0; i < symbols.Count; i += batchSize)
        {
            var batch = symbols.Skip(i).Take(batchSize).ToList();
            var tasks = batch.Select(FetchStockDataAsync);
            var results = await Task.WhenAll(tasks);
            stockDataList.AddRange(results.Where(data => data != null)!);

            if (i + batchSize < symbols.Count)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(200));
            }
        }

        return stockDataList;
    }

    private async Task<QuoteData?> FetchQuoteDataAsync(string symbol)
    {
        try
        {
            var url = $"https://www.alphavantage.co/query?function=GLOBAL_QUOTE&symbol={symbol}&apikey={_alphaVantageApiKey}";
            var response = await _httpClient.GetStringAsync(url);
            var json = JsonDocument.Parse(response);
            
            var quote = json.RootElement.GetProperty("Global Quote");
            
            return new QuoteData
            {
                Price = decimal.Parse(quote.GetProperty("05. price").GetString() ?? "0"),
                PriceChange = decimal.Parse(quote.GetProperty("09. change").GetString() ?? "0"),
                PriceChangePercentage = decimal.Parse(quote.GetProperty("10. change percent").GetString()?.Replace("%", "") ?? "0"),
                Volume = long.Parse(quote.GetProperty("06. volume").GetString() ?? "0"),
                DayHigh = decimal.Parse(quote.GetProperty("03. high").GetString() ?? "0"),
                DayLow = decimal.Parse(quote.GetProperty("04. low").GetString() ?? "0"),
                Week52High = 0,
                Week52Low = 0,
                PeRatio = 0,
                DividendYield = 0,
                Beta = 0,
                Eps = 0,
                MarketCap = 0,
                AverageVolume = 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching quote data for {symbol}");
            return null;
        }
    }

    private async Task<FundamentalsData?> FetchFundamentalsDataAsync(string symbol)
    {
        try
        {
            var url = $"https://www.alphavantage.co/query?function=OVERVIEW&symbol={symbol}&apikey={_alphaVantageApiKey}";
            var response = await _httpClient.GetStringAsync(url);
            var json = JsonDocument.Parse(response);
            
            var overview = json.RootElement;
            
            if (overview.GetProperty("Symbol").GetString() == null)
            {
                return null;
            }

            return new FundamentalsData
            {
                Sector = overview.GetProperty("Sector").GetString() ?? "Unknown",
                Industry = overview.GetProperty("Industry").GetString() ?? "Unknown",
                Fundamentals = new Fundamentals
                {
                    Revenue = decimal.Parse(overview.GetProperty("RevenueTTM").GetString() ?? "0"),
                    RevenueGrowth = decimal.Parse(overview.GetProperty("RevenuePerShareTTM").GetString() ?? "0"),
                    NetIncome = decimal.Parse(overview.GetProperty("ProfitMargin").GetString() ?? "0"),
                    GrossMargin = decimal.Parse(overview.GetProperty("GrossProfitTTM").GetString() ?? "0"),
                    OperatingMargin = decimal.Parse(overview.GetProperty("OperatingMarginTTM").GetString() ?? "0"),
                    NetMargin = decimal.Parse(overview.GetProperty("ProfitMargin").GetString() ?? "0"),
                    DebtToEquity = decimal.Parse(overview.GetProperty("DebtToEquityRatio").GetString() ?? "0"),
                    ReturnOnEquity = decimal.Parse(overview.GetProperty("ReturnOnEquityTTM").GetString() ?? "0"),
                    ReturnOnAssets = decimal.Parse(overview.GetProperty("ReturnOnAssetsTTM").GetString() ?? "0"),
                    CurrentRatio = decimal.Parse(overview.GetProperty("CurrentRatio").GetString() ?? "0"),
                    QuickRatio = decimal.Parse(overview.GetProperty("QuickRatio").GetString() ?? "0"),
                    BookValuePerShare = decimal.Parse(overview.GetProperty("BookValue").GetString() ?? "0"),
                    PriceToBook = decimal.Parse(overview.GetProperty("PriceToBookRatio").GetString() ?? "0"),
                    PriceToSales = decimal.Parse(overview.GetProperty("PriceToSalesRatioTTM").GetString() ?? "0")
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching fundamentals data for {symbol}");
            return null;
        }
    }

    private async Task<TechnicalIndicators?> FetchTechnicalDataAsync(string symbol)
    {
        try
        {
            return await _technicalAnalysisService.CalculateTechnicalIndicatorsAsync(symbol);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error calculating technical indicators for {symbol}");
            return new TechnicalIndicators();
        }
    }
}

internal class QuoteData
{
    public decimal Price { get; set; }
    public decimal PriceChange { get; set; }
    public decimal PriceChangePercentage { get; set; }
    public long Volume { get; set; }
    public decimal DayHigh { get; set; }
    public decimal DayLow { get; set; }
    public decimal Week52High { get; set; }
    public decimal Week52Low { get; set; }
    public decimal PeRatio { get; set; }
    public decimal DividendYield { get; set; }
    public decimal Beta { get; set; }
    public decimal Eps { get; set; }
    public decimal MarketCap { get; set; }
    public long AverageVolume { get; set; }
    public string? CompanyName { get; set; }
}

internal class FundamentalsData
{
    public string Sector { get; set; } = "Unknown";
    public string Industry { get; set; } = "Unknown";
    public Fundamentals Fundamentals { get; set; } = new();
}
