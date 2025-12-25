using StockTradeAdviser.Core.Models;
using System.Security.Claims;

namespace StockTradeAdviser.Api.Services;

public class MockUserService : IUserService
{
    public async Task<User?> GetUserAsync(string userId)
    {
        // Return mock user for testing
        return new User
        {
            Id = userId,
            AzureAdObjectId = userId,
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

    public async Task<User?> GetUserByAzureAdObjectIdAsync(string azureAdObjectId)
    {
        return await GetUserAsync(azureAdObjectId);
    }

    public async Task<User> GetOrCreateUserAsync(string azureAdObjectId, ClaimsPrincipal? claimsPrincipal)
    {
        // Create mock claims if none provided (for testing without auth)
        if (claimsPrincipal == null)
        {
            claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("name", "Test User"),
                new Claim("email", "test@example.com"),
                new Claim("upn", "test@example.com")
            }));
        }

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            AzureAdObjectId = azureAdObjectId,
            Email = claimsPrincipal.FindFirst("email")?.Value ?? claimsPrincipal.FindFirst("upn")?.Value ?? "test@example.com",
            DisplayName = claimsPrincipal.FindFirst("name")?.Value ?? "Test User",
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

        return user;
    }

    public async Task<User> UpdateUserAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        return user;
    }

    public async Task DeleteUserAsync(string userId)
    {
        // Mock implementation - just return completed task
        await Task.CompletedTask;
    }
}
