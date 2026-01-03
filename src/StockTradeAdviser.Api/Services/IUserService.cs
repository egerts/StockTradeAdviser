using StockTradeAdviser.Core.Models;
using System.Security.Claims;

namespace StockTradeAdviser.Api.Services;

public interface IUserService
{
    Task<User?> GetUserAsync(string userId);
    Task<User?> GetUserByEntraObjectIdAsync(string entraObjectId);
    Task<User> GetOrCreateUserAsync(string entraObjectId, ClaimsPrincipal claimsPrincipal);
    Task<User> UpdateUserAsync(User user);
    Task DeleteUserAsync(string userId);
}
