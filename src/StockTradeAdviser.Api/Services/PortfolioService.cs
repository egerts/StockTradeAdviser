using Microsoft.Extensions.Logging;
using StockTradeAdviser.Core.Models;
using StockTradeAdviser.Data.Services;

namespace StockTradeAdviser.Api.Services;

public class PortfolioService : IPortfolioService
{
    private readonly ICosmosDbService _cosmosDbService;
    private readonly ILogger<PortfolioService> _logger;

    public PortfolioService(ICosmosDbService cosmosDbService, ILogger<PortfolioService> logger)
    {
        _cosmosDbService = cosmosDbService;
        _logger = logger;
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

    public async Task<Portfolio?> GetPortfolioAsync(string portfolioId, string userId)
    {
        try
        {
            return await _cosmosDbService.GetPortfolioAsync(portfolioId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting portfolio: {PortfolioId}", portfolioId);
            throw;
        }
    }

    public async Task<List<Portfolio>> GetUserPortfoliosAsync(string userId)
    {
        try
        {
            return await _cosmosDbService.GetUserPortfoliosAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user portfolios: {UserId}", userId);
            throw;
        }
    }

    public async Task<Portfolio> CreatePortfolioAsync(Portfolio portfolio)
    {
        try
        {
            return await _cosmosDbService.CreatePortfolioAsync(portfolio);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating portfolio");
            throw;
        }
    }

    public async Task<Portfolio> UpdatePortfolioAsync(Portfolio portfolio)
    {
        try
        {
            return await _cosmosDbService.UpdatePortfolioAsync(portfolio);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating portfolio: {PortfolioId}", portfolio.Id);
            throw;
        }
    }

    public async Task DeletePortfolioAsync(string portfolioId, string userId)
    {
        try
        {
            await _cosmosDbService.DeletePortfolioAsync(portfolioId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting portfolio: {PortfolioId}", portfolioId);
            throw;
        }
    }

    public async Task<Portfolio> AddHoldingAsync(string portfolioId, string userId, Holding holding)
    {
        try
        {
            var portfolio = await _cosmosDbService.GetPortfolioAsync(portfolioId, userId);
            if (portfolio == null)
            {
                throw new InvalidOperationException("Portfolio not found");
            }

            holding.Id = Guid.NewGuid().ToString();
            holding.LastUpdated = DateTime.UtcNow;

            var existingHolding = portfolio.Holdings.FirstOrDefault(h => h.Symbol.Equals(holding.Symbol, StringComparison.OrdinalIgnoreCase));
            if (existingHolding != null)
            {
                existingHolding.Quantity += holding.Quantity;
                existingHolding.AverageCostPrice = ((existingHolding.AverageCostPrice * existingHolding.Quantity) + (holding.AverageCostPrice * holding.Quantity)) / (existingHolding.Quantity + holding.Quantity);
                existingHolding.LastUpdated = DateTime.UtcNow;
                
                if (holding.Transactions.Any())
                {
                    existingHolding.Transactions.AddRange(holding.Transactions);
                }
            }
            else
            {
                portfolio.Holdings.Add(holding);
            }

            return await _cosmosDbService.UpdatePortfolioAsync(portfolio);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding holding to portfolio: {PortfolioId}", portfolioId);
            throw;
        }
    }

    public async Task<Portfolio> UpdateHoldingAsync(string portfolioId, string userId, Holding holding)
    {
        try
        {
            var portfolio = await _cosmosDbService.GetPortfolioAsync(portfolioId, userId);
            if (portfolio == null)
            {
                throw new InvalidOperationException("Portfolio not found");
            }

            var existingHolding = portfolio.Holdings.FirstOrDefault(h => h.Id == holding.Id);
            if (existingHolding == null)
            {
                throw new InvalidOperationException("Holding not found");
            }

            existingHolding.Quantity = holding.Quantity;
            existingHolding.AverageCostPrice = holding.AverageCostPrice;
            existingHolding.CurrentPrice = holding.CurrentPrice;
            existingHolding.LastUpdated = DateTime.UtcNow;

            return await _cosmosDbService.UpdatePortfolioAsync(portfolio);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating holding in portfolio: {PortfolioId}", portfolioId);
            throw;
        }
    }

    public async Task DeleteHoldingAsync(string portfolioId, string userId, string holdingId)
    {
        try
        {
            var portfolio = await _cosmosDbService.GetPortfolioAsync(portfolioId, userId);
            if (portfolio == null)
            {
                throw new InvalidOperationException("Portfolio not found");
            }

            var holding = portfolio.Holdings.FirstOrDefault(h => h.Id == holdingId);
            if (holding == null)
            {
                throw new InvalidOperationException("Holding not found");
            }

            portfolio.Holdings.Remove(holding);
            await _cosmosDbService.UpdatePortfolioAsync(portfolio);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting holding from portfolio: {PortfolioId}", portfolioId);
            throw;
        }
    }
}
