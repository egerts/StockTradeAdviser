using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using StockTradeAdviser.Functions.Services;

namespace StockTradeAdviser.Functions.Functions;

public class RecommendationGenerationFunction
{
    private readonly ILogger<RecommendationGenerationFunction> _logger;
    private readonly IRecommendationService _recommendationService;
    private readonly ICosmosDbService _cosmosDbService;

    public RecommendationGenerationFunction(
        ILogger<RecommendationGenerationFunction> logger,
        IRecommendationService recommendationService,
        ICosmosDbService cosmosDbService)
    {
        _logger = logger;
        _recommendationService = recommendationService;
        _cosmosDbService = cosmosDbService;
    }

    [Function("GenerateRecommendationsTimer")]
    public async Task RunTimer([TimerTrigger("0 0 6,14,22 * * *")] TimerInfo timer, FunctionContext context)
    {
        _logger.LogInformation($"Recommendation generation timer trigger function executed at: {DateTime.UtcNow}");

        try
        {
            var users = await GetAllActiveUsers();
            var totalRecommendations = 0;

            foreach (var user in users)
            {
                try
                {
                    var recommendations = await _recommendationService.GenerateRecommendationsAsync(user.Id);
                    totalRecommendations += recommendations.Count;
                    
                    _logger.LogInformation($"Generated {recommendations.Count} recommendations for user: {user.Email}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error generating recommendations for user: {user.Email}");
                }
            }

            _logger.LogInformation($"Successfully generated {totalRecommendations} total recommendations for {users.Count} users");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during recommendation generation");
            throw;
        }
    }

    private async Task<List<StockTradeAdviser.Core.Models.User>> GetAllActiveUsers()
    {
        return new List<StockTradeAdviser.Core.Models.User>();
    }
}
