using Microsoft.Extensions.Logging;
using StockTradeAdviser.Core.Models;

namespace StockTradeAdviser.Api.Services;

public class MockRecommendationService : IRecommendationService
{
    private readonly ILogger<MockRecommendationService> _logger;
    private readonly List<Recommendation> _mockRecommendations;

    public MockRecommendationService(ILogger<MockRecommendationService> logger)
    {
        _logger = logger;
        _mockRecommendations = new List<Recommendation>();
        InitializeSampleData();
    }

    private void InitializeSampleData()
    {
        var userId = "mock-user-id";
        
        _mockRecommendations.Add(new Recommendation
        {
            Id = "rec-1",
            UserId = userId,
            Symbol = "AAPL",
            Action = RecommendationAction.Buy,
            Confidence = 0.85m,
            TargetPrice = 195.50m,
            StopLoss = 165.00m,
            Reasoning = "Strong technical indicators with solid fundamentals. Recent earnings beat expectations and the company continues to show strong growth in services segment.",
            KeyFactors = new List<string> { "Strong technical indicators", "Solid fundamentals", "Earnings beat", "Services growth" },
            RiskLevel = RiskLevel.Medium,
            TimeHorizon = TimeHorizon.MediumTerm,
            TechnicalScore = 75m,
            FundamentalScore = 82m,
            SentimentScore = 78m,
            OverallScore = 78m,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            ValidUntil = DateTime.UtcNow.AddDays(5)
        });

        _mockRecommendations.Add(new Recommendation
        {
            Id = "rec-2",
            UserId = userId,
            Symbol = "MSFT",
            Action = RecommendationAction.StrongBuy,
            Confidence = 0.92m,
            TargetPrice = 425.00m,
            StopLoss = 350.00m,
            Reasoning = "Cloud computing growth and AI integration driving strong revenue. Azure growth accelerating and AI products showing strong adoption.",
            KeyFactors = new List<string> { "Azure growth", "AI integration", "Cloud dominance", "Strong margins" },
            RiskLevel = RiskLevel.Low,
            TimeHorizon = TimeHorizon.LongTerm,
            TechnicalScore = 88m,
            FundamentalScore = 90m,
            SentimentScore = 85m,
            OverallScore = 88m,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            ValidUntil = DateTime.UtcNow.AddDays(6)
        });

        _mockRecommendations.Add(new Recommendation
        {
            Id = "rec-3",
            UserId = userId,
            Symbol = "TSLA",
            Action = RecommendationAction.Sell,
            Confidence = 0.65m,
            TargetPrice = 180.00m,
            StopLoss = 220.00m,
            Reasoning = "Increasing competition in EV market and production challenges. Valuation appears stretched compared to traditional automakers.",
            KeyFactors = new List<string> { "EV competition", "Production challenges", "High valuation", "Market saturation" },
            RiskLevel = RiskLevel.High,
            TimeHorizon = TimeHorizon.ShortTerm,
            TechnicalScore = 35m,
            FundamentalScore = 42m,
            SentimentScore = 38m,
            OverallScore = 38m,
            CreatedAt = DateTime.UtcNow.AddDays(-3),
            ValidUntil = DateTime.UtcNow.AddDays(4)
        });
    }

    public async Task<User?> GetUserByAzureAdObjectIdAsync(string azureAdObjectId)
    {
        // Return mock user for testing
        return new User
        {
            Id = "mock-user-id",
            AzureAdObjectId = azureAdObjectId,
            Email = "test@example.com",
            DisplayName = "Test User",
            TradingStrategy = new TradingStrategy
            {
                InvestmentHorizon = InvestmentHorizon.MediumTerm,
                RiskTolerance = RiskTolerance.Medium,
                PreferredSectors = new List<string> { "Technology", "Healthcare" }
            }
        };
    }

    public async Task<Recommendation?> GetRecommendationAsync(string recommendationId, string userId)
    {
        await Task.Delay(100); // Simulate async operation
        return _mockRecommendations.FirstOrDefault(r => r.Id == recommendationId && r.UserId == userId);
    }

    public async Task<List<Recommendation>> GetUserRecommendationsAsync(string userId, int limit = 50)
    {
        await Task.Delay(100); // Simulate async operation
        return _mockRecommendations
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Take(limit)
            .ToList();
    }

    public async Task<List<Recommendation>> GetActiveRecommendationsAsync(string userId)
    {
        await Task.Delay(100); // Simulate async operation
        return _mockRecommendations
            .Where(r => r.UserId == userId && r.Status == RecommendationStatus.Active && r.ValidUntil > DateTime.UtcNow)
            .OrderByDescending(r => r.CreatedAt)
            .ToList();
    }

    public async Task<List<Recommendation>> GenerateRecommendationsAsync(string userId)
    {
        await Task.Delay(500); // Simulate analysis time
        
        var newRecommendations = new List<Recommendation>();
        var symbols = new[] { "GOOGL", "NVDA", "AMZN", "META" };
        
        foreach (var symbol in symbols)
        {
            var existingRec = _mockRecommendations.FirstOrDefault(r => r.Symbol == symbol && r.UserId == userId);
            if (existingRec == null)
            {
                var recommendation = GenerateMockRecommendation(userId, symbol);
                _mockRecommendations.Add(recommendation);
                newRecommendations.Add(recommendation);
            }
        }

        return newRecommendations;
    }

    public async Task<Recommendation> GenerateRecommendationAsync(string userId, string symbol)
    {
        await Task.Delay(300); // Simulate analysis time
        
        var existingRec = _mockRecommendations.FirstOrDefault(r => r.Symbol == symbol && r.UserId == userId);
        if (existingRec != null)
        {
            return existingRec;
        }

        var recommendation = GenerateMockRecommendation(userId, symbol);
        _mockRecommendations.Add(recommendation);
        return recommendation;
    }

    public async Task<Recommendation> UpdateRecommendationAsync(Recommendation recommendation)
    {
        await Task.Delay(100); // Simulate async operation
        
        var existingRec = _mockRecommendations.FirstOrDefault(r => r.Id == recommendation.Id);
        if (existingRec != null)
        {
            existingRec.Action = recommendation.Action;
            existingRec.Confidence = recommendation.Confidence;
            existingRec.TargetPrice = recommendation.TargetPrice;
            existingRec.StopLoss = recommendation.StopLoss;
            existingRec.Reasoning = recommendation.Reasoning;
            existingRec.Status = recommendation.Status;
            existingRec.ActualAction = recommendation.ActualAction;
            existingRec.ActualPrice = recommendation.ActualPrice;
            existingRec.ExecutedAt = recommendation.ExecutedAt;
        }
        
        return existingRec ?? recommendation;
    }

    public async Task<List<RecommendationHistory>> GetRecommendationHistoryAsync(string userId, int limit = 100)
    {
        await Task.Delay(100); // Simulate async operation
        
        // Return mock history data
        return new List<RecommendationHistory>
        {
            new RecommendationHistory
            {
                Id = "hist-1",
                RecommendationId = "rec-old-1",
                UserId = userId,
                Symbol = "AAPL",
                OriginalAction = RecommendationAction.Buy,
                OriginalPrice = 150.00m,
                ActualAction = RecommendationAction.Buy,
                ActualPrice = 152.00m,
                Outcome = RecommendationOutcome.Profitable,
                ProfitLoss = 2.00m,
                ProfitLossPercentage = 1.33m,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                ClosedAt = DateTime.UtcNow.AddDays(-15)
            }
        };
    }

    public async Task UpdateRecommendationHistoryAsync(Recommendation recommendation, RecommendationOutcome outcome)
    {
        await Task.Delay(50); // Simulate async operation
        _logger.LogInformation("Updated recommendation history for {Symbol} with outcome {Outcome}", recommendation.Symbol, outcome);
    }

    private Recommendation GenerateMockRecommendation(string userId, string symbol)
    {
        var random = new Random();
        var baseScore = random.Next(30, 95);
        var action = baseScore switch
        {
            >= 80 => RecommendationAction.StrongBuy,
            >= 65 => RecommendationAction.Buy,
            >= 35 => RecommendationAction.Hold,
            >= 20 => RecommendationAction.Sell,
            _ => RecommendationAction.StrongSell
        };

        var currentPrice = (decimal)(random.Next(50, 500) + random.NextSingle());
        var targetPrice = currentPrice * (1 + (decimal)random.Next(-20, 30) / 100m);

        return new Recommendation
        {
            Id = $"rec-{Guid.NewGuid():N}",
            UserId = userId,
            Symbol = symbol,
            Action = action,
            Confidence = baseScore / 100m,
            TargetPrice = Math.Round(targetPrice, 2),
            StopLoss = Math.Round(currentPrice * 0.9m, 2),
            Reasoning = GenerateMockReasoning(action, baseScore),
            KeyFactors = GenerateMockKeyFactors(action, baseScore),
            RiskLevel = baseScore > 70 ? RiskLevel.Low : baseScore > 40 ? RiskLevel.Medium : RiskLevel.High,
            TimeHorizon = baseScore > 70 ? TimeHorizon.ShortTerm : TimeHorizon.MediumTerm,
            TechnicalScore = baseScore + (decimal)random.Next(-10, 10),
            FundamentalScore = baseScore + (decimal)random.Next(-10, 10),
            SentimentScore = baseScore + (decimal)random.Next(-5, 5),
            OverallScore = baseScore,
            CreatedAt = DateTime.UtcNow,
            ValidUntil = DateTime.UtcNow.AddDays(7)
        };
    }

    private string GenerateMockReasoning(RecommendationAction action, int score)
    {
        return action switch
        {
            RecommendationAction.StrongBuy => "Excellent technical and fundamental indicators suggest strong upside potential.",
            RecommendationAction.Buy => "Positive indicators with good risk-reward ratio for investment.",
            RecommendationAction.Hold => "Mixed signals suggest waiting for clearer directional momentum.",
            RecommendationAction.Sell => "Concerning indicators suggest potential downside risk.",
            RecommendationAction.StrongSell => "Multiple negative indicators indicate high risk of further decline.",
            _ => "Neutral stance with balanced risk and reward."
        };
    }

    private List<string> GenerateMockKeyFactors(RecommendationAction action, int score)
    {
        var factors = new List<string>();
        
        if (score > 70)
        {
            factors.AddRange(new[] { "Strong technical indicators", "Solid fundamentals", "Positive momentum" });
        }
        else if (score > 40)
        {
            factors.AddRange(new[] { "Moderate technical signals", "Stable fundamentals" });
        }
        else
        {
            factors.AddRange(new[] { "Weak technical indicators", "Concerning fundamentals" });
        }

        if (action == RecommendationAction.Buy || action == RecommendationAction.StrongBuy)
        {
            factors.Add("Growth potential");
        }
        else if (action == RecommendationAction.Sell || action == RecommendationAction.StrongSell)
        {
            factors.Add("Risk factors");
        }

        return factors;
    }
}
