using System.Text.Json.Serialization;
using Biplott.Api.Middlewares;
using Biplott.Application;
using Biplott.Infrastructure;
using Biplott.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Services to DI Container
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// 2. Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<BiplottDbContext>("database", tags: new[] { "db", "sql", "ready" });

// 3. CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://127.0.0.1:3000",
                "http://localhost:80",
                "http://frontend:3000"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// 4. Global Exception Handling Middleware
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// 5. OpenAPI Endpoint (Enabled in Dev & Docker)
app.MapOpenApi();

// 6. Middlewares Pipeline
app.UseRouting();
app.UseCors("AllowFrontend");

// 7. Health Check Endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/healthz");

app.MapControllers();

// 8. Database Migration & Seeding on Startup with Retry Loop
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var dbContext = services.GetRequiredService<BiplottDbContext>();

    const int maxRetries = 10;
    var delay = TimeSpan.FromSeconds(3);

    for (int retry = 1; retry <= maxRetries; retry++)
    {
        try
        {
            logger.LogInformation("Attempting database connection and migration (Attempt {Retry}/{MaxRetries})...", retry, maxRetries);
            await dbContext.Database.MigrateAsync();
            await DbInitializer.SeedAsync(dbContext, logger);
            logger.LogInformation("Database migration and seeding completed successfully.");
            break;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Database connection attempt {Retry}/{MaxRetries} failed: {Message}. Retrying in {Delay}s...", retry, maxRetries, ex.Message, delay.TotalSeconds);
            if (retry == maxRetries)
            {
                logger.LogError(ex, "Could not connect to database after {MaxRetries} attempts.", maxRetries);
            }
            else
            {
                await Task.Delay(delay);
            }
        }
    }
}

app.Run();
