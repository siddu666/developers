using ExchangeRateUpdater.Application;
using ExchangeRateUpdater.Infrastructure;
using Prometheus;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Health checks
builder.Services.AddHealthChecks()
    .AddUrlGroup(
        new Uri(builder.Configuration["ExchangeRateProvider:CnbHealthUrl"] 
                ?? "https://www.cnb.cz/en/financial-markets/foreign-exchange-market/central-bank-exchange-rate-fixing/central-bank-exchange-rate-fixing/daily.txt"), 
        name: "cnb")
    .AddRedis(
        builder.Configuration["Redis:Configuration"], 
        name: "redis", 
        failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded, 
        tags: new[] { "ready" });

// JWT Auth (toggle via config)
var jwtEnabled = builder.Configuration.GetValue<bool>("Jwt:Enabled", false);
if (jwtEnabled)
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.Audience = builder.Configuration["Jwt:Audience"];
            options.Authority = builder.Configuration["Jwt:Authority"];
            // Additional token validation parameters can be added here
        });
}

// Add logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add application and infrastructure services
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

// Metrics
app.UseHttpMetrics();

// Simple API key middleware
var apiKeyEnabled = builder.Configuration.GetValue<bool>("ApiKey:Enabled", false);
var apiKeyValue = builder.Configuration["ApiKey:Value"] ?? string.Empty;
if (apiKeyEnabled)
{
    app.Use(async (context, next) =>
    {
        if (context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
        {
            if (!context.Request.Headers.TryGetValue("X-API-KEY", out var key) || string.IsNullOrEmpty(apiKeyValue) || key != apiKeyValue)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }
        }
        await next();
    });
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection(); // only for production
}

if (jwtEnabled)
{
    app.UseAuthentication();
}

app.UseAuthorization();

app.MapControllers();

// Health checks endpoint
app.MapHealthChecks("/health");

// Prometheus scrape endpoint
app.MapMetrics();

app.Run();

public partial class Program { }
