using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Cosmos;
using StockTradeAdviser.Data.Services;

[assembly: FunctionsStartup(typeof(StockTradeAdviser.Functions.Startup))]

namespace StockTradeAdviser.Functions;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        var context = builder.GetContext();
        var configuration = context.Configuration;

        builder.Services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddConsole();
        });

        builder.Services.AddSingleton<CosmosClient>(serviceProvider =>
        {
            var endpoint = configuration["CosmosDb:Endpoint"];
            var key = configuration["CosmosDb:Key"];
            
            if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(key))
            {
                throw new InvalidOperationException("Cosmos DB configuration is missing");
            }

            var cosmosClientOptions = new CosmosClientOptions
            {
                ApplicationName = "StockTradeAdviser",
                ConnectionMode = ConnectionMode.Direct,
                MaxRetryAttemptsOnRateLimitedRequests = 10,
                MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(30)
            };

            return new CosmosClient(endpoint, key, cosmosClientOptions);
        });

        builder.Services.AddSingleton<ICosmosDbService, CosmosDbService>();

        builder.Services.AddSingleton<ServiceBusClient>(serviceProvider =>
        {
            var connectionString = configuration["ServiceBus:ConnectionString"];
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Service Bus connection string is missing");
            }
            return new ServiceBusClient(connectionString);
        });

        builder.Services.AddScoped<IStockDataService, StockDataService>();
        builder.Services.AddScoped<IRecommendationService, RecommendationService>();
        builder.Services.AddScoped<ITechnicalAnalysisService, TechnicalAnalysisService>();
    }
}
