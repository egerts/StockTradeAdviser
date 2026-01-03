using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Functions.Worker;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Cosmos;
using StockTradeAdviser.Data.Services;
using StockTradeAdviser.Functions.Services;
using System.Threading.Tasks;

namespace StockTradeAdviser.Functions;

class Program
{
    static async Task Main(string[] args)
    {
        var host = new HostBuilder()
            .ConfigureFunctionsWorkerDefaults()
            .ConfigureServices(services =>
            {
                services.AddLogging(loggingBuilder =>
                {
                    loggingBuilder.AddConsole();
                });

                services.AddSingleton<CosmosClient>(serviceProvider =>
                {
                    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
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

                services.AddSingleton<ICosmosDbService, CosmosDbService>();

                services.AddSingleton<ServiceBusClient>(serviceProvider =>
                {
                    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                    var connectionString = configuration["ServiceBus:ConnectionString"];
                    if (string.IsNullOrEmpty(connectionString))
                    {
                        throw new InvalidOperationException("Service Bus connection string is missing");
                    }
                    return new ServiceBusClient(connectionString);
                });

                services.AddScoped<IStockDataService, StockDataService>();
                services.AddScoped<IRecommendationService, RecommendationService>();
                services.AddScoped<ITechnicalAnalysisService, TechnicalAnalysisService>();
            })
            .Build();

        await host.RunAsync();
    }
}
