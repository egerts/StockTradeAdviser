using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using StockTradeAdviser.Api.Services;
using StockTradeAdviser.Core.Models;
using System.Security.Claims;

namespace StockTradeAdviser.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize] - Temporarily disabled for testing
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserService userService, ILogger<AuthController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet("profile")]
    public async Task<ActionResult<User>> GetProfile()
    {
        try
        {
            _logger.LogInformation("Starting GetProfile request");

            var objectId = User.GetObjectId();
            if (string.IsNullOrWhiteSpace(objectId))
            {
                _logger.LogWarning("User object ID missing in token");
                return Unauthorized("User object ID not found");
            }

            _logger.LogInformation("Calling GetOrCreateUserAsync with objectId: {ObjectId}", objectId);
            var user = await _userService.GetOrCreateUserAsync(objectId, User);

            if (user == null)
            {
                _logger.LogWarning("User returned as null");
                return NotFound("User not found");
            }

            _logger.LogInformation("Successfully retrieved user with ID: {UserId}", user.Id);
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user profile. Exception: {ExceptionType}, Message: {Message}", 
                ex.GetType().Name, ex.Message);
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPut("profile")]
    public async Task<ActionResult<User>> UpdateProfile([FromBody] User updatedUser)
    {
        try
        {
            var objectId = User.GetObjectId();
            if (string.IsNullOrWhiteSpace(objectId))
            {
                return Unauthorized("User object ID not found");
            }

            var existingUser = await _userService.GetUserByEntraObjectIdAsync(objectId);
            if (existingUser == null)
            {
                return NotFound("User not found");
            }

            updatedUser.Id = existingUser.Id;
            updatedUser.EntraObjectId = existingUser.EntraObjectId;

            var updated = await _userService.UpdateUserAsync(updatedUser);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        var returnUrl = Url.Content("~/");
        return SignOut(
            new AuthenticationProperties { RedirectUri = returnUrl },
            OpenIdConnectDefaults.AuthenticationScheme,
            CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
