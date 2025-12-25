using StockTradeAdviser.Core.Models;
using System.Security.Claims;

namespace StockTradeAdviser.Api.Services;

public interface IUserService
{
    Task<User?> GetUserAsync(string userId);
    Task<User?> GetUserByAzureAdObjectIdAsync(string azureAdObjectId);
    Task<User> GetOrCreateUserAsync(string azureAdObjectId, ClaimsPrincipal claimsPrincipal);
    Task<User> UpdateUserAsync(User user);
    Task DeleteUserAsync(string userId);
}
