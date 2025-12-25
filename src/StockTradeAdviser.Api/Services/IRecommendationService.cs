using StockTradeAdviser.Core.Models;

namespace StockTradeAdviser.Api.Services;

public interface IRecommendationService
{
    Task<User?> GetUserByAzureAdObjectIdAsync(string azureAdObjectId);
    Task<Recommendation?> GetRecommendationAsync(string recommendationId, string userId);
    Task<List<Recommendation>> GetUserRecommendationsAsync(string userId, int limit = 50);
    Task<List<Recommendation>> GetActiveRecommendationsAsync(string userId);
    Task<List<Recommendation>> GenerateRecommendationsAsync(string userId);
    Task<Recommendation> GenerateRecommendationAsync(string userId, string symbol);
    Task<Recommendation> UpdateRecommendationAsync(Recommendation recommendation);
    Task<List<RecommendationHistory>> GetRecommendationHistoryAsync(string userId, int limit = 100);
    Task UpdateRecommendationHistoryAsync(Recommendation recommendation, RecommendationOutcome outcome);
}
