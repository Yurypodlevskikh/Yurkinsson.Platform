using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Sinks.MariaDB.Extensions;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using YurkinssonAuthentication.Configurations;
using YurkinssonAuthentication.Data;
using YurkinssonAuthentication.DTOs;
using YurkinssonAuthentication.Services;
using YurkinssonAuthentication.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Use Serilog as the Logging provider
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Setting up a security protocol. Allow support to TLS 1.2
ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

// Add appsettings.json, environment variables
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory()) // Specifies the root directory
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

//// Databases configuration
var connectionString = builder.Configuration.GetConnectionString("MariaDbIdentity");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);
//builder.Configuration["ConnectionStrings:MariaDbIdentity"],
//        //new MySqlServerVersion(new Version(10, 5, 25))
//        new MySqlServerVersion(new Version(8, 0, 30))
//    ));

builder.Services.AddDataProtection()
    .PersistKeysToDbContext<AppDbContext>()
    .SetApplicationName("YurkinssonAuthentication");

//builder.Services.AddDbContext<AppDbContext>(options =>
//options.UseMySql(builder.Configuration["ConnectionStrings:MariaDbIdentity"], ServerVersion.AutoDetect(builder.Configuration["ConnectionStrings:MariaDbIdentity"])));
//builder.Host.UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration));

// Setting up Identity
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireDigit = false;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(20);
    options.Lockout.MaxFailedAccessAttempts = 5;
}).AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
options.TokenLifespan = TimeSpan.FromHours(2));

// Setting up Jwt authentication
var audiences = builder.Configuration["JwtYurkIdentity:Audiences"]
    ?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();

// Setting up controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    //options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["JwtYurkIdentity:Issuer"],
            ValidateAudience = true,
            ValidAudiences = audiences,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtYurkIdentity:Key"]))
        };
        //options.Events = new JwtBearerEvents
        //{
        //    OnMessageReceived = context =>
        //    {
        //        Console.WriteLine($"Token received: {context.Token}");
        //        return Task.CompletedTask;
        //    },
        //    OnTokenValidated = context =>
        //    {
        //        Console.WriteLine("Token validated.");
        //        return Task.CompletedTask;
        //    },
        //    OnAuthenticationFailed = context =>
        //    {
        //        Console.WriteLine($"Authentication failed: {context.Exception.Message}");
        //        return Task.CompletedTask;
        //    }
        //};
    });
// Add Authorization
builder.Services.AddAuthorization();

string[]? origins = builder.Configuration["RegisteredApplications"]?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
// Setting up CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowedOrigins", builder =>
    {
        builder.WithOrigins(origins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials(); // Allows the use of cookies/authorization.
    });
});

// Connecting services.
builder.Services.AddTransient<IAccountService, AccountService>();

builder.Services.AddSingleton<IEnumerable<string>>(audiences);
builder.Services.AddScoped<IAudienceService, AudienceService>();

builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpNoreplYurkiHost"));
builder.Services.AddTransient<IEmailSender, EmailService>();

// Connecting Rate Limiting
builder.Services.AddRateLimitingPolicies();

// Setting up Swagger
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo { Title = "YurkinssonAuthentication", Version = "v1" });
//    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//    {
//        Name = "Authorization",
//        Type = SecuritySchemeType.Http,
//        Scheme = "bearer",
//        BearerFormat = "JWT",
//        In = ParameterLocation.Header,
//        Description = "Enter the token in the format: {token}."
//    });
//    // Add a filter to automatically apply security to all operations
//    c.AddSecurityRequirement(new OpenApiSecurityRequirement
//    {
//        {
//            new OpenApiSecurityScheme
//            {
//                Reference = new OpenApiReference
//                {
//                    Type = ReferenceType.SecurityScheme,
//                    Id = "Bearer"
//                }
//            },
//            Array.Empty<string>()
//        }
//    });
//});
builder.Services.AddHostedService<DatabaseInitializerHostedService>();

var app = builder.Build();
// Log HTTP requests
app.UseSerilogRequestLogging();

//app.UseExceptionHandler(appBuilder =>
//{
//    appBuilder.Run(async context =>
//    {
//        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
//        if (exceptionHandlerPathFeature?.Error != null)
//        {
//            context.Response.StatusCode = 500;
//            context.Response.ContentType = "application/json";
//            await context.Response.WriteAsync($"{{\"error\": \"{exceptionHandlerPathFeature.Error.Message}\"}}");
//        }
//    });
//});
//app.UseDeveloperExceptionPage();
// Configure the HTTP request pipeline.цд
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

// Enable Rate Limiting Middleware
app.UseRateLimiter();
app.UseHttpsRedirection();
app.UseCors("AllowedOrigins");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    var logger = services.GetRequiredService<ILogger<Program>>();
//    try
//    {
//        logger.LogInformation("Attempt to apply migration...");
//        var dbContext = services.GetRequiredService<AppDbContext>();
//        dbContext.Database.Migrate();
//        logger.LogInformation("Database migrations applied successfully.");
//    }
//    catch (Exception ex)
//    {
        
//        logger.LogError(ex, "Error initializing database.");
//        throw;
//    }
//}

app.Run();