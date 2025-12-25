using StockTradeAdviser.Core.Models;

namespace StockTradeAdviser.Api.Services;

public interface ITransactionService
{
    Task<List<Transaction>> GetTransactionsAsync(string portfolioId, string holdingId, string userId);
    Task<Transaction?> GetTransactionAsync(string transactionId, string userId);
    Task<Transaction> CreateTransactionAsync(string portfolioId, string holdingId, CreateTransactionRequest request, string userId);
    Task<Transaction> UpdateTransactionAsync(string transactionId, UpdateTransactionRequest request, string userId);
    Task DeleteTransactionAsync(string transactionId, string userId);
    Task<List<Transaction>> GetPortfolioTransactionsAsync(string portfolioId, string userId);
}
