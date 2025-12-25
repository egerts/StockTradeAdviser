using Microsoft.Extensions.Logging;
using StockTradeAdviser.Core.Models;
using StockTradeAdviser.Data.Services;

namespace StockTradeAdviser.Api.Services;

public class RecommendationService : IRecommendationService
{
    private readonly ICosmosDbService _cosmosDbService;
    private readonly ILogger<RecommendationService> _logger;

    public RecommendationService(ICosmosDbService cosmosDbService, ILogger<RecommendationService> logger)
    {
        _cosmosDbService = cosmosDbService;
        _logger = logger;
    }

    public async Task<User?> GetUserByAzureAdObjectIdAsync(string azureAdObjectId)
    {
        try
        {
            return await _cosmosDbService.GetUserByAzureAdObjectIdAsync(azureAdObjectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by Azure AD object ID: {AzureAdObjectId}", azureAdObjectId);
            throw;
        }
    }

    public async Task<Recommendation?> GetRecommendationAsync(string recommendationId, string userId)
    {
        try
        {
            return await _cosmosDbService.GetRecommendationAsync(recommendationId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recommendation: {RecommendationId}", recommendationId);
            throw;
        }
    }

    public async Task<List<Recommendation>> GetUserRecommendationsAsync(string userId, int limit = 50)
    {
        try
        {
            return await _cosmosDbService.GetUserRecommendationsAsync(userId, limit);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user recommendations: {UserId}", userId);
            throw;
        }
    }

    public async Task<List<Recommendation>> GetActiveRecommendationsAsync(string userId)
    {
        try
        {
            return await _cosmosDbService.GetActiveRecommendationsAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active recommendations: {UserId}", userId);
            throw;
        }
    }

    public async Task<List<Recommendation>> GenerateRecommendationsAsync(string userId)
    {
        try
        {
            _logger.LogInformation("Generating recommendations for user: {UserId}", userId);

            var user = await _cosmosDbService.GetUserAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User not found: {UserId}", userId);
                return new List<Recommendation>();
            }

            var symbols = GetWatchlistSymbols(user);
            var recommendations = new List<Recommendation>();

            foreach (var symbol in symbols.Take(10))
            {
                try
                {
                    var recommendation = await GenerateRecommendationAsync(userId, symbol);
                    if (recommendation != null && recommendation.Confidence >= 0.6m)
                    {
                        recommendations.Add(recommendation);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error generating recommendation for symbol: {Symbol}", symbol);
                }
            }

            _logger.LogInformation("Generated {Count} recommendations for user: {UserId}", recommendations.Count, userId);
            return recommendations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating recommendations for user: {UserId}", userId);
            return new List<Recommendation>();
        }
    }

    public async Task<Recommendation> GenerateRecommendationAsync(string userId, string symbol)
    {
        try
        {
            _logger.LogInformation("Generating recommendation for {Symbol} for user: {UserId}", symbol, userId);

            var user = await _cosmosDbService.GetUserAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException($"User not found: {userId}");
            }

            var stockData = await _cosmosDbService.GetStockDataAsync(symbol);
            if (stockData == null)
            {
                _logger.LogWarning("Stock data not found for {Symbol}", symbol);
                return new Recommendation();
            }

            var score = AnalyzeStock(stockData);

            var recommendation = new Recommendation
            {
                UserId = userId,
                Symbol = symbol,
                Action = DetermineAction(score, user.TradingStrategy),
                Confidence = score,
                TargetPrice = CalculateTargetPrice(stockData.Price, score),
                StopLoss = CalculateStopLoss(stockData.Price, user.TradingStrategy.SellStrategy.StopLossPercentage),
                Reasoning = GenerateReasoning(score, stockData),
                KeyFactors = GenerateKeyFactors(score, stockData),
                RiskLevel = DetermineRiskLevel(score, stockData.Beta),
                TimeHorizon = DetermineTimeHorizon(user.TradingStrategy.InvestmentHorizon, score),
                TechnicalScore = CalculateTechnicalScore(stockData),
                FundamentalScore = CalculateFundamentalScore(stockData),
                SentimentScore = 50m,
                OverallScore = score
            };

            return await _cosmosDbService.CreateRecommendationAsync(recommendation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating recommendation for {Symbol}", symbol);
            throw;
        }
    }

    public async Task<Recommendation> UpdateRecommendationAsync(Recommendation recommendation)
    {
        try
        {
            return await _cosmosDbService.UpdateRecommendationAsync(recommendation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating recommendation: {RecommendationId}", recommendation.Id);
            throw;
        }
    }

    public async Task<List<RecommendationHistory>> GetRecommendationHistoryAsync(string userId, int limit = 100)
    {
        try
        {
            return await _cosmosDbService.GetRecommendationHistoryAsync(userId, limit);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recommendation history: {UserId}", userId);
            throw;
        }
    }

    public async Task UpdateRecommendationHistoryAsync(Recommendation recommendation, RecommendationOutcome outcome)
    {
        try
        {
            var history = new RecommendationHistory
            {
                RecommendationId = recommendation.Id,
                UserId = recommendation.UserId,
                Symbol = recommendation.Symbol,
                OriginalAction = recommendation.Action,
                OriginalPrice = recommendation.TargetPrice,
                ActualAction = recommendation.ActualAction,
                ActualPrice = recommendation.ActualPrice,
                Outcome = outcome,
                CreatedAt = recommendation.CreatedAt,
                ClosedAt = DateTime.UtcNow,
                ProfitLoss = recommendation.ActualPrice.HasValue && recommendation.TargetPrice > 0 
                    ? recommendation.ActualPrice.Value - recommendation.TargetPrice 
                    : null,
                ProfitLossPercentage = recommendation.ActualPrice.HasValue && recommendation.TargetPrice > 0
                    ? ((recommendation.ActualPrice.Value - recommendation.TargetPrice) / recommendation.TargetPrice) * 100
                    : null
            };

            await _cosmosDbService.CreateRecommendationHistoryAsync(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating recommendation history for {Symbol}", recommendation.Symbol);
        }
    }

    private List<string> GetWatchlistSymbols(User user)
    {
        var symbols = new List<string>
        {
            "AAPL", "MSFT", "GOOGL", "AMZN", "TSLA", "META", "NVDA", "JPM",
            "JNJ", "V", "PG", "UNH", "HD", "MA", "BAC", "XOM", "PFE", "CSCO"
        };

        if (user.TradingStrategy.PreferredSectors.Any())
        {
            var sectorSymbols = GetSymbolsBySector(user.TradingStrategy.PreferredSectors);
            symbols.AddRange(sectorSymbols);
        }

        return symbols.Distinct().Take(50).ToList();
    }

    private List<string> GetSymbolsBySector(List<string> sectors)
    {
        var sectorMap = new Dictionary<string, List<string>>
        {
            { "Technology", new List<string> { "AAPL", "MSFT", "GOOGL", "META", "NVDA", "ADBE", "CRM", "NFLX" } },
            { "Healthcare", new List<string> { "JNJ", "PFE", "UNH", "ABT", "MRK", "MDT" } },
            { "Finance", new List<string> { "JPM", "BAC", "WFC", "GS", "MS", "C", "AXP" } },
            { "Consumer", new List<string> { "AMZN", "HD", "MCD", "NKE", "SBUX", "LOW", "TGT" } },
            { "Energy", new List<string> { "XOM", "CVX", "COP", "EOG", "SLB" } }
        };

        var result = new List<string>();
        foreach (var sector in sectors)
        {
            if (sectorMap.TryGetValue(sector, out var symbols))
            {
                result.AddRange(symbols);
            }
        }

        return result;
    }

    private decimal AnalyzeStock(StockData stockData)
    {
        var technicalScore = CalculateTechnicalScore(stockData);
        var fundamentalScore = CalculateFundamentalScore(stockData);
        var sentimentScore = 50m;

        return Math.Round((technicalScore * 0.4m) + (fundamentalScore * 0.4m) + (sentimentScore * 0.2m), 2);
    }

    private decimal CalculateTechnicalScore(StockData stockData)
    {
        var score = 0m;
        var indicators = stockData.TechnicalIndicators;

        if (indicators.Rsi < 30) score += 20;
        else if (indicators.Rsi < 50) score += 10;
        else if (indicators.Rsi > 70) score -= 20;
        else if (indicators.Rsi > 60) score -= 10;

        if (stockData.Price > indicators.Sma20) score += 15;
        if (stockData.Price > indicators.Sma50) score += 15;
        if (stockData.Price > indicators.Sma200) score += 10;

        if (indicators.Macd > indicators.MacdSignal) score += 15;
        if (indicators.MacdHistogram > 0) score += 10;

        if (stockData.Price <= indicators.BollingerLower) score += 10;
        else if (stockData.Price >= indicators.BollingerUpper) score -= 10;

        return Math.Max(0, Math.Min(100, score));
    }

    private decimal CalculateFundamentalScore(StockData stockData)
    {
        var score = 0m;
        var fundamentals = stockData.Fundamentals;

        if (stockData.PeRatio > 0 && stockData.PeRatio < 15) score += 20;
        else if (stockData.PeRatio > 0 && stockData.PeRatio < 25) score += 10;

        if (fundamentals.RevenueGrowth > 0.15m) score += 15;
        else if (fundamentals.RevenueGrowth > 0.10m) score += 10;
        else if (fundamentals.RevenueGrowth > 0.05m) score += 5;

        if (fundamentals.ReturnOnEquity > 0.20m) score += 15;
        else if (fundamentals.ReturnOnEquity > 0.15m) score += 10;
        else if (fundamentals.ReturnOnEquity > 0.10m) score += 5;

        if (fundamentals.NetMargin > 0.20m) score += 15;
        else if (fundamentals.NetMargin > 0.15m) score += 10;
        else if (fundamentals.NetMargin > 0.10m) score += 5;

        if (fundamentals.DebtToEquity < 0.5m) score += 10;
        else if (fundamentals.DebtToEquity < 1.0m) score += 5;

        if (stockData.DividendYield > 0.03m) score += 10;
        else if (stockData.DividendYield > 0.02m) score += 5;

        return Math.Max(0, Math.Min(100, score));
    }

    private RecommendationAction DetermineAction(decimal score, TradingStrategy strategy)
    {
        return score switch
        {
            >= 80 => RecommendationAction.StrongBuy,
            >= 65 => RecommendationAction.Buy,
            >= 35 => RecommendationAction.Hold,
            >= 20 => RecommendationAction.Sell,
            _ => RecommendationAction.StrongSell
        };
    }

    private decimal CalculateTargetPrice(decimal currentPrice, decimal score)
    {
        var multiplier = score switch
        {
            >= 80 => 1.20m,
            >= 65 => 1.15m,
            >= 50 => 1.10m,
            >= 35 => 1.05m,
            _ => 0.95m
        };

        return Math.Round(currentPrice * multiplier, 2);
    }

    private decimal CalculateStopLoss(decimal currentPrice, decimal stopLossPercentage)
    {
        return Math.Round(currentPrice * (1 - stopLossPercentage / 100), 2);
    }

    private string GenerateReasoning(decimal score, StockData stockData)
    {
        var reasons = new List<string>();

        if (CalculateTechnicalScore(stockData) > 60)
        {
            reasons.Add("Strong technical indicators suggest upward momentum");
        }
        else if (CalculateTechnicalScore(stockData) < 40)
        {
            reasons.Add("Technical indicators indicate potential downside");
        }

        if (CalculateFundamentalScore(stockData) > 60)
        {
            reasons.Add("Solid fundamentals with strong financial metrics");
        }
        else if (CalculateFundamentalScore(stockData) < 40)
        {
            reasons.Add("Weak fundamentals raise concerns");
        }

        if (stockData.PriceChangePercentage > 2)
        {
            reasons.Add("Recent positive price movement supports bullish outlook");
        }
        else if (stockData.PriceChangePercentage < -2)
        {
            reasons.Add("Recent price decline may present buying opportunity");
        }

        return string.Join("; ", reasons);
    }

    private List<string> GenerateKeyFactors(decimal score, StockData stockData)
    {
        var factors = new List<string>();

        if (stockData.TechnicalIndicators.Rsi < 30)
            factors.Add("Oversold conditions (RSI)");
        if (stockData.TechnicalIndicators.Rsi > 70)
            factors.Add("Overbought conditions (RSI)");

        if (stockData.Price > stockData.TechnicalIndicators.Sma20)
            factors.Add("Price above 20-day SMA");
        if (stockData.Price > stockData.TechnicalIndicators.Sma50)
            factors.Add("Price above 50-day SMA");

        if (stockData.Fundamentals.RevenueGrowth > 0.10m)
            factors.Add("Strong revenue growth");
        if (stockData.Fundamentals.ReturnOnEquity > 0.15m)
            factors.Add("High return on equity");

        if (stockData.PeRatio < 20)
            factors.Add("Attractive P/E ratio");
        if (stockData.DividendYield > 0.02m)
            factors.Add("Dividend income potential");

        return factors;
    }

    private RiskLevel DetermineRiskLevel(decimal score, decimal beta)
    {
        if (beta > 1.5m || score < 30) return RiskLevel.VeryHigh;
        if (beta > 1.2m || score < 40) return RiskLevel.High;
        if (beta > 0.8m || score < 60) return RiskLevel.Medium;
        return RiskLevel.Low;
    }

    private TimeHorizon DetermineTimeHorizon(InvestmentHorizon userHorizon, decimal score)
    {
        if (score > 70) return TimeHorizon.ShortTerm;
        if (score > 50) return TimeHorizon.MediumTerm;
        return TimeHorizon.LongTerm;
    }
}
