using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;
using StockTradeAdviser.Api.Services;
using StockTradeAdviser.Data.Services;
using Microsoft.Azure.Cosmos;

var builder = WebApplication.CreateBuilder(args);

// Temporarily disable authentication for testing
// builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//     .AddMicrosoftIdentityJwtApi(builder.Configuration.GetSection("AzureAd"));

// builder.Services.AddAuthorization(options =>
// {
//     options.FallbackPolicy = options.DefaultPolicy;
// });

builder.Services.AddRazorPages();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.SetIsOriginAllowed(origin => true) // Allow any origin
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .WithExposedHeaders("*"); // Expose all headers
    });
});

// Temporarily disable Cosmos DB for testing
// builder.Services.AddSingleton<CosmosClient>(serviceProvider =>
// {
//     var configuration = serviceProvider.GetRequiredService<IConfiguration>();
//     var endpoint = configuration["CosmosDb:Endpoint"];
//     var key = configuration["CosmosDb:Key"];
//     
//     if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(key))
//     {
//         throw new InvalidOperationException("Cosmos DB configuration is missing");
//     }

//     var cosmosClientOptions = new CosmosClientOptions
//     {
//         ApplicationName = "StockTradeAdviser.Api",
//         ConnectionMode = ConnectionMode.Direct,
//         MaxRetryAttemptsOnRateLimitedRequests = 10,
//         MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(30)
//     };

//     return new CosmosClient(endpoint, key, cosmosClientOptions);
// });

// builder.Services.AddSingleton<ICosmosDbService, CosmosDbService>();
builder.Services.AddScoped<IUserService, MockUserService>();
// builder.Services.AddScoped<IPortfolioService, PortfolioService>();
builder.Services.AddScoped<IPortfolioService, MockPortfolioService>();
// builder.Services.AddScoped<IRecommendationService, RecommendationService>();
builder.Services.AddScoped<IRecommendationService, MockRecommendationService>();
builder.Services.AddScoped<ITransactionService, MockTransactionService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); // Disabled for testing
app.UseCors("AllowReactApp");

// app.UseAuthentication();
// app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

// Add a simple root endpoint
app.MapGet("/", () => Results.Json(new {
    message = "StockTradeAdviser API",
    version = "1.0.0",
    status = "Running",
    endpoints = new {
        swagger = "/swagger",
        auth = "/api/auth",
        weather = "/weatherforecast"
    }
}));

app.Run();
