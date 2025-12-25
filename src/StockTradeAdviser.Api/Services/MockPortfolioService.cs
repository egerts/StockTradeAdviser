using Microsoft.Extensions.Logging;
using StockTradeAdviser.Core.Models;

namespace StockTradeAdviser.Api.Services;

public class MockPortfolioService : IPortfolioService
{
    private readonly ILogger<MockPortfolioService> _logger;
    private static readonly List<Portfolio> _mockPortfolios = new();

    public MockPortfolioService(ILogger<MockPortfolioService> logger)
    {
        _logger = logger;
        
        // Initialize with some sample data
        if (!_mockPortfolios.Any())
        {
            InitializeSampleData();
        }
    }

    private void InitializeSampleData()
    {
        var userId = "mock-user-id";
        
        _mockPortfolios.Add(new Portfolio
        {
            Id = "d193bd08-a582-423a-90d8-27a162e05f36", // Fixed GUID for consistency
            UserId = userId,
            Name = "Tech Growth Portfolio",
            Description = "Focus on high-growth technology stocks",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Holdings = new List<Holding>
            {
                new Holding
                {
                    Id = "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
                    Symbol = "AAPL",
                    Quantity = 100,
                    AverageCostPrice = 150.00m,
                    CurrentPrice = 175.50m,
                    AssetType = AssetType.Stock,
                    LastUpdated = DateTime.UtcNow
                },
                new Holding
                {
                    Id = "b2c3d4e5-f6a7-8901-bcde-f23456789012",
                    Symbol = "MSFT",
                    Quantity = 50,
                    AverageCostPrice = 300.00m,
                    CurrentPrice = 380.25m,
                    AssetType = AssetType.Stock,
                    LastUpdated = DateTime.UtcNow
                }
            }
        });

        _mockPortfolios.Add(new Portfolio
        {
            Id = "94aa9a25-8576-465a-aebb-7bf55a989586", // Fixed GUID for consistency
            UserId = userId,
            Name = "Dividend Income Portfolio",
            Description = "Stable dividend-paying stocks for income",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Holdings = new List<Holding>
            {
                new Holding
                {
                    Id = "c3d4e5f6-a7b8-9012-cdef-345678901234",
                    Symbol = "JNJ",
                    Quantity = 75,
                    AverageCostPrice = 140.00m,
                    CurrentPrice = 155.75m,
                    AssetType = AssetType.Stock,
                    LastUpdated = DateTime.UtcNow
                }
            }
        });
    }

    public async Task<User?> GetUserByAzureAdObjectIdAsync(string azureAdObjectId)
    {
        // Return mock user
        return new User
        {
            Id = "mock-user-id",
            AzureAdObjectId = azureAdObjectId,
            Email = "test@example.com",
            DisplayName = "Test User",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Portfolios = new List<Portfolio>(),
            TradingStrategy = new TradingStrategy
            {
                RiskTolerance = RiskTolerance.Medium,
                InvestmentHorizon = InvestmentHorizon.MediumTerm,
                MaxPortfolioSize = 20,
                PreferredSectors = new List<string>(),
                SellStrategy = new SellStrategy
                {
                    TakeProfitPercentage = 20,
                    StopLossPercentage = 10,
                    TrailingStopEnabled = false,
                    TrailingStopPercentage = 5
                }
            }
        };
    }

    public async Task<Portfolio?> GetPortfolioAsync(string portfolioId, string userId)
    {
        return _mockPortfolios.FirstOrDefault(p => p.Id == portfolioId && p.UserId == userId);
    }

    public async Task<List<Portfolio>> GetUserPortfoliosAsync(string userId)
    {
        return _mockPortfolios.Where(p => p.UserId == userId).ToList();
    }

    public async Task<Portfolio> CreatePortfolioAsync(Portfolio portfolio)
    {
        portfolio.Id = Guid.NewGuid().ToString();
        portfolio.CreatedAt = DateTime.UtcNow;
        portfolio.UpdatedAt = DateTime.UtcNow;
        portfolio.Holdings ??= new List<Holding>();
        
        _mockPortfolios.Add(portfolio);
        return portfolio;
    }

    public async Task<Portfolio> UpdatePortfolioAsync(Portfolio portfolio)
    {
        var existing = _mockPortfolios.FirstOrDefault(p => p.Id == portfolio.Id);
        if (existing != null)
        {
            existing.Name = portfolio.Name;
            existing.Description = portfolio.Description;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        return existing ?? portfolio;
    }

    public async Task DeletePortfolioAsync(string portfolioId, string userId)
    {
        var portfolio = _mockPortfolios.FirstOrDefault(p => p.Id == portfolioId && p.UserId == userId);
        if (portfolio != null)
        {
            _mockPortfolios.Remove(portfolio);
        }
    }

    public async Task<Portfolio> AddHoldingAsync(string portfolioId, string userId, Holding holding)
    {
        var portfolio = _mockPortfolios.FirstOrDefault(p => p.Id == portfolioId && p.UserId == userId);
        if (portfolio == null)
        {
            throw new InvalidOperationException("Portfolio not found");
        }

        holding.Id = Guid.NewGuid().ToString();
        holding.LastUpdated = DateTime.UtcNow;

        var existingHolding = portfolio.Holdings.FirstOrDefault(h => h.Symbol.Equals(holding.Symbol, StringComparison.OrdinalIgnoreCase));
        if (existingHolding != null)
        {
            // Update existing holding - fix division by zero by using original quantity
            var originalQuantity = existingHolding.Quantity;
            existingHolding.Quantity += holding.Quantity;
            
            if (existingHolding.Quantity > 0)
            {
                existingHolding.AverageCostPrice = ((existingHolding.AverageCostPrice * originalQuantity) + (holding.AverageCostPrice * holding.Quantity)) / existingHolding.Quantity;
            }
            
            existingHolding.LastUpdated = DateTime.UtcNow;
        }
        else
        {
            portfolio.Holdings.Add(holding);
        }

        portfolio.UpdatedAt = DateTime.UtcNow;
        return portfolio;
    }

    public async Task<Portfolio> UpdateHoldingAsync(string portfolioId, string userId, Holding holding)
    {
        var portfolio = _mockPortfolios.FirstOrDefault(p => p.Id == portfolioId && p.UserId == userId);
        if (portfolio == null)
        {
            throw new InvalidOperationException("Portfolio not found");
        }

        var existingHolding = portfolio.Holdings.FirstOrDefault(h => h.Id == holding.Id);
        if (existingHolding == null)
        {
            throw new InvalidOperationException("Holding not found");
        }

        existingHolding.Quantity = holding.Quantity;
        existingHolding.AverageCostPrice = holding.AverageCostPrice;
        existingHolding.CurrentPrice = holding.CurrentPrice;
        existingHolding.LastUpdated = DateTime.UtcNow;
        portfolio.UpdatedAt = DateTime.UtcNow;

        return portfolio;
    }

    public async Task DeleteHoldingAsync(string portfolioId, string userId, string holdingId)
    {
        var portfolio = _mockPortfolios.FirstOrDefault(p => p.Id == portfolioId && p.UserId == userId);
        if (portfolio == null)
        {
            throw new InvalidOperationException("Portfolio not found");
        }

        var holding = portfolio.Holdings.FirstOrDefault(h => h.Id == holdingId);
        if (holding != null)
        {
            portfolio.Holdings.Remove(holding);
            portfolio.UpdatedAt = DateTime.UtcNow;
        }
    }
}
