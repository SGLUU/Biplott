using System.Text.Json.Serialization;
using Biplott.Api.Middlewares;
using Biplott.Application;
using Biplott.Infrastructure;
using Biplott.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Services to DI Container
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// 2. JWT Authentication & Authorization
var secretKey = builder.Configuration["Jwt:SecretKey"]
    ?? "BiplotSuperSecretKeyForDevelopmentPhase3_MustBeAtLeast32BytesLong!";
var issuer = builder.Configuration["Jwt:Issuer"] ?? "BiplottApi";
var audience = builder.Configuration["Jwt:Audience"] ?? "BiplottClient";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// 3. Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<BiplottDbContext>("database", tags: new[] { "db", "sql", "ready" });

// 4. CORS Policy
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

// 5. Global Exception Handling Middleware
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// 6. OpenAPI Endpoint (Enabled in Dev & Docker)
app.MapOpenApi();

// 7. Middlewares Pipeline
app.UseRouting();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

// 8. Health Check Endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/healthz");

app.MapControllers();

// 9. Database Migration & Seeding on Startup with Retry Loop
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
            await DbInitializer.SeedRolesAndAdminAsync(services, logger);
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
