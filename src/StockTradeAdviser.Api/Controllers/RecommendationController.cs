using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockTradeAdviser.Api.Services;
using StockTradeAdviser.Core.Models;
using Microsoft.Identity.Web;

namespace StockTradeAdviser.Api.Controllers;

[ApiController]
[Route("api/recommendations")]
// [Authorize] - Temporarily disabled for testing
public class RecommendationController : ControllerBase
{
    private readonly IRecommendationService _recommendationService;
    private readonly ILogger<RecommendationController> _logger;

    public RecommendationController(IRecommendationService recommendationService, ILogger<RecommendationController> logger)
    {
        _recommendationService = recommendationService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<Recommendation>>> GetRecommendations([FromQuery] int limit = 50)
    {
        try
        {
            // For testing without authentication, use mock user ID
            var userId = "mock-user-id";

            var recommendations = await _recommendationService.GetUserRecommendationsAsync(userId, limit);
            return Ok(recommendations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recommendations");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("active")]
    public async Task<ActionResult<List<Recommendation>>> GetActiveRecommendations()
    {
        try
        {
            // For testing without authentication, use mock user ID
            var userId = "mock-user-id";

            var recommendations = await _recommendationService.GetActiveRecommendationsAsync(userId);
            return Ok(recommendations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active recommendations");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Recommendation>> GetRecommendation(string id)
    {
        try
        {
            // For testing without authentication, use mock user ID
            var userId = "mock-user-id";

            var recommendation = await _recommendationService.GetRecommendationAsync(id, userId);
            if (recommendation == null)
            {
                return NotFound("Recommendation not found");
            }

            return Ok(recommendation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recommendation {RecommendationId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("generate")]
    public async Task<ActionResult<List<Recommendation>>> GenerateRecommendations()
    {
        try
        {
            // For testing without authentication, use mock user ID
            var userId = "mock-user-id";

            var recommendations = await _recommendationService.GenerateRecommendationsAsync(userId);
            return Ok(recommendations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating recommendations");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("generate/{symbol}")]
    public async Task<ActionResult<Recommendation>> GenerateRecommendationForSymbol(string symbol)
    {
        try
        {
            // For testing without authentication, use mock user ID
            var userId = "mock-user-id";

            var recommendation = await _recommendationService.GenerateRecommendationAsync(userId, symbol.ToUpper());
            return Ok(recommendation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating recommendation for symbol {Symbol}", symbol);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}/execute")]
    public async Task<ActionResult<Recommendation>> ExecuteRecommendation(string id, [FromBody] ExecuteRecommendationRequest request)
    {
        try
        {
            // For testing without authentication, use mock user ID
            var userId = "mock-user-id";

            var recommendation = await _recommendationService.GetRecommendationAsync(id, userId);
            if (recommendation == null)
            {
                return NotFound("Recommendation not found");
            }

            recommendation.Status = RecommendationStatus.Executed;
            recommendation.ActualAction = request.Action;
            recommendation.ActualPrice = request.Price;
            recommendation.ExecutedAt = DateTime.UtcNow;

            var updatedRecommendation = await _recommendationService.UpdateRecommendationAsync(recommendation);

            if (request.Outcome.HasValue)
            {
                await _recommendationService.UpdateRecommendationHistoryAsync(updatedRecommendation, request.Outcome.Value);
            }

            return Ok(updatedRecommendation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing recommendation {RecommendationId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("history")]
    public async Task<ActionResult<List<RecommendationHistory>>> GetRecommendationHistory([FromQuery] int limit = 100)
    {
        try
        {
            // For testing without authentication, use mock user ID
            var userId = "mock-user-id";

            var history = await _recommendationService.GetRecommendationHistoryAsync(userId, limit);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recommendation history");
            return StatusCode(500, "Internal server error");
        }
    }
}

public class ExecuteRecommendationRequest
{
    public RecommendationAction Action { get; set; }
    public decimal Price { get; set; }
    public RecommendationOutcome? Outcome { get; set; }
}
