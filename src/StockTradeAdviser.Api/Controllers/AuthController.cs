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
            // For testing without authentication, use a mock object ID
            var objectId = "mock-user-id";
            
            var user = await _userService.GetOrCreateUserAsync(objectId, null);
            if (user == null)
            {
                return NotFound("User not found");
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user profile");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("profile")]
    public async Task<ActionResult<User>> UpdateProfile([FromBody] User updatedUser)
    {
        try
        {
            // For testing without authentication, use a mock object ID
            var objectId = "mock-user-id";
            
            // var objectId = User.GetObjectId();
            // if (string.IsNullOrEmpty(objectId))
            // {
            //     return Unauthorized("User object ID not found");
            // }

            var existingUser = await _userService.GetUserByAzureAdObjectIdAsync(objectId);
            if (existingUser == null)
            {
                return NotFound("User not found");
            }

            // Allow ID mismatch for testing - update the existing user with the new data
            // In production, you would want to validate this
            // if (existingUser.Id != updatedUser.Id)
            // {
            //     return BadRequest("User ID mismatch");
            // }

            // Update the existing user with the new data, but keep the original ID
            updatedUser.Id = existingUser.Id;
            updatedUser.AzureAdObjectId = existingUser.AzureAdObjectId;

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
