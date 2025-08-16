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

    // Replace the service registration section in Program.cs (around lines 95-120)

    // Register database contexts first
    builder.Services.AddDbContext<PlcDataContext>(options =>
    {
        options.UseSqlite(builder.Configuration.GetConnectionString("SourceDatabase"));
    });


    // Register database contexts for dependency injection
    builder.Services.AddScoped<IDatabaseContext>(provider =>
    {
        var connectionString = builder.Configuration.GetConnectionString("SourceDatabase");
        return new SqliteContext(connectionString);
    });

    builder.Services.AddScoped<ISourceDatabaseContext>(provider =>
    {
        return provider.GetRequiredService<PlcDataContext>();
    });

    builder.Services.AddScoped<ITargetDatabaseContext>(provider =>
    {
        var env = builder.Configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var dbType = builder.Configuration.GetValue<string>($"DatabaseSettings:TargetType:{env}");

        if (dbType?.ToUpper() == "ORACLE")
        {
            var oracleConnectionString = builder.Configuration.GetValue<string>($"ConnectionStrings:TargetDatabase:{env}");
            return new OracleContext(oracleConnectionString);
        }
        else
        {
            var sqliteConnectionString = builder.Configuration.GetValue<string>($"ConnectionStrings:TargetDatabase:{env}");
            return new SqliteContext(sqliteConnectionString);
        }
    });

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

    // Comment out or remove DatabaseMigrationService temporarily to isolate the issue
    // builder.Services.AddScoped<DatabaseMigrationService>();

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

    //using (var sourceDb = new SqliteConnection("Data Source=plc_source.db"))
    //{
    //    sourceDb.Execute(File.ReadAllText("Model/Database/init.sql"));
    //}

    //if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Production")
    //{
    //    using (var targetDb = new SqliteConnection("Data Source=plc_target.db"))
    //    {
    //        targetDb.Execute(File.ReadAllText("Model/Database/init.sql"));
    //    }
    //}

    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var context = scope.ServiceProvider.GetRequiredService<PlcDataContext>();
            await context.Database.EnsureCreatedAsync();
            Log.Information("Database created successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create database");
            // Don't throw here, let the app continue
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

    app.MapControllers();

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