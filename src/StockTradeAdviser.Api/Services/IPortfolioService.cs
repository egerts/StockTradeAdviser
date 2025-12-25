using StockTradeAdviser.Core.Models;

namespace StockTradeAdviser.Api.Services;

public interface IPortfolioService
{
    Task<User?> GetUserByAzureAdObjectIdAsync(string azureAdObjectId);
    Task<Portfolio?> GetPortfolioAsync(string portfolioId, string userId);
    Task<List<Portfolio>> GetUserPortfoliosAsync(string userId);
    Task<Portfolio> CreatePortfolioAsync(Portfolio portfolio);
    Task<Portfolio> UpdatePortfolioAsync(Portfolio portfolio);
    Task DeletePortfolioAsync(string portfolioId, string userId);
    Task<Portfolio> AddHoldingAsync(string portfolioId, string userId, Holding holding);
    Task<Portfolio> UpdateHoldingAsync(string portfolioId, string userId, Holding holding);
    Task DeleteHoldingAsync(string portfolioId, string userId, string holdingId);
}
