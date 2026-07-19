using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Application.Services;
using ExpenseTracker.Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using ExpenseTracker.Api.ExceptionHandling;
using ExpenseTracker.Api.Services;
using ExpenseTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Api.BackgroundServices;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;
using Asp.Versioning;
using ExpenseTracker.Api.Middleware;
using ExpenseTracker.Infrastructure.Identity;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.DataProtection;
using System.IO.Compression;
using Microsoft.AspNetCore.ResponseCompression;
using ExpenseTracker.Api.Services.Momo;


var builder = WebApplication.CreateBuilder(args);

// Register application services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});
builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

if (allowedOrigins.Length > 0)
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("WebClient", policy =>
        {
            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });
}
builder.Services.AddDataProtection()
    .SetApplicationName("ExpenseTracker.Pro");

var jwtSection = builder.Configuration.GetSection(
    JwtSettings.SectionName);
var jwtSettings = jwtSection.Get<JwtSettings>() ??
    throw new InvalidOperationException(
        "The JWT configuration section is missing.");

ValidateJwtSettings(jwtSettings);
builder.Services.Configure<JwtSettings>(jwtSection);
builder.Services.Configure<GoogleAuthSettings>(
    builder.Configuration.GetSection(GoogleAuthSettings.SectionName));
builder.Services.Configure<TwilioVerifySettings>(
    builder.Configuration.GetSection(TwilioVerifySettings.SectionName));
builder.Services.Configure<SendGridSettings>(
    builder.Configuration.GetSection(SendGridSettings.SectionName));
builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion =
            new ApiVersion(1, 0);

        options.AssumeDefaultVersionWhenUnspecified =
            true;

        options.ReportApiVersions =
            true;

        options.ApiVersionReader =
            new UrlSegmentApiVersionReader();
    })
    .AddMvc()
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";

        options.SubstituteApiVersionInUrl =
            true;
    });

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Version = "v1",
            Title = "ExpenseTracker Pro API",
            Description =
                "REST API for personal finance management, including " +
                "accounts, transactions, transfers, budgets, recurring " +
                "transactions, financial goals, reports, notifications, " +
                "and dashboard analytics.",

            Contact = new OpenApiContact
            {
                Name = "ExpenseTracker Pro Development Team"
            },

            License = new OpenApiLicense
            {
                Name = "Private Development Project"
            }
        });

    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description =
                "Enter the JWT access token returned by the login endpoint. " +
                "Do not include the word 'Bearer'; Swagger adds it automatically."
        });

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type =
                            ReferenceType.SecurityScheme,

                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });

    var xmlFile =
        $"{typeof(Program).Assembly.GetName().Name}.xml";

    var xmlPath =
        Path.Combine(
            AppContext.BaseDirectory,
            xmlFile);

    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    options.CustomSchemaIds(type =>
        type.FullName?.Replace("+", ".")
        ?? type.Name);

    options.OrderActionsBy(apiDescription =>
        $"{apiDescription.ActionDescriptor.RouteValues["controller"]}_" +
        $"{apiDescription.HttpMethod}_" +
        $"{apiDescription.RelativePath}");
});

builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddInfrastructureServices(builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection is missing."));
builder.Services.AddScoped<IGoogleIdentityVerifier, GoogleIdentityVerifier>();
builder.Services.AddHttpClient<
    IMobileVerificationService,
    TwilioMobileVerificationService>(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(15);
    });
builder.Services.AddHttpClient<
    IPasswordResetEmailSender,
    SendGridPasswordResetEmailSender>(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(15);
    });

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme =JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,

                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.Key)),

                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
    });

builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    options.GlobalLimiter =
        PartitionedRateLimiter.Create<HttpContext, string>(context =>
            RateLimitPartition.GetFixedWindowLimiter(
                GetRateLimitPartitionKey(context),
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 120,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }));

    options.AddPolicy("authentication", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            GetClientPartitionKey(context),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.AddPolicy("token", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            GetClientPartitionKey(context),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.AddPolicy("verification", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            GetClientPartitionKey(context),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 3,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.AddPolicy("momo", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            GetRateLimitPartitionKey(context),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.OnRejected = async (rejectedContext, cancellationToken) =>
    {
        rejectedContext.HttpContext.Response.ContentType =
            "application/problem+json";

        await rejectedContext.HttpContext.Response.WriteAsJsonAsync(
            new
            {
                type = "https://httpstatuses.com/429",
                title = "Too many requests",
                status = StatusCodes.Status429TooManyRequests,
                detail = "Please wait before trying again.",
                traceId = rejectedContext.HttpContext.TraceIdentifier
            },
            cancellationToken);
    };
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService,CurrentUserService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IBudgetService, BudgetService>();
builder.Services.AddScoped<IRecurringTransactionService, RecurringTransactionService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ITransferService,TransferService>();
builder.Services.AddScoped<IFinancialGoalService,FinancialGoalService>();
builder.Services.AddScoped<INotificationService,NotificationService>();
builder.Services.AddScoped<INotificationEngineService, NotificationEngineService>();
builder.Services.AddScoped<IAuditTrailService, AuditTrailService>();
builder.Services.AddSingleton<MomoConversationEngine>();
builder.Services.AddScoped<IMomoAssistantService, MomoAssistantService>();
builder.Services.AddHostedService<ExpenseTrackerBackgroundService>();
builder.Services.AddScoped<ISystemBackgroundProcessor, SystemBackgroundProcessor>();
builder.Services.AddEndpointsApiExplorer();

builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<ExpenseTrackerDbContext>(
        name: "sql-server",
        failureStatus:
            Microsoft.Extensions.Diagnostics.HealthChecks
                .HealthStatus.Unhealthy,
        tags: ["database", "ready"]);

var app = builder.Build();
app.UseResponseCompression();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<AuditLogMiddleware>();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    var context =
        scope.ServiceProvider
            .GetRequiredService<ExpenseTrackerDbContext>();

    await context.Database.MigrateAsync();

    if (builder.Configuration.GetValue<bool>(
        "DevelopmentSeeder:Enabled"))
    {
        var seeder = scope.ServiceProvider
            .GetRequiredService<DevelopmentDataSeeder>();

        await seeder.SeedAsync();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "ExpenseTracker Pro API v1");

        options.DocumentTitle =
            "ExpenseTracker Pro API Documentation";

        options.DisplayRequestDuration();

        options.EnableDeepLinking();

        options.EnableFilter();

        options.ShowExtensions();
    });
}


if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Cache-Control"] = "no-store";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    context.Response.Headers["Permissions-Policy"] =
        "camera=(), microphone=(), geolocation=()";

    await next();
});

app.UseRouting();
if (allowedOrigins.Length > 0)
{
    app.UseCors("WebClient");
}
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

app.MapHealthChecks(
        "/health/live",
        new HealthCheckOptions
        {
            Predicate = _ => false
        })
    .WithTags("Health Checks")
    .WithOpenApi(operation =>
    {
        operation.Summary = "Check API liveness";
        operation.Description =
            "Confirms that the ExpenseTracker API process is running.";

        return operation;
    });

app.MapHealthChecks(
        "/health/ready",
        new HealthCheckOptions
        {
            Predicate = check =>
                check.Tags.Contains("ready"),

            ResponseWriter = WriteHealthResponse
        })
    .WithTags("Health Checks")
    .WithOpenApi(operation =>
    {
        operation.Summary = "Check API readiness";
        operation.Description =
            "Confirms that the API and SQL Server database are available.";

        return operation;
    });

app.MapControllers();

app.Run();

static async Task WriteHealthResponse(
    HttpContext context,
    HealthReport report)
{
    context.Response.ContentType =
        "application/json";

    var response = new
    {
        status = report.Status.ToString(),

        totalDuration =
            report.TotalDuration.TotalMilliseconds,

        checks = report.Entries.Select(entry =>
            new
            {
                name = entry.Key,
                status =
                    entry.Value.Status.ToString(),
                description =
                    entry.Value.Description,
                duration =
                    entry.Value.Duration.TotalMilliseconds,
                error = entry.Value.Exception is null
                    ? null
                    : "The health check failed."
            }),

        timestamp = DateTime.UtcNow
    };

    await context.Response.WriteAsync(
        JsonSerializer.Serialize(
            response,
            new JsonSerializerOptions
            {
                WriteIndented = true
            }));
}

static string GetRateLimitPartitionKey(HttpContext context)
{
    return context.User.FindFirstValue(ClaimTypes.NameIdentifier) ??
        GetClientPartitionKey(context);
}

static string GetClientPartitionKey(HttpContext context)
{
    return context.Connection.RemoteIpAddress?.ToString() ??
        "unknown-client";
}

static void ValidateJwtSettings(JwtSettings settings)
{
    if (string.IsNullOrWhiteSpace(settings.Key) ||
        Encoding.UTF8.GetByteCount(settings.Key) < 32)
    {
        throw new InvalidOperationException(
            "Jwt:Key must be supplied securely and contain at least 32 bytes. " +
            "For local development, use 'dotnet user-secrets set Jwt:Key <value>'. " +
            "For deployments, set the Jwt__Key environment variable.");
    }

    if (settings.Key.Count(character => character == '.') == 2)
    {
        throw new InvalidOperationException(
            "Jwt:Key appears to be a JWT. Supply a random signing secret instead.");
    }

    if (string.IsNullOrWhiteSpace(settings.Issuer) ||
        string.IsNullOrWhiteSpace(settings.Audience))
    {
        throw new InvalidOperationException(
            "Jwt:Issuer and Jwt:Audience are required.");
    }

    if (settings.DurationInMinutes is < 5 or > 60)
    {
        throw new InvalidOperationException(
            "Jwt:DurationInMinutes must be between 5 and 60.");
    }

    if (settings.RefreshTokenDurationInDays is < 1 or > 90)
    {
        throw new InvalidOperationException(
            "Jwt:RefreshTokenDurationInDays must be between 1 and 90.");
    }
}
