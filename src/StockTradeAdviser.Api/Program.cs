using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.Azure.Cosmos;
using StockTradeAdviser.Api.Services;
using StockTradeAdviser.Data.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("MicrosoftEntra"));

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddSingleton<CosmosClient>(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var cosmosSection = configuration.GetSection("CosmosDb");
    var endpoint = cosmosSection.GetValue<string>("Endpoint");
    var key = cosmosSection.GetValue<string>("Key");
    
    if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(key))
    {
        throw new InvalidOperationException("Cosmos DB configuration is missing. Please set CosmosDb:Endpoint and CosmosDb:Key in configuration.");
    }

    var cosmosClientOptions = new CosmosClientOptions
    {
        ApplicationName = "StockTradeAdviser.Api",
        ConnectionMode = ConnectionMode.Direct,
        MaxRetryAttemptsOnRateLimitedRequests = 10,
        MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(30)
    };

    return new CosmosClient(endpoint, key, cosmosClientOptions);
});

builder.Services.AddSingleton<ICosmosDbService, CosmosDbService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPortfolioService, PortfolioService>();
builder.Services.AddScoped<IRecommendationService, MockRecommendationService>();
builder.Services.AddScoped<ITransactionService, MockTransactionService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowReactApp");

app.UseAuthentication();
app.UseAuthorization();

// Add global exception handler to prevent CORS issues on errors
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        
        var exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
        var response = new
        {
            error = "Internal server error",
            message = exception?.Message ?? "An unexpected error occurred"
        };
        
        await context.Response.WriteAsJsonAsync(response);
    });
});

app.MapControllers();

// Add a simple root endpoint
app.MapGet("/", () => Results.Json(new {
    message = "StockTradeAdviser API",
    version = "1.0.0",
    status = "Running",
    endpoints = new {
        swagger = "/swagger",
        auth = "/api/auth"
    }
}));

app.Run();
