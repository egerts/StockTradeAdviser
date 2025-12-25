using System.Text.Json.Serialization;

namespace StockTradeAdviser.Core.Models;

public class Recommendation
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;
    
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;
    
    [JsonPropertyName("action")]
    public RecommendationAction Action { get; set; }
    
    [JsonPropertyName("confidence")]
    public decimal Confidence { get; set; }
    
    [JsonPropertyName("targetPrice")]
    public decimal TargetPrice { get; set; }
    
    [JsonPropertyName("stopLoss")]
    public decimal StopLoss { get; set; }
    
    [JsonPropertyName("reasoning")]
    public string Reasoning { get; set; } = string.Empty;
    
    [JsonPropertyName("keyFactors")]
    public List<string> KeyFactors { get; set; } = new();
    
    [JsonPropertyName("riskLevel")]
    public RiskLevel RiskLevel { get; set; }
    
    [JsonPropertyName("timeHorizon")]
    public TimeHorizon TimeHorizon { get; set; }
    
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [JsonPropertyName("validUntil")]
    public DateTime ValidUntil { get; set; } = DateTime.UtcNow.AddDays(7);
    
    [JsonPropertyName("status")]
    public RecommendationStatus Status { get; set; } = RecommendationStatus.Active;
    
    [JsonPropertyName("technicalScore")]
    public decimal TechnicalScore { get; set; }
    
    [JsonPropertyName("fundamentalScore")]
    public decimal FundamentalScore { get; set; }
    
    [JsonPropertyName("sentimentScore")]
    public decimal SentimentScore { get; set; }
    
    [JsonPropertyName("overallScore")]
    public decimal OverallScore { get; set; }
    
    [JsonPropertyName("actualAction")]
    public RecommendationAction? ActualAction { get; set; }
    
    [JsonPropertyName("actualPrice")]
    public decimal? ActualPrice { get; set; }
    
    [JsonPropertyName("executedAt")]
    public DateTime? ExecutedAt { get; set; }
}

public class RecommendationHistory
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("recommendationId")]
    public string RecommendationId { get; set; } = string.Empty;
    
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;
    
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;
    
    [JsonPropertyName("originalAction")]
    public RecommendationAction OriginalAction { get; set; }
    
    [JsonPropertyName("originalPrice")]
    public decimal OriginalPrice { get; set; }
    
    [JsonPropertyName("actualAction")]
    public RecommendationAction? ActualAction { get; set; }
    
    [JsonPropertyName("actualPrice")]
    public decimal? ActualPrice { get; set; }
    
    [JsonPropertyName("outcome")]
    public RecommendationOutcome? Outcome { get; set; }
    
    [JsonPropertyName("profitLoss")]
    public decimal? ProfitLoss { get; set; }
    
    [JsonPropertyName("profitLossPercentage")]
    public decimal? ProfitLossPercentage { get; set; }
    
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [JsonPropertyName("closedAt")]
    public DateTime? ClosedAt { get; set; }
}

public enum RecommendationAction
{
    StrongBuy,
    Buy,
    Hold,
    Sell,
    StrongSell
}

public enum RiskLevel
{
    Low,
    Medium,
    High,
    VeryHigh
}

public enum TimeHorizon
{
    ShortTerm,  // 1-4 weeks
    MediumTerm, // 1-6 months
    LongTerm    // 6+ months
}

public enum RecommendationStatus
{
    Active,
    Executed,
    Expired,
    Cancelled
}

public enum RecommendationOutcome
{
    Profitable,
    Loss,
    Breakeven
}
