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


var builder = WebApplication.CreateBuilder(args);

// Register application services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
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

// JWT configuration
var jwtKey = builder.Configuration["Jwt:Key"]?? throw new InvalidOperationException("The JWT signing key is missing.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"]?? throw new InvalidOperationException("The JWT issuer is missing.");
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("The JWT audience is missing.");

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
                ValidIssuer = jwtIssuer,

                ValidateAudience = true,
                ValidAudience = jwtAudience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
    });

builder.Services.AddAuthorization();
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
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    var context =
        scope.ServiceProvider
            .GetRequiredService<ExpenseTrackerDbContext>();

    await context.Database.MigrateAsync();

    var seeder =
        scope.ServiceProvider
            .GetRequiredService<DevelopmentDataSeeder>();

    await seeder.SeedAsync();
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


app.UseHttpsRedirection();

app.UseAuthentication();
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
                error =
                    entry.Value.Exception?.Message
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