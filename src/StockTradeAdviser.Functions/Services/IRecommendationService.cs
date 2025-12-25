using StockTradeAdviser.Core.Models;

namespace StockTradeAdviser.Functions.Services;

public interface IRecommendationService
{
    Task<List<Recommendation>> GenerateRecommendationsAsync(string userId);
    Task<Recommendation> GenerateRecommendationAsync(string userId, string symbol);
    Task<RecommendationScore> AnalyzeStockAsync(string symbol, User user);
    Task UpdateRecommendationHistoryAsync(Recommendation recommendation, RecommendationOutcome outcome);
}
