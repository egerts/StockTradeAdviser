using Microsoft.Extensions.Logging;
using StockTradeAdviser.Core.Models;

namespace StockTradeAdviser.Api.Services;

public class MockTransactionService : ITransactionService
{
    private readonly ILogger<MockTransactionService> _logger;
    private readonly List<Transaction> _mockTransactions;
    private readonly List<Portfolio> _mockPortfolios;

    public MockTransactionService(ILogger<MockTransactionService> logger)
    {
        _logger = logger;
        _mockTransactions = new List<Transaction>();
        _mockPortfolios = new List<Portfolio>();
        InitializeSampleData();
    }

    private void InitializeSampleData()
    {
        var userId = "mock-user-id";
        var portfolioId = "d193bd08-a582-423a-90d8-27a162e05f36"; // Tech Growth Portfolio
        var holdingId1 = "a1b2c3d4-e5f6-7890-abcd-ef1234567890"; // AAPL
        var holdingId2 = "b2c3d4e5-f6a7-8901-bcde-f23456789012"; // MSFT

        // Initialize sample portfolio data
        _mockPortfolios.Add(new Portfolio
        {
            Id = portfolioId,
            UserId = userId,
            Name = "Tech Growth Portfolio",
            Description = "Focus on high-growth technology stocks",
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            Holdings = new List<Holding>
            {
                new Holding
                {
                    Id = holdingId1,
                    Symbol = "AAPL",
                    Quantity = 150m,
                    AverageCostPrice = 155.50m,
                    CurrentPrice = 195.50m,
                    AssetType = AssetType.Stock,
                    LastUpdated = DateTime.UtcNow
                },
                new Holding
                {
                    Id = holdingId2,
                    Symbol = "MSFT",
                    Quantity = 100m,
                    AverageCostPrice = 385.00m,
                    CurrentPrice = 425.30m,
                    AssetType = AssetType.Stock,
                    LastUpdated = DateTime.UtcNow
                }
            }
        });

        // Sample transactions for AAPL
        _mockTransactions.Add(new Transaction
        {
            Id = "txn-1",
            HoldingId = holdingId1,
            PortfolioId = portfolioId,
            UserId = userId,
            Type = TransactionType.Buy,
            Quantity = 100m,
            Price = 150.00m,
            TotalAmount = 15000.00m,
            Fees = 9.99m,
            Timestamp = DateTime.UtcNow.AddDays(-30),
            Notes = "Initial AAPL purchase"
        });

        _mockTransactions.Add(new Transaction
        {
            Id = "txn-2",
            HoldingId = holdingId1,
            PortfolioId = portfolioId,
            UserId = userId,
            Type = TransactionType.Buy,
            Quantity = 50m,
            Price = 165.50m,
            TotalAmount = 8275.00m,
            Fees = 7.50m,
            Timestamp = DateTime.UtcNow.AddDays(-15),
            Notes = "Additional AAPL shares"
        });

        _mockTransactions.Add(new Transaction
        {
            Id = "txn-3",
            HoldingId = holdingId1,
            PortfolioId = portfolioId,
            UserId = userId,
            Type = TransactionType.Dividend,
            Quantity = 150m,
            Price = 0.96m,
            TotalAmount = 144.00m,
            Fees = 0m,
            Timestamp = DateTime.UtcNow.AddDays(-7),
            Notes = "AAPL quarterly dividend"
        });

        // Sample transactions for MSFT
        _mockTransactions.Add(new Transaction
        {
            Id = "txn-4",
            HoldingId = holdingId2,
            PortfolioId = portfolioId,
            UserId = userId,
            Type = TransactionType.Buy,
            Quantity = 75m,
            Price = 380.00m,
            TotalAmount = 28500.00m,
            Fees = 12.50m,
            Timestamp = DateTime.UtcNow.AddDays(-20),
            Notes = "Initial MSFT purchase"
        });

        _mockTransactions.Add(new Transaction
        {
            Id = "txn-5",
            HoldingId = holdingId2,
            PortfolioId = portfolioId,
            UserId = userId,
            Type = TransactionType.Buy,
            Quantity = 25m,
            Price = 410.25m,
            TotalAmount = 10256.25m,
            Fees = 8.75m,
            Timestamp = DateTime.UtcNow.AddDays(-10),
            Notes = "Additional MSFT shares"
        });
    }

    public async Task<List<Transaction>> GetTransactionsAsync(string portfolioId, string holdingId, string userId)
    {
        await Task.Delay(100); // Simulate async operation
        return _mockTransactions
            .Where(t => t.PortfolioId == portfolioId && t.HoldingId == holdingId && t.UserId == userId)
            .OrderByDescending(t => t.Timestamp)
            .ToList();
    }

    public async Task<Transaction?> GetTransactionAsync(string transactionId, string userId)
    {
        await Task.Delay(100); // Simulate async operation
        return _mockTransactions.FirstOrDefault(t => t.Id == transactionId && t.UserId == userId);
    }

    public async Task<Transaction> CreateTransactionAsync(string portfolioId, string holdingId, CreateTransactionRequest request, string userId)
    {
        await Task.Delay(200); // Simulate async operation
        
        var transaction = new Transaction
        {
            Id = $"txn-{Guid.NewGuid():N}",
            HoldingId = holdingId,
            PortfolioId = portfolioId,
            UserId = userId,
            Type = request.GetTransactionType(),
            Quantity = request.Quantity,
            Price = request.Price,
            TotalAmount = request.Quantity * request.Price,
            Fees = request.Fees,
            Notes = request.Notes,
            Timestamp = request.Timestamp ?? DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockTransactions.Add(transaction);
        
        // Update the holding based on the transaction
        await UpdateHoldingFromTransaction(portfolioId, holdingId, transaction, userId);
        
        _logger.LogInformation("Created transaction {TransactionId} for holding {HoldingId} in portfolio {PortfolioId}", transaction.Id, holdingId, portfolioId);
        
        return transaction;
    }

    private async Task UpdateHoldingFromTransaction(string portfolioId, string holdingId, Transaction transaction, string userId)
    {
        // Find the portfolio and holding
        var portfolio = _mockPortfolios.FirstOrDefault(p => p.Id == portfolioId && p.UserId == userId);
        if (portfolio == null) return;

        var holding = portfolio.Holdings.FirstOrDefault(h => h.Id == holdingId);
        if (holding == null) return;

        // Update holding based on transaction type
        switch (transaction.Type)
        {
            case TransactionType.Buy:
                var originalQuantity = holding.Quantity;
                holding.Quantity += transaction.Quantity;
                // Update average cost price using transaction price, not holding average cost
                holding.AverageCostPrice = ((holding.AverageCostPrice * originalQuantity) + transaction.TotalAmount) / holding.Quantity;
                break;
                
            case TransactionType.Sell:
                holding.Quantity -= transaction.Quantity;
                break;
                
            case TransactionType.Dividend:
                // For dividend, we might want to track cash or reinvest
                // For now, just log it
                break;
                
            case TransactionType.Split:
                // For stock split, adjust quantity and average cost
                holding.Quantity *= transaction.Quantity; // Assuming quantity represents split ratio
                holding.AverageCostPrice /= transaction.Quantity;
                break;
        }

        // Update current price to match the transaction price
        holding.CurrentPrice = transaction.Price;
        holding.LastUpdated = DateTime.UtcNow;
        
        // Portfolio totals are computed properties, no need to manually update them
        
        _logger.LogInformation("Updated holding {HoldingId} based on transaction {TransactionType} {Quantity} @ {Price}", 
            holdingId, transaction.Type, transaction.Quantity, transaction.Price);
    }

    public async Task<Transaction> UpdateTransactionAsync(string transactionId, UpdateTransactionRequest request, string userId)
    {
        await Task.Delay(100); // Simulate async operation
        
        var transaction = _mockTransactions.FirstOrDefault(t => t.Id == transactionId && t.UserId == userId);
        if (transaction == null)
        {
            throw new InvalidOperationException("Transaction not found");
        }

        if (request.Type.HasValue) transaction.Type = request.Type.Value;
        if (request.Quantity.HasValue) transaction.Quantity = request.Quantity.Value;
        if (request.Price.HasValue) transaction.Price = request.Price.Value;
        if (request.Notes != null) transaction.Notes = request.Notes;
        if (request.Fees.HasValue) transaction.Fees = request.Fees.Value;
        if (request.Timestamp.HasValue) transaction.Timestamp = request.Timestamp.Value;
        
        transaction.TotalAmount = transaction.Quantity * transaction.Price;
        transaction.UpdatedAt = DateTime.UtcNow;
        
        _logger.LogInformation("Updated transaction {TransactionId}", transactionId);
        
        return transaction;
    }

    public async Task DeleteTransactionAsync(string transactionId, string userId)
    {
        await Task.Delay(100); // Simulate async operation
        
        var transaction = _mockTransactions.FirstOrDefault(t => t.Id == transactionId && t.UserId == userId);
        if (transaction != null)
        {
            _mockTransactions.Remove(transaction);
            _logger.LogInformation("Deleted transaction {TransactionId}", transactionId);
        }
    }

    public async Task<List<Transaction>> GetPortfolioTransactionsAsync(string portfolioId, string userId)
    {
        await Task.Delay(100); // Simulate async operation
        return _mockTransactions
            .Where(t => t.PortfolioId == portfolioId && t.UserId == userId)
            .OrderByDescending(t => t.Timestamp)
            .ToList();
    }
}
