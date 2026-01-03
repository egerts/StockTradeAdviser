using StockTradeAdviser.Core.Models;
using StockTradeAdviser.Data.Services;

namespace StockTradeAdviser.Api.Services;

public class TransactionService : ITransactionService
{
    private readonly ICosmosDbService _cosmosDbService;

    public TransactionService(ICosmosDbService cosmosDbService)
    {
        _cosmosDbService = cosmosDbService;
    }

    public async Task<List<Transaction>> GetTransactionsAsync(string portfolioId, string holdingId, string userId)
    {
        // For now, return empty list as transactions are stored within holdings
        // This would need to be implemented in the Cosmos DB service
        return new List<Transaction>();
    }

    public async Task<Transaction?> GetTransactionAsync(string transactionId, string userId)
    {
        // For now, return null as this would need to be implemented in the Cosmos DB service
        return null;
    }

    public async Task<Transaction> CreateTransactionAsync(string portfolioId, string holdingId, CreateTransactionRequest request, string userId)
    {
        var transaction = new Transaction
        {
            Id = Guid.NewGuid().ToString(),
            PortfolioId = portfolioId,
            HoldingId = holdingId,
            UserId = userId,
            Type = request.GetTransactionType(),
            Quantity = request.Quantity,
            Price = request.Price,
            TotalAmount = request.Quantity * request.Price,
            Notes = request.Notes,
            Fees = request.Fees,
            Timestamp = request.Timestamp ?? DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // For now, return the transaction as storing it would need Cosmos DB service updates
        // In a real implementation, this would call _cosmosDbService.CreateTransactionAsync(transaction)
        return transaction;
    }

    public async Task<Transaction> UpdateTransactionAsync(string transactionId, UpdateTransactionRequest request, string userId)
    {
        // For now, return a mock transaction as this would need Cosmos DB service updates
        var existingTransaction = await GetTransactionAsync(transactionId, userId);
        if (existingTransaction == null)
        {
            throw new InvalidOperationException("Transaction not found");
        }

        if (request.Quantity.HasValue) existingTransaction.Quantity = request.Quantity.Value;
        if (request.Price.HasValue) existingTransaction.Price = request.Price.Value;
        if (request.Notes != null) existingTransaction.Notes = request.Notes;
        if (request.Fees.HasValue) existingTransaction.Fees = request.Fees.Value;
        if (request.Timestamp.HasValue) existingTransaction.Timestamp = request.Timestamp.Value;

        existingTransaction.TotalAmount = existingTransaction.Quantity * existingTransaction.Price;
        existingTransaction.UpdatedAt = DateTime.UtcNow;

        return existingTransaction;
    }

    public async Task DeleteTransactionAsync(string transactionId, string userId)
    {
        // For now, do nothing as this would need Cosmos DB service updates
        await Task.CompletedTask;
    }

    public async Task<List<Transaction>> GetPortfolioTransactionsAsync(string portfolioId, string userId)
    {
        // For now, return empty list as this would need to be implemented in the Cosmos DB service
        return new List<Transaction>();
    }
}
