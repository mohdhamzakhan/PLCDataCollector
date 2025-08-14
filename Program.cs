using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using PLCDataCollector.Model;
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
        .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy())
        .AddOracle(builder.Configuration.GetConnectionString("Production"), name: "oracle-db")
        .AddCheck<PLCHealthCheck>("plc-connection");

    // Add response compression for better performance
    builder.Services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
    });

    // Add memory caching for performance
    builder.Services.AddMemoryCache();

    // Add HTTP client for external API calls
    builder.Services.AddHttpClient();

    // Register your services
    builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();
    builder.Services.AddScoped<IProductionService, ProductionService>();
    builder.Services.AddScoped<IPLCService, PLCService>();
    builder.Services.AddScoped<IGraphDataService, GraphDataService>();
    builder.Services.AddScoped<IFTPService, FTPService>();
    builder.Services.AddScoped<IDataParsingService, DataParsingService>();
    builder.Services.AddSingleton<IAlertService, AlertService>();
    builder.Services.AddSingleton<IWebSocketService, WebSocketService>();

    // Add background services for real-time data collection
    builder.Services.AddHostedService<PLCDataCollectorBackgroundService>();
    builder.Services.AddHostedService<HealthCheckBackgroundService>();

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