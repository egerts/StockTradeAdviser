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

    public async Task<User?> GetUserByEntraObjectIdAsync(string entraObjectId)
    {
        try
        {
            return await _cosmosDbService.GetUserByEntraObjectIdAsync(entraObjectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by Entra object ID: {EntraObjectId}", entraObjectId);
            throw;
        }
    }

    public async Task<User> GetOrCreateUserAsync(string entraObjectId, ClaimsPrincipal claimsPrincipal)
    {
        try
        {
            _logger.LogInformation("GetOrCreateUserAsync called with EntraObjectId: {EntraObjectId}", entraObjectId);
            
            _logger.LogInformation("Checking for existing user");
            var existingUser = await _cosmosDbService.GetUserByEntraObjectIdAsync(entraObjectId);
            
            if (existingUser != null)
            {
                _logger.LogInformation("Found existing user with ID: {UserId}", existingUser.Id);
                return existingUser;
            }

            _logger.LogInformation("Creating new user");
            var newUser = new User
            {
                EntraObjectId = entraObjectId,
                Email = claimsPrincipal?.FindFirst(ClaimTypes.Email)?.Value ?? claimsPrincipal?.FindFirst("preferred_username")?.Value ?? "test@example.com",
                DisplayName = claimsPrincipal?.FindFirst("name")?.Value ?? "Test User",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Calling CreateUserAsync for new user");
            var createdUser = await _cosmosDbService.CreateUserAsync(newUser);
            _logger.LogInformation("Successfully created user with ID: {UserId}", createdUser.Id);
            
            return createdUser;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating or getting user: {EntraObjectId}. Exception: {ExceptionType}, Message: {Message}", 
                entraObjectId, ex.GetType().Name, ex.Message);
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
