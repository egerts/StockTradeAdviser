using Microsoft.AspNetCore.Mvc;
using StockTradeAdviser.Core.Models;

namespace StockTradeAdviser.Api.Controllers;

[ApiController]
[Route("api/stocks")]
// [Authorize] - Temporarily disabled for testing
public class StockController : ControllerBase
{
    private readonly ILogger<StockController> _logger;

    public StockController(ILogger<StockController> logger)
    {
        _logger = logger;
    }

    [HttpGet("market-overview")]
    public ActionResult<List<StockData>> GetMarketOverview()
    {
        try
        {
            // Return mock market data for major indices and stocks
            var marketData = new List<StockData>
            {
                new StockData
                {
                    Symbol = "SPY",
                    Price = 478.25m,
                    PriceChange = 2.15m,
                    PriceChangePercentage = 0.45m,
                    Volume = 75000000,
                    MarketCap = 45000000000m
                },
                new StockData
                {
                    Symbol = "QQQ",
                    Price = 412.80m,
                    PriceChange = -1.20m,
                    PriceChangePercentage = -0.29m,
                    Volume = 45000000,
                    MarketCap = 28000000000m
                },
                new StockData
                {
                    Symbol = "DIA",
                    Price = 378.90m,
                    PriceChange = 1.85m,
                    PriceChangePercentage = 0.49m,
                    Volume = 32000000,
                    MarketCap = 34000000000m
                },
                new StockData
                {
                    Symbol = "AAPL",
                    Price = 195.50m,
                    PriceChange = 3.25m,
                    PriceChangePercentage = 1.69m,
                    Volume = 85000000,
                    MarketCap = 3000000000000m
                },
                new StockData
                {
                    Symbol = "MSFT",
                    Price = 425.30m,
                    PriceChange = -2.10m,
                    PriceChangePercentage = -0.49m,
                    Volume = 22000000,
                    MarketCap = 3160000000000m
                },
                new StockData
                {
                    Symbol = "GOOGL",
                    Price = 145.80m,
                    PriceChange = 1.45m,
                    PriceChangePercentage = 1.00m,
                    Volume = 18000000,
                    MarketCap = 1830000000000m
                },
                new StockData
                {
                    Symbol = "TSLA",
                    Price = 242.60m,
                    PriceChange = -5.80m,
                    PriceChangePercentage = -2.34m,
                    Volume = 120000000,
                    MarketCap = 770000000000m
                },
                new StockData
                {
                    Symbol = "NVDA",
                    Price = 495.20m,
                    PriceChange = 8.75m,
                    PriceChangePercentage = 1.80m,
                    Volume = 45000000,
                    MarketCap = 1220000000000m
                }
            };

            return Ok(marketData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting market overview");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{symbol}")]
    public ActionResult<StockData> GetStockData(string symbol)
    {
        try
        {
            // Return mock stock data for any symbol
            var random = new Random();
            var basePrice = random.Next(50, 500);
            var change = (decimal)(random.NextDouble() * 20 - 10); // -10 to +10
            
            var stockData = new StockData
            {
                Symbol = symbol.ToUpper(),
                Price = basePrice + change,
                PriceChange = change,
                PriceChangePercentage = (change / basePrice) * 100,
                Volume = random.Next(1000000, 100000000),
                MarketCap = (decimal)(random.NextDouble() * 1000000000000)
            };

            return Ok(stockData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stock data for {Symbol}", symbol);
            return StatusCode(500, "Internal server error");
        }
    }
}
