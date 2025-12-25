using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StockTradeAdviser.Core.Models;

namespace StockTradeAdviser.Data.Services;

public class CosmosDbService : ICosmosDbService
{
    private readonly CosmosClient _cosmosClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CosmosDbService> _logger;
    private readonly string _databaseName;
    private readonly Container _usersContainer;
    private readonly Container _portfoliosContainer;
    private readonly Container _stocksContainer;
    private readonly Container _recommendationsContainer;
    private readonly Container _recommendationHistoryContainer;

    public CosmosDbService(
        CosmosClient cosmosClient,
        IConfiguration configuration,
        ILogger<CosmosDbService> logger)
    {
        _cosmosClient = cosmosClient;
        _configuration = configuration;
        _logger = logger;
        _databaseName = _configuration["CosmosDb:DatabaseName"] ?? "StockTradeAdviser";
        
        _usersContainer = _cosmosClient.GetContainer(_databaseName, "users");
        _portfoliosContainer = _cosmosClient.GetContainer(_databaseName, "portfolios");
        _stocksContainer = _cosmosClient.GetContainer(_databaseName, "stocks");
        _recommendationsContainer = _cosmosClient.GetContainer(_databaseName, "recommendations");
        _recommendationHistoryContainer = _cosmosClient.GetContainer(_databaseName, "recommendationHistory");
    }

    public async Task<StockTradeAdviser.Core.Models.User?> GetUserAsync(string userId)
    {
        try
        {
            var response = await _usersContainer.ReadItemAsync<StockTradeAdviser.Core.Models.User>(userId, new PartitionKey(userId));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<StockTradeAdviser.Core.Models.User?> GetUserByAzureAdObjectIdAsync(string azureAdObjectId)
    {
        try
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.azureAdObjectId = @azureAdObjectId")
                .WithParameter("@azureAdObjectId", azureAdObjectId);
            
            var iterator = _usersContainer.GetItemQueryIterator<StockTradeAdviser.Core.Models.User>(query);
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                if (response.Any())
                {
                    return response.First();
                }
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by Azure AD object ID: {AzureAdObjectId}", azureAdObjectId);
            throw;
        }
    }

    public async Task<StockTradeAdviser.Core.Models.User> CreateUserAsync(StockTradeAdviser.Core.Models.User user)
    {
        user.Id = Guid.NewGuid().ToString();
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        
        var response = await _usersContainer.CreateItemAsync(user, new PartitionKey(user.Id));
        return response.Resource;
    }

    public async Task<StockTradeAdviser.Core.Models.User> UpdateUserAsync(StockTradeAdviser.Core.Models.User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        var response = await _usersContainer.UpsertItemAsync(user, new PartitionKey(user.Id));
        return response.Resource;
    }

    public async Task DeleteUserAsync(string userId)
    {
        await _usersContainer.DeleteItemAsync<StockTradeAdviser.Core.Models.User>(userId, new PartitionKey(userId));
    }

    public async Task<Portfolio?> GetPortfolioAsync(string portfolioId, string userId)
    {
        try
        {
            var response = await _portfoliosContainer.ReadItemAsync<Portfolio>(portfolioId, new PartitionKey(userId));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<List<Portfolio>> GetUserPortfoliosAsync(string userId)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.userId = @userId")
            .WithParameter("@userId", userId);
        
        var portfolios = new List<Portfolio>();
        var iterator = _portfoliosContainer.GetItemQueryIterator<Portfolio>(query);
        
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            portfolios.AddRange(response);
        }
        
        return portfolios;
    }

    public async Task<Portfolio> CreatePortfolioAsync(Portfolio portfolio)
    {
        portfolio.Id = Guid.NewGuid().ToString();
        portfolio.CreatedAt = DateTime.UtcNow;
        portfolio.UpdatedAt = DateTime.UtcNow;
        
        var response = await _portfoliosContainer.CreateItemAsync(portfolio, new PartitionKey(portfolio.UserId));
        return response.Resource;
    }

    public async Task<Portfolio> UpdatePortfolioAsync(Portfolio portfolio)
    {
        portfolio.UpdatedAt = DateTime.UtcNow;
        var response = await _portfoliosContainer.UpsertItemAsync(portfolio, new PartitionKey(portfolio.UserId));
        return response.Resource;
    }

    public async Task DeletePortfolioAsync(string portfolioId, string userId)
    {
        await _portfoliosContainer.DeleteItemAsync<Portfolio>(portfolioId, new PartitionKey(userId));
    }

    public async Task<StockData?> GetStockDataAsync(string symbol)
    {
        try
        {
            var response = await _stocksContainer.ReadItemAsync<StockData>(symbol.ToUpper(), new PartitionKey(symbol.ToUpper()));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<List<StockData>> GetMultipleStockDataAsync(List<string> symbols)
    {
        var stockDataList = new List<StockData>();
        var tasks = symbols.Select(async symbol =>
        {
            var stockData = await GetStockDataAsync(symbol);
            return stockData;
        });

        var results = await Task.WhenAll(tasks);
        stockDataList.AddRange(results.Where(data => data != null)!);
        
        return stockDataList;
    }

    public async Task<StockData> UpdateStockDataAsync(StockData stockData)
    {
        stockData.Id = stockData.Symbol.ToUpper();
        stockData.Timestamp = DateTime.UtcNow;
        
        var response = await _stocksContainer.UpsertItemAsync(stockData, new PartitionKey(stockData.Id));
        return response.Resource;
    }

    public async Task<Recommendation?> GetRecommendationAsync(string recommendationId, string userId)
    {
        try
        {
            var response = await _recommendationsContainer.ReadItemAsync<Recommendation>(recommendationId, new PartitionKey(userId));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<List<Recommendation>> GetUserRecommendationsAsync(string userId, int limit = 50)
    {
        var query = new QueryDefinition("SELECT TOP @limit * FROM c WHERE c.userId = @userId ORDER BY c.createdAt DESC")
            .WithParameter("@userId", userId)
            .WithParameter("@limit", limit);
        
        var recommendations = new List<Recommendation>();
        var iterator = _recommendationsContainer.GetItemQueryIterator<Recommendation>(query);
        
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            recommendations.AddRange(response);
        }
        
        return recommendations;
    }

    public async Task<List<Recommendation>> GetActiveRecommendationsAsync(string userId)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.userId = @userId AND c.status = 'Active' ORDER BY c.createdAt DESC")
            .WithParameter("@userId", userId);
        
        var recommendations = new List<Recommendation>();
        var iterator = _recommendationsContainer.GetItemQueryIterator<Recommendation>(query);
        
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            recommendations.AddRange(response);
        }
        
        return recommendations;
    }

    public async Task<Recommendation> CreateRecommendationAsync(Recommendation recommendation)
    {
        recommendation.Id = Guid.NewGuid().ToString();
        recommendation.CreatedAt = DateTime.UtcNow;
        
        var response = await _recommendationsContainer.CreateItemAsync(recommendation, new PartitionKey(recommendation.UserId));
        return response.Resource;
    }

    public async Task<Recommendation> UpdateRecommendationAsync(Recommendation recommendation)
    {
        var response = await _recommendationsContainer.UpsertItemAsync(recommendation, new PartitionKey(recommendation.UserId));
        return response.Resource;
    }

    public async Task<List<RecommendationHistory>> GetRecommendationHistoryAsync(string userId, int limit = 100)
    {
        var query = new QueryDefinition("SELECT TOP @limit * FROM c WHERE c.userId = @userId ORDER BY c.createdAt DESC")
            .WithParameter("@userId", userId)
            .WithParameter("@limit", limit);
        
        var history = new List<RecommendationHistory>();
        var iterator = _recommendationHistoryContainer.GetItemQueryIterator<RecommendationHistory>(query);
        
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            history.AddRange(response);
        }
        
        return history;
    }

    public async Task<RecommendationHistory> CreateRecommendationHistoryAsync(RecommendationHistory history)
    {
        history.Id = Guid.NewGuid().ToString();
        history.CreatedAt = DateTime.UtcNow;
        
        var response = await _recommendationHistoryContainer.CreateItemAsync(history, new PartitionKey(history.UserId));
        return response.Resource;
    }
}
