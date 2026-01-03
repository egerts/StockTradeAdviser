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

    public CosmosDbService(
        CosmosClient cosmosClient,
        IConfiguration configuration,
        ILogger<CosmosDbService> logger)
    {
        _cosmosClient = cosmosClient;
        _configuration = configuration;
        _logger = logger;
        _databaseName = _configuration["CosmosDb:DatabaseName"] ?? throw new ArgumentNullException("CosmosDb:DatabaseName");
    }

    private Container GetUsersContainer() => _cosmosClient.GetContainer(_databaseName, "users");
    private Container GetPortfoliosContainer() => _cosmosClient.GetContainer(_databaseName, "portfolios");
    private Container GetStocksContainer() => _cosmosClient.GetContainer(_databaseName, "stocks");
    private Container GetRecommendationsContainer() => _cosmosClient.GetContainer(_databaseName, "recommendations");
    private Container GetRecommendationHistoryContainer() => _cosmosClient.GetContainer(_databaseName, "recommendationHistory");

    public async Task<StockTradeAdviser.Core.Models.User?> GetUserAsync(string userId)
    {
        try
        {
            var response = await GetUsersContainer().ReadItemAsync<StockTradeAdviser.Core.Models.User>(userId, new PartitionKey(userId));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<StockTradeAdviser.Core.Models.User?> GetUserByEntraObjectIdAsync(string entraObjectId)
    {
        try
        {
            _logger.LogInformation("GetUserByEntraObjectIdAsync called for: {EntraObjectId}", entraObjectId);
            
            var query = new QueryDefinition("SELECT * FROM c WHERE c.entraObjectId = @entraObjectId")
                .WithParameter("@entraObjectId", entraObjectId);
            
            _logger.LogInformation("Executing query against users container");
            var iterator = GetUsersContainer().GetItemQueryIterator<StockTradeAdviser.Core.Models.User>(query);
            
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                if (response.Count != 0)
                {
                    _logger.LogInformation("Found user with ID: {UserId}", response.First().Id);
                    return response.First();
                }
            }
            
            _logger.LogInformation("No user found for EntraObjectId: {EntraObjectId}", entraObjectId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by Entra object ID: {EntraObjectId}. Exception: {ExceptionType}, Message: {Message}", 
                entraObjectId, ex.GetType().Name, ex.Message);
            throw;
        }
    }

    public async Task<StockTradeAdviser.Core.Models.User> CreateUserAsync(StockTradeAdviser.Core.Models.User user)
    {
        try
        {
            _logger.LogInformation("CreateUserAsync called for user with EntraObjectId: {EntraObjectId}", user.EntraObjectId);
            
            // Create a minimal user object with only required properties
            var minimalUser = new
            {
                id = Guid.NewGuid().ToString(),
                email = user.Email ?? "test@example.com",
                displayName = user.DisplayName ?? "Test User",
                entraObjectId = user.EntraObjectId,
                createdAt = DateTime.UtcNow,
                updatedAt = DateTime.UtcNow
            };
            
            _logger.LogInformation("Created minimal user object with ID: {UserId}", minimalUser.id);
            
            // Serialize the minimal user to JSON
            var userJson = System.Text.Json.JsonSerializer.Serialize(minimalUser);
            _logger.LogInformation("Minimal user JSON being sent to Cosmos DB: {UserJson}", userJson);
            
            _logger.LogInformation("Getting users container and creating item");
            
            // Use the minimal user object for Cosmos DB
            var response = await GetUsersContainer().CreateItemAsync(minimalUser, new PartitionKey(minimalUser.id));
            
            _logger.LogInformation("Successfully created user in Cosmos DB with ID: {UserId}", response.Resource.id);
            
            // Convert back to full User object
            var fullUser = new StockTradeAdviser.Core.Models.User
            {
                Id = minimalUser.id,
                Email = minimalUser.email,
                DisplayName = minimalUser.displayName,
                EntraObjectId = minimalUser.entraObjectId,
                CreatedAt = minimalUser.createdAt,
                UpdatedAt = minimalUser.updatedAt
            };
            
            return fullUser;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user in Cosmos DB. Exception: {ExceptionType}, Message: {Message}", 
                ex.GetType().Name, ex.Message);
            throw;
        }
    }

    public async Task<StockTradeAdviser.Core.Models.User> UpdateUserAsync(StockTradeAdviser.Core.Models.User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        var response = await GetUsersContainer().UpsertItemAsync(user, new PartitionKey(user.Id));
        return response.Resource;
    }

    public async Task DeleteUserAsync(string userId)
    {
        await GetUsersContainer().DeleteItemAsync<StockTradeAdviser.Core.Models.User>(userId, new PartitionKey(userId));
    }

    public async Task<Portfolio?> GetPortfolioAsync(string portfolioId, string userId)
    {
        try
        {
            var response = await GetPortfoliosContainer().ReadItemAsync<Portfolio>(portfolioId, new PartitionKey(userId));
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
        var iterator = GetPortfoliosContainer().GetItemQueryIterator<Portfolio>(query);
        
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
        
        var response = await GetPortfoliosContainer().CreateItemAsync(portfolio, new PartitionKey(portfolio.UserId));
        return response.Resource;
    }

    public async Task<Portfolio> UpdatePortfolioAsync(Portfolio portfolio)
    {
        portfolio.UpdatedAt = DateTime.UtcNow;
        var response = await GetPortfoliosContainer().UpsertItemAsync(portfolio, new PartitionKey(portfolio.UserId));
        return response.Resource;
    }

    public async Task DeletePortfolioAsync(string portfolioId, string userId)
    {
        await GetPortfoliosContainer().DeleteItemAsync<Portfolio>(portfolioId, new PartitionKey(userId));
    }

    public async Task<StockData?> GetStockDataAsync(string symbol)
    {
        try
        {
            var response = await GetStocksContainer().ReadItemAsync<StockData>(symbol.ToUpper(), new PartitionKey(symbol.ToUpper()));
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
        
        var response = await GetStocksContainer().UpsertItemAsync(stockData, new PartitionKey(stockData.Id));
        return response.Resource;
    }

    public async Task<Recommendation?> GetRecommendationAsync(string recommendationId, string userId)
    {
        try
        {
            var response = await GetRecommendationsContainer().ReadItemAsync<Recommendation>(recommendationId, new PartitionKey(userId));
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
        var iterator = GetRecommendationsContainer().GetItemQueryIterator<Recommendation>(query);
        
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
        var iterator = GetRecommendationsContainer().GetItemQueryIterator<Recommendation>(query);
        
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
        
        var response = await GetRecommendationsContainer().CreateItemAsync(recommendation, new PartitionKey(recommendation.UserId));
        return response.Resource;
    }

    public async Task<Recommendation> UpdateRecommendationAsync(Recommendation recommendation)
    {
        var response = await GetRecommendationsContainer().UpsertItemAsync(recommendation, new PartitionKey(recommendation.UserId));
        return response.Resource;
    }

    public async Task<List<RecommendationHistory>> GetRecommendationHistoryAsync(string userId, int limit = 100)
    {
        var query = new QueryDefinition("SELECT TOP @limit * FROM c WHERE c.userId = @userId ORDER BY c.createdAt DESC")
            .WithParameter("@userId", userId)
            .WithParameter("@limit", limit);
        
        var history = new List<RecommendationHistory>();
        var iterator = GetRecommendationHistoryContainer().GetItemQueryIterator<RecommendationHistory>(query);
        
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
        
        var response = await GetRecommendationHistoryContainer().CreateItemAsync(history, new PartitionKey(history.UserId));
        return response.Resource;
    }
}
