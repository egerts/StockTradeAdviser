using System.Text.Json.Serialization;

namespace StockTradeAdviser.Core.Models;

public class Transaction
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("holdingId")]
    public string HoldingId { get; set; } = string.Empty;
    
    [JsonPropertyName("portfolioId")]
    public string PortfolioId { get; set; } = string.Empty;
    
    [JsonPropertyName("type")]
    public TransactionType Type { get; set; }
    
    [JsonPropertyName("quantity")]
    public decimal Quantity { get; set; }
    
    [JsonPropertyName("price")]
    public decimal Price { get; set; }
    
    [JsonPropertyName("totalAmount")]
    public decimal TotalAmount { get; set; }
    
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    [JsonPropertyName("notes")]
    public string Notes { get; set; } = string.Empty;
    
    [JsonPropertyName("fees")]
    public decimal Fees { get; set; } = 0m;
    
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;
    
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum TransactionType
{
    Buy,
    Sell,
    Dividend,
    Split
}

public class CreateTransactionRequest
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    
    [JsonPropertyName("quantity")]
    public decimal Quantity { get; set; }
    
    [JsonPropertyName("price")]
    public decimal Price { get; set; }
    
    [JsonPropertyName("notes")]
    public string Notes { get; set; } = string.Empty;
    
    [JsonPropertyName("fees")]
    public decimal Fees { get; set; } = 0m;
    
    [JsonPropertyName("timestamp")]
    public DateTime? Timestamp { get; set; }
    
    // Helper method to convert string to enum
    public TransactionType GetTransactionType()
    {
        return Type.ToLower() switch
        {
            "buy" => TransactionType.Buy,
            "sell" => TransactionType.Sell,
            "dividend" => TransactionType.Dividend,
            "split" => TransactionType.Split,
            _ => TransactionType.Buy // Default fallback
        };
    }
}

public class UpdateTransactionRequest
{
    [JsonPropertyName("type")]
    public TransactionType? Type { get; set; }
    
    [JsonPropertyName("quantity")]
    public decimal? Quantity { get; set; }
    
    [JsonPropertyName("price")]
    public decimal? Price { get; set; }
    
    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
    
    [JsonPropertyName("fees")]
    public decimal? Fees { get; set; }
    
    [JsonPropertyName("timestamp")]
    public DateTime? Timestamp { get; set; }
}
