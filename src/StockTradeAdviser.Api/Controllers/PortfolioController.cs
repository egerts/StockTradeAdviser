using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockTradeAdviser.Api.Services;
using StockTradeAdviser.Core.Models;
using Microsoft.Identity.Web;
using System.Text.Json.Serialization;

namespace StockTradeAdviser.Api.Controllers;

[ApiController]
[Route("api/portfolios")]
// [Authorize] - Temporarily disabled for testing
public class PortfolioController : ControllerBase
{
    private readonly IPortfolioService _portfolioService;
    private readonly ITransactionService _transactionService;
    private readonly ILogger<PortfolioController> _logger;

    public PortfolioController(IPortfolioService portfolioService, ITransactionService transactionService, ILogger<PortfolioController> logger)
    {
        _portfolioService = portfolioService;
        _transactionService = transactionService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<Portfolio>>> GetPortfolios()
    {
        try
        {
            // For testing without authentication, use mock user ID
            var userId = "mock-user-id";
            
            var portfolios = await _portfolioService.GetUserPortfoliosAsync(userId);
            return Ok(portfolios);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting portfolios");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Portfolio>> GetPortfolio(string id)
    {
        try
        {
            // For testing without authentication, use mock user ID
            var userId = "mock-user-id";

            var portfolio = await _portfolioService.GetPortfolioAsync(id, userId);
            if (portfolio == null)
            {
                return NotFound("Portfolio not found");
            }

            return Ok(portfolio);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting portfolio {PortfolioId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<Portfolio>> CreatePortfolio([FromBody] Portfolio portfolio)
    {
        try
        {
            // For testing without authentication, use mock user ID
            var userId = "mock-user-id";

            portfolio.UserId = userId;
            var createdPortfolio = await _portfolioService.CreatePortfolioAsync(portfolio);
            return CreatedAtAction(nameof(GetPortfolio), new { id = createdPortfolio.Id }, createdPortfolio);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating portfolio");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Portfolio>> UpdatePortfolio(string id, [FromBody] Portfolio portfolio)
    {
        try
        {
            // For testing without authentication, use mock user ID
            var userId = "mock-user-id";

            if (id != portfolio.Id)
            {
                return BadRequest("Portfolio ID mismatch");
            }

            var existingPortfolio = await _portfolioService.GetPortfolioAsync(id, userId);
            if (existingPortfolio == null)
            {
                return NotFound("Portfolio not found");
            }

            portfolio.UserId = userId;
            var updatedPortfolio = await _portfolioService.UpdatePortfolioAsync(portfolio);
            return Ok(updatedPortfolio);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating portfolio {PortfolioId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePortfolio(string id)
    {
        try
        {
            // For testing without authentication, use mock user ID
            var userId = "mock-user-id";

            var existingPortfolio = await _portfolioService.GetPortfolioAsync(id, userId);
            if (existingPortfolio == null)
            {
                return NotFound("Portfolio not found");
            }

            await _portfolioService.DeletePortfolioAsync(id, userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting portfolio {PortfolioId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    public class HoldingDto
    {
        public string Symbol { get; set; } = "";
        public decimal Quantity { get; set; }
        public decimal AverageCostPrice { get; set; }
        public decimal CurrentPrice { get; set; }
        public string AssetType { get; set; } = "Stock";
    }

    public class AddHoldingRequest
    {
        public HoldingDto Holding { get; set; } = new HoldingDto();
    }

    [HttpPost("{id}/holdings")]
    public async Task<ActionResult<Portfolio>> AddHolding(string id, [FromBody] AddHoldingRequest request)
    {
        try
        {
            // For testing without authentication, use mock user ID
            var userId = "mock-user-id";

            var portfolio = await _portfolioService.GetPortfolioAsync(id, userId);
            if (portfolio == null)
            {
                return NotFound("Portfolio not found");
            }

            var holdingDto = request.Holding ?? new HoldingDto();
            
            // Convert DTO to Holding entity with proper AssetType conversion
            var holding = new Holding
            {
                Symbol = holdingDto.Symbol,
                Quantity = holdingDto.Quantity,
                AverageCostPrice = holdingDto.AverageCostPrice,
                CurrentPrice = holdingDto.CurrentPrice,
                AssetType = Enum.TryParse<AssetType>(holdingDto.AssetType, true, out var assetType) ? assetType : AssetType.Stock
            };

            var updatedPortfolio = await _portfolioService.AddHoldingAsync(id, userId, holding);
            return Ok(updatedPortfolio);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding holding to portfolio {PortfolioId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    public class UpdateHoldingRequest
    {
        public HoldingDto Holding { get; set; } = new HoldingDto();
    }

    [HttpPut("{id}/holdings/{holdingId}")]
    public async Task<ActionResult<Portfolio>> UpdateHolding(string id, string holdingId, [FromBody] UpdateHoldingRequest request)
    {
        try
        {
            // For testing without authentication, use mock user ID
            var userId = "mock-user-id";

            var portfolio = await _portfolioService.GetPortfolioAsync(id, userId);
            if (portfolio == null)
            {
                return NotFound("Portfolio not found");
            }

            var holdingDto = request.Holding;
            var existingHolding = portfolio.Holdings.FirstOrDefault(h => h.Id == holdingId);
            if (existingHolding == null)
            {
                return NotFound("Holding not found");
            }

            // Update the existing holding with new values
            existingHolding.Symbol = holdingDto.Symbol;
            existingHolding.Quantity = holdingDto.Quantity;
            existingHolding.AverageCostPrice = holdingDto.AverageCostPrice;
            existingHolding.CurrentPrice = holdingDto.CurrentPrice;
            existingHolding.AssetType = Enum.TryParse<AssetType>(holdingDto.AssetType, true, out var assetType) ? assetType : AssetType.Stock;
            existingHolding.LastUpdated = DateTime.UtcNow;

            var updatedPortfolio = await _portfolioService.UpdateHoldingAsync(id, userId, existingHolding);
            return Ok(updatedPortfolio);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating holding {HoldingId} in portfolio {PortfolioId}", holdingId, id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}/holdings/{holdingId}")]
    public async Task<IActionResult> DeleteHolding(string id, string holdingId)
    {
        try
        {
            // For testing without authentication, use mock user ID
            var userId = "mock-user-id";

            var portfolio = await _portfolioService.GetPortfolioAsync(id, userId);
            if (portfolio == null)
            {
                return NotFound("Portfolio not found");
            }

            await _portfolioService.DeleteHoldingAsync(id, userId, holdingId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting holding {HoldingId} from portfolio {PortfolioId}", holdingId, id);
            return StatusCode(500, "Internal server error");
        }
    }

    // Transaction endpoints
    [HttpGet("{id}/transactions")]
    public async Task<ActionResult<List<Transaction>>> GetPortfolioTransactions(string id)
    {
        try
        {
            // For testing without authentication, use mock user ID
            var userId = "mock-user-id";

            var transactions = await _transactionService.GetPortfolioTransactionsAsync(id, userId);
            return Ok(transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transactions for portfolio {PortfolioId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}/holdings/{holdingId}/transactions")]
    public async Task<ActionResult<List<Transaction>>> GetHoldingTransactions(string id, string holdingId)
    {
        try
        {
            // For testing without authentication, use mock user ID
            var userId = "mock-user-id";

            var transactions = await _transactionService.GetTransactionsAsync(id, holdingId, userId);
            return Ok(transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transactions for portfolio {PortfolioId}, holding {HoldingId}", id, holdingId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("{id}/holdings/{holdingId}/transactions")]
    public async Task<ActionResult<Transaction>> CreateTransaction(string id, string holdingId, [FromBody] CreateTransactionRequest request)
    {
        try
        {
            // For testing without authentication, use mock user ID
            var userId = "mock-user-id";

            // Verify portfolio exists
            var portfolio = await _portfolioService.GetPortfolioAsync(id, userId);
            if (portfolio == null)
            {
                return NotFound("Portfolio not found");
            }

            // Verify holding exists in portfolio
            var holding = portfolio.Holdings.FirstOrDefault(h => h.Id == holdingId);
            if (holding == null)
            {
                return NotFound("Holding not found");
            }

            var transaction = await _transactionService.CreateTransactionAsync(id, holdingId, new CreateTransactionRequest
            {
                Type = request.Type,
                Quantity = request.Quantity,
                Price = request.Price,
                Notes = request.Notes,
                Fees = request.Fees,
                Timestamp = request.Timestamp
            }, userId);
            
            // Get the updated portfolio to return to frontend
            var updatedPortfolio = await _portfolioService.GetPortfolioAsync(id, userId);
            return Ok(updatedPortfolio);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating transaction for portfolio {PortfolioId}, holding {HoldingId}", id, holdingId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}/transactions/{transactionId}")]
    public async Task<ActionResult<Transaction>> UpdateTransaction(string id, string transactionId, [FromBody] UpdateTransactionRequest request)
    {
        try
        {
            // For testing without authentication, use mock user ID
            var userId = "mock-user-id";

            // Verify portfolio exists
            var portfolio = await _portfolioService.GetPortfolioAsync(id, userId);
            if (portfolio == null)
            {
                return NotFound("Portfolio not found");
            }

            try
            {
                var transaction = await _transactionService.UpdateTransactionAsync(transactionId, request, userId);
                return Ok(transaction);
            }
            catch (InvalidOperationException)
            {
                return NotFound("Transaction not found");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating transaction {TransactionId} for portfolio {PortfolioId}", transactionId, id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}/transactions/{transactionId}")]
    public async Task<IActionResult> DeleteTransaction(string id, string transactionId)
    {
        try
        {
            // For testing without authentication, use mock user ID
            var userId = "mock-user-id";

            // Verify portfolio exists
            var portfolio = await _portfolioService.GetPortfolioAsync(id, userId);
            if (portfolio == null)
            {
                return NotFound("Portfolio not found");
            }

            await _transactionService.DeleteTransactionAsync(transactionId, userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting transaction {TransactionId} for portfolio {PortfolioId}", transactionId, id);
            return StatusCode(500, "Internal server error");
        }
    }
}
