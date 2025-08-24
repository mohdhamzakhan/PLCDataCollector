using Dapper;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PLCDataCollector.Model.Classes;
using PLCDataCollector.Model.Database;
using PLCDataCollector.Model.Validation;
using PLCDataCollector.Service.Implementation;
using PLCDataCollector.Service.Interfaces;
using Serilog;
using System.Reflection;
using System.Threading.RateLimiting;

// Configure Serilog for production logging
var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .CreateLogger();

try
{
    Log.Information("Starting PLCDataCollector Application");

    // Use Serilog for logging
    builder.Host.UseSerilog();

    // Add services to the container
    builder.Services.AddControllers(options =>
    {
        options.Filters.Add<GlobalExceptionFilter>();
    });

    // Configure strongly typed settings
    builder.Services.Configure<AppSettings>(builder.Configuration);
    builder.Services.Configure<ConnectionStrings>(builder.Configuration.GetSection("ConnectionStrings"));

    // Add Swagger/OpenAPI
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "PLC Data Collector API",
            Version = "v1",
            Description = "API for real-time production monitoring and shift-based graph visualization",
            Contact = new OpenApiContact
            {
                Name = "Production Team",
                Email = "production@company.com"
            }
        });

        // Include XML comments for better API documentation
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            c.IncludeXmlComments(xmlPath);
        }

        // Add security definition for API keys if needed
        c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
        {
            Description = "API Key needed to access the endpoints",
            In = ParameterLocation.Header,
            Name = "X-API-Key",
            Type = SecuritySchemeType.ApiKey
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "ApiKey"
                    }
                },
                new string[] {}
            }
        });
    });

    // Add CORS policy for your TV display application
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("ProductionPolicy", policy =>
        {
            policy.WithOrigins("http://localhost:3000", "http://your-tv-display-url")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
    });

    // Add health checks for production monitoring
    builder.Services.AddHealthChecks()
        .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

    // Add response compression for better performance
    builder.Services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
    });

    // Add memory caching for performance
    builder.Services.AddMemoryCache();

    // Add HTTP client for external API calls
    builder.Services.AddHttpClient();

    // Database Configuration - Source can be Oracle OR SQLite, Target is ALWAYS SQLite
    var env = builder.Configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") ?? "Development";
    var sourceDbType = builder.Configuration.GetValue<string>($"DatabaseSettings:SourceType:{env}");

    // Register Source Database Context (Oracle OR SQLite based on environment)
    if (sourceDbType?.ToUpper() == "ORACLE")
    {
        // Use Oracle for source database
        var oracleConnectionString = builder.Configuration.GetValue<string>($"ConnectionStrings:SourceDatabase:{env}");
        builder.Services.AddDbContext<PlcDataContext>(options =>
        {
            options.UseOracle(oracleConnectionString);
        });
    }
    else
    {
        // Use SQLite for source database (default)
        var sqliteConnectionString = builder.Configuration.GetValue<string>($"ConnectionStrings:SourceDatabase:{env}")
                                    ?? builder.Configuration.GetConnectionString("SourceDatabase")
                                    ?? "Data Source=plc_source.db";
        builder.Services.AddDbContext<PlcDataContext>(options =>
        {
            options.UseSqlite(sqliteConnectionString);
        });
    }

    // Register database contexts for dependency injection
    builder.Services.AddScoped<IDatabaseContext>(provider =>
    {
        if (sourceDbType?.ToUpper() == "ORACLE")
        {
            var oracleConnectionString = builder.Configuration.GetValue<string>($"ConnectionStrings:SourceDatabase:{env}");
            return new OracleContext(oracleConnectionString);
        }
        else
        {
            var sqliteConnectionString = builder.Configuration.GetValue<string>($"ConnectionStrings:SourceDatabase:{env}")
                                        ?? builder.Configuration.GetConnectionString("SourceDatabase")
                                        ?? "Data Source=plc_source.db";
            return new SqliteContext(sqliteConnectionString);
        }
    });

    builder.Services.AddScoped<ISourceDatabaseContext>(provider =>
    {
        return provider.GetRequiredService<PlcDataContext>();
    });

    // Target Database is ALWAYS SQLite
    builder.Services.AddScoped<ITargetDatabaseContext>(provider =>
    {
        var sqliteConnectionString = builder.Configuration.GetValue<string>($"ConnectionStrings:TargetDatabase:{env}")
                                    ?? "Data Source=plc_target.db";
        return new SqliteContext(sqliteConnectionString);
    });
    builder.Services.Configure<DataSyncSettings>(
    builder.Configuration.GetSection("DataSync"));

    // Register your services (make sure all interfaces have implementations)
    builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();
    builder.Services.AddScoped<IProductionService, ProductionService>();
    builder.Services.AddScoped<IPLCService, PLCService>();
    builder.Services.AddScoped<IGraphDataService, GraphDataService>();
    builder.Services.AddScoped<IFTPService, FTPService>();
    builder.Services.AddScoped<IDataParsingService, DataParsingService>();
    builder.Services.AddSingleton<IAlertService, AlertService>();
    builder.Services.AddSingleton<IWebSocketService, WebSocketService>();
    builder.Services.AddScoped<IPlcDataService, PlcDataService>();
    builder.Services.AddScoped<PlcDataValidator>();
    builder.Services.AddSingleton<IDataSyncMonitor, DataSyncMonitor>();

    // Add background services
    builder.Services.AddHostedService<PLCDataCollectorBackgroundService>();
    builder.Services.AddHostedService<HealthCheckBackgroundService>();
    builder.Services.Configure<DataSyncSettings>(
        builder.Configuration.GetSection("DataSync"));
    builder.Services.AddHostedService<DataSyncBackgroundService>();

    // Configure rate limiting
    builder.Services.AddRateLimiter(options =>
    {
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
                factory: partition => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = 1000,
                    Window = TimeSpan.FromMinutes(1)
                }));
    });

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var configService = scope.ServiceProvider.GetRequiredService<IConfigurationService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            var appSettings = configService.GetAppSettings();
            logger.LogInformation("Configuration loaded successfully");
            logger.LogInformation("Line Details Count: {Count}", appSettings.LineDetails?.Count ?? 0);

            // Test specific line detail
            var egrvLine = configService.GetLineDetail("EGRV_Final");
            if (egrvLine != null)
            {
                logger.LogInformation("EGRV_Final line loaded: {LineName}", egrvLine.LineName);
            }
            else
            {
                logger.LogWarning("EGRV_Final line not found in configuration");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load configuration");
        }
    }

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "PLC Data Collector API V1");
            c.RoutePrefix = string.Empty; // Makes Swagger UI available at root
            c.DisplayRequestDuration();
            c.EnableTryItOutByDefault();
        });
    }
    else
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();

        // Enable Swagger in production with authentication
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "PLC Data Collector API V1");
            c.RoutePrefix = "api-docs";
        });
    }

    // Add security headers
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Add("X-Frame-Options", "DENY");
        context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
        await next();
    });

    // Enable WebSocket support
    var webSocketOptions = new WebSocketOptions()
    {
        KeepAliveInterval = TimeSpan.FromMinutes(2),
        ReceiveBufferSize = 4 * 1024
    };
    app.UseWebSockets(webSocketOptions);

    app.UseHttpsRedirection();
    app.UseResponseCompression();
    app.UseCors("ProductionPolicy");
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    // Database Creation Section
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var currentEnv = app.Environment.EnvironmentName;
            var currentSourceDbType = builder.Configuration.GetValue<string>($"DatabaseSettings:SourceType:{currentEnv}");

            // 1. Create Source Database (Oracle OR SQLite)
            Log.Information("Creating source database ({SourceType})...", currentSourceDbType ?? "SQLite");
            var sourceContext = scope.ServiceProvider.GetRequiredService<PlcDataContext>();
            await sourceContext.Database.EnsureCreatedAsync();
            Log.Information("Source database created successfully");

            // 2. Create Target Database (ALWAYS SQLite)
            Log.Information("Creating target database (SQLite)...");
            var targetDbContext = scope.ServiceProvider.GetRequiredService<ITargetDatabaseContext>();

            if (targetDbContext.TestConnectionAsync())
            {
                using var targetConnection = targetDbContext.CreateConnection();
                targetConnection.Open();

                // Always use SQLite script for target database
                string scriptPath = "Model/Database/init.sql";

                if (File.Exists(scriptPath))
                {
                    var targetDbScript = await File.ReadAllTextAsync(scriptPath);

                    var statements = targetDbScript.Split(';', StringSplitOptions.RemoveEmptyEntries);

                    foreach (var statement in statements)
                    {
                        var trimmedStatement = statement.Trim();
                        if (!string.IsNullOrEmpty(trimmedStatement))
                        {
                            using var command = targetConnection.CreateCommand();
                            command.CommandText = trimmedStatement;
                            try
                            {
                                command.ExecuteNonQuery();
                            }
                            catch (Exception sqlEx)
                            {
                                Log.Warning("SQL execution warning: {Message}", sqlEx.Message);
                            }
                        }
                    }

                    Log.Information("Target database (SQLite) created successfully");
                }
                else
                {
                    Log.Warning("Database script not found: {ScriptPath}", scriptPath);
                }
            }
            else
            {
                Log.Error("Cannot connect to target database");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create databases: {Message}", ex.Message);
        }
    }

    // Add health check endpoints
    app.MapHealthChecks("/health", new HealthCheckOptions()
    {
        Predicate = _ => true,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    app.MapHealthChecks("/health/ready", new HealthCheckOptions()
    {
        Predicate = check => check.Tags.Contains("ready"),
    });

    app.MapHealthChecks("/health/live", new HealthCheckOptions()
    {
        Predicate = _ => false,
    });
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }


    app.MapControllers();

    Log.Information("Application started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}