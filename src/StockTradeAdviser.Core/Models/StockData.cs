using System.Text.Json.Serialization;

namespace StockTradeAdviser.Core.Models;

public class StockData
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;
    
    [JsonPropertyName("companyName")]
    public string CompanyName { get; set; } = string.Empty;
    
    [JsonPropertyName("sector")]
    public string Sector { get; set; } = string.Empty;
    
    [JsonPropertyName("industry")]
    public string Industry { get; set; } = string.Empty;
    
    [JsonPropertyName("marketCap")]
    public decimal MarketCap { get; set; }
    
    [JsonPropertyName("price")]
    public decimal Price { get; set; }
    
    [JsonPropertyName("priceChange")]
    public decimal PriceChange { get; set; }
    
    [JsonPropertyName("priceChangePercentage")]
    public decimal PriceChangePercentage { get; set; }
    
    [JsonPropertyName("volume")]
    public long Volume { get; set; }
    
    [JsonPropertyName("averageVolume")]
    public long AverageVolume { get; set; }
    
    [JsonPropertyName("dayHigh")]
    public decimal DayHigh { get; set; }
    
    [JsonPropertyName("dayLow")]
    public decimal DayLow { get; set; }
    
    [JsonPropertyName("week52High")]
    public decimal Week52High { get; set; }
    
    [JsonPropertyName("week52Low")]
    public decimal Week52Low { get; set; }
    
    [JsonPropertyName("peRatio")]
    public decimal PeRatio { get; set; }
    
    [JsonPropertyName("dividendYield")]
    public decimal DividendYield { get; set; }
    
    [JsonPropertyName("beta")]
    public decimal Beta { get; set; }
    
    [JsonPropertyName("eps")]
    public decimal Eps { get; set; }
    
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    [JsonPropertyName("technicalIndicators")]
    public TechnicalIndicators TechnicalIndicators { get; set; } = new();
    
    [JsonPropertyName("fundamentals")]
    public Fundamentals Fundamentals { get; set; } = new();
}

public class TechnicalIndicators
{
    [JsonPropertyName("rsi")]
    public decimal Rsi { get; set; }
    
    [JsonPropertyName("macd")]
    public decimal Macd { get; set; }
    
    [JsonPropertyName("macdSignal")]
    public decimal MacdSignal { get; set; }
    
    [JsonPropertyName("macdHistogram")]
    public decimal MacdHistogram { get; set; }
    
    [JsonPropertyName("sma20")]
    public decimal Sma20 { get; set; }
    
    [JsonPropertyName("sma50")]
    public decimal Sma50 { get; set; }
    
    [JsonPropertyName("sma200")]
    public decimal Sma200 { get; set; }
    
    [JsonPropertyName("ema12")]
    public decimal Ema12 { get; set; }
    
    [JsonPropertyName("ema26")]
    public decimal Ema26 { get; set; }
    
    [JsonPropertyName("bollingerUpper")]
    public decimal BollingerUpper { get; set; }
    
    [JsonPropertyName("bollingerMiddle")]
    public decimal BollingerMiddle { get; set; }
    
    [JsonPropertyName("bollingerLower")]
    public decimal BollingerLower { get; set; }
    
    [JsonPropertyName("volumeSma")]
    public decimal VolumeSma { get; set; }
}

public class Fundamentals
{
    [JsonPropertyName("revenue")]
    public decimal Revenue { get; set; }
    
    [JsonPropertyName("revenueGrowth")]
    public decimal RevenueGrowth { get; set; }
    
    [JsonPropertyName("netIncome")]
    public decimal NetIncome { get; set; }
    
    [JsonPropertyName("grossMargin")]
    public decimal GrossMargin { get; set; }
    
    [JsonPropertyName("operatingMargin")]
    public decimal OperatingMargin { get; set; }
    
    [JsonPropertyName("netMargin")]
    public decimal NetMargin { get; set; }
    
    [JsonPropertyName("debtToEquity")]
    public decimal DebtToEquity { get; set; }
    
    [JsonPropertyName("returnOnEquity")]
    public decimal ReturnOnEquity { get; set; }
    
    [JsonPropertyName("returnOnAssets")]
    public decimal ReturnOnAssets { get; set; }
    
    [JsonPropertyName("currentRatio")]
    public decimal CurrentRatio { get; set; }
    
    [JsonPropertyName("quickRatio")]
    public decimal QuickRatio { get; set; }
    
    [JsonPropertyName("bookValuePerShare")]
    public decimal BookValuePerShare { get; set; }
    
    [JsonPropertyName("priceToBook")]
    public decimal PriceToBook { get; set; }
    
    [JsonPropertyName("priceToSales")]
    public decimal PriceToSales { get; set; }
}
