using System.Text.Json.Serialization;

namespace StockTradeAdviser.Core.Models;

public class User
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;
    
    [JsonPropertyName("azureAdObjectId")]
    public string AzureAdObjectId { get; set; } = string.Empty;
    
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [JsonPropertyName("tradingStrategy")]
    public TradingStrategy TradingStrategy { get; set; } = new();
    
    [JsonPropertyName("portfolios")]
    public List<Portfolio> Portfolios { get; set; } = new();
}

public class TradingStrategy
{
    [JsonPropertyName("riskTolerance")]
    public RiskTolerance RiskTolerance { get; set; } = RiskTolerance.Medium;
    
    [JsonPropertyName("investmentHorizon")]
    public InvestmentHorizon InvestmentHorizon { get; set; } = InvestmentHorizon.MediumTerm;
    
    [JsonPropertyName("maxPortfolioSize")]
    public int MaxPortfolioSize { get; set; } = 20;
    
    [JsonPropertyName("preferredSectors")]
    public List<string> PreferredSectors { get; set; } = new();
    
    [JsonPropertyName("sellStrategy")]
    public SellStrategy SellStrategy { get; set; } = new();
}

public class SellStrategy
{
    [JsonPropertyName("takeProfitPercentage")]
    public decimal TakeProfitPercentage { get; set; } = 20m;
    
    [JsonPropertyName("stopLossPercentage")]
    public decimal StopLossPercentage { get; set; } = 10m;
    
    [JsonPropertyName("trailingStopEnabled")]
    public bool TrailingStopEnabled { get; set; } = false;
    
    [JsonPropertyName("trailingStopPercentage")]
    public decimal TrailingStopPercentage { get; set; } = 5m;
}

public enum RiskTolerance
{
    Low,
    Medium,
    High
}

public enum InvestmentHorizon
{
    ShortTerm,
    MediumTerm,
    LongTerm
}
