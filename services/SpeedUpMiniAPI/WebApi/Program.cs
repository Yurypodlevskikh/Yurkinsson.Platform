using DataAccess.Extensions;
using BusinessLogic.Extensions;
using WebApi.Configuration;
using WebApi.Endpoints;
using WebApi.Filters;
using WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
var connectionString = builder.Configuration["ConnectionStrings:MariaDbSpeedUp"];

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'MariaDbSpeedUp' is not configured.");
}

builder.Services.AddDataAccess(connectionString);
builder.Services.AddBusinessLogic(builder.Configuration);
builder.Services.AddTransient<TokenValidationFilter>();
builder.Services.AddRateLimitingPolicies();

var allowedOrigins = builder.Configuration["AllowedOrigins"]?.Split(",");
builder.Services.AddCors(options =>
{
    if(allowedOrigins is null || allowedOrigins.Length == 0)
    {
        throw new InvalidOperationException("Allowed origins are not configured.");
    }
    
    options.AddPolicy("DefaultCorsPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Start migrations when on startup
builder.Services.AddHostedService<MigrationHostedService>();

var app = builder.Build();

// Enable Swagger only in development
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Enable HTTPS redirection
app.UseHttpsRedirection();

// Enable CORS before anything that handles requests
app.UseCors("DefaultCorsPolicy");

// Optional: Handle OPTIONS requests manually (if needed)
app.Use(async (context, next) =>
{
    if(context.Request.Method == "OPTIONS")
    {
        context.Response.StatusCode = 200;
        await context.Response.CompleteAsync();
    }
    else
    {
        await next(context);
    }
});

// Rate limiting
app.UseRateLimiter();

// Endpoints
app.MapIdentityEndpoints();
app.MapMetronomeSettingsEndpoints();

app.Run();
