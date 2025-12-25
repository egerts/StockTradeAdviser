using Microsoft.Extensions.Logging;
using StockTradeAdviser.Core.Models;
using StockTradeAdviser.Data.Services;
using System.Security.Claims;

namespace StockTradeAdviser.Api.Services;

public class UserService : IUserService
{
    private readonly ICosmosDbService _cosmosDbService;
    private readonly ILogger<UserService> _logger;

    public UserService(ICosmosDbService cosmosDbService, ILogger<UserService> logger)
    {
        _cosmosDbService = cosmosDbService;
        _logger = logger;
    }

    public async Task<User?> GetUserAsync(string userId)
    {
        try
        {
            return await _cosmosDbService.GetUserAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user: {UserId}", userId);
            throw;
        }
    }

    public async Task<User?> GetUserByAzureAdObjectIdAsync(string azureAdObjectId)
    {
        try
        {
            return await _cosmosDbService.GetUserByAzureAdObjectIdAsync(azureAdObjectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by Azure AD object ID: {AzureAdObjectId}", azureAdObjectId);
            throw;
        }
    }

    public async Task<User> GetOrCreateUserAsync(string azureAdObjectId, ClaimsPrincipal claimsPrincipal)
    {
        try
        {
            var existingUser = await _cosmosDbService.GetUserByAzureAdObjectIdAsync(azureAdObjectId);
            if (existingUser != null)
            {
                return existingUser;
            }

            var newUser = new User
            {
                AzureAdObjectId = azureAdObjectId,
                Email = claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value ?? claimsPrincipal.FindFirst("preferred_username")?.Value ?? string.Empty,
                DisplayName = claimsPrincipal.FindFirst("name")?.Value ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            return await _cosmosDbService.CreateUserAsync(newUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating or getting user: {AzureAdObjectId}", azureAdObjectId);
            throw;
        }
    }

    public async Task<User> UpdateUserAsync(User user)
    {
        try
        {
            return await _cosmosDbService.UpdateUserAsync(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", user.Id);
            throw;
        }
    }

    public async Task DeleteUserAsync(string userId)
    {
        try
        {
            await _cosmosDbService.DeleteUserAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId}", userId);
            throw;
        }
    }
}
