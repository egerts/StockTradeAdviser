using StockTradeAdviser.Core.Models;

namespace StockTradeAdviser.Data.Services;

public interface ICosmosDbService
{
    Task<User?> GetUserAsync(string userId);
    Task<User?> GetUserByAzureAdObjectIdAsync(string azureAdObjectId);
    Task<User> CreateUserAsync(User user);
    Task<User> UpdateUserAsync(User user);
    Task DeleteUserAsync(string userId);
    
    Task<Portfolio?> GetPortfolioAsync(string portfolioId, string userId);
    Task<List<Portfolio>> GetUserPortfoliosAsync(string userId);
    Task<Portfolio> CreatePortfolioAsync(Portfolio portfolio);
    Task<Portfolio> UpdatePortfolioAsync(Portfolio portfolio);
    Task DeletePortfolioAsync(string portfolioId, string userId);
    
    Task<StockData?> GetStockDataAsync(string symbol);
    Task<List<StockData>> GetMultipleStockDataAsync(List<string> symbols);
    Task<StockData> UpdateStockDataAsync(StockData stockData);
    
    Task<Recommendation?> GetRecommendationAsync(string recommendationId, string userId);
    Task<List<Recommendation>> GetUserRecommendationsAsync(string userId, int limit = 50);
    Task<List<Recommendation>> GetActiveRecommendationsAsync(string userId);
    Task<Recommendation> CreateRecommendationAsync(Recommendation recommendation);
    Task<Recommendation> UpdateRecommendationAsync(Recommendation recommendation);
    Task<List<RecommendationHistory>> GetRecommendationHistoryAsync(string userId, int limit = 100);
    Task<RecommendationHistory> CreateRecommendationHistoryAsync(RecommendationHistory history);
}
