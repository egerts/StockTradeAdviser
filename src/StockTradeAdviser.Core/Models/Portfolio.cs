using System.Text.Json.Serialization;

namespace StockTradeAdviser.Core.Models;

public class Portfolio
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
    
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [JsonPropertyName("holdings")]
    public List<Holding> Holdings { get; set; } = new();
    
    [JsonPropertyName("totalValue")]
    public decimal TotalValue => Holdings.Sum(h => h.CurrentValue);
    
    [JsonPropertyName("totalCost")]
    public decimal TotalCost => Holdings.Sum(h => h.TotalCost);
    
    [JsonPropertyName("totalGainLoss")]
    public decimal TotalGainLoss => TotalValue - TotalCost;
    
    [JsonPropertyName("totalGainLossPercentage")]
    public decimal TotalGainLossPercentage => TotalCost > 0 ? (TotalGainLoss / TotalCost) * 100 : 0;
}

public class Holding
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;
    
    [JsonPropertyName("assetType")]
    public AssetType AssetType { get; set; } = AssetType.Stock;
    
    [JsonPropertyName("quantity")]
    public decimal Quantity { get; set; }
    
    [JsonPropertyName("averageCostPrice")]
    public decimal AverageCostPrice { get; set; }
    
    [JsonPropertyName("currentPrice")]
    public decimal CurrentPrice { get; set; }
    
    [JsonPropertyName("lastUpdated")]
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    
    [JsonPropertyName("totalCost")]
    public decimal TotalCost => Quantity * AverageCostPrice;
    
    [JsonPropertyName("currentValue")]
    public decimal CurrentValue => Quantity * CurrentPrice;
    
    [JsonPropertyName("gainLoss")]
    public decimal GainLoss => CurrentValue - TotalCost;
    
    [JsonPropertyName("gainLossPercentage")]
    public decimal GainLossPercentage => TotalCost > 0 ? (GainLoss / TotalCost) * 100 : 0;
    
    [JsonPropertyName("transactions")]
    public List<Transaction> Transactions { get; set; } = new();
}

public enum AssetType
{
    Stock,
    Option,
    ETF,
    Bond,
    Crypto,
    Commodity
}
