using MeuCatalogo.Application.Entities;
using MeuCatalogo.Application.Infrastructure.Data;
using MeuCatalogo.Application.Interfaces;
using MeuCatalogo.Application.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using MeuCatalogo.API.Filters;
using MeuCatalogo.API.Middlewares;
using MeuCatalogo.API.Converters;
using MeuCatalogo.API.Infrastructure.Messages;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Sentry;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Configure Sentry
    builder.WebHost.UseSentry(o =>
    {
        o.Dsn = "https://d28b29c06ae140171a04078f72b872b8@o4507538405916672.ingest.us.sentry.io/4510213135925248";
        o.Debug = builder.Environment.IsDevelopment();
        o.TracesSampleRate = 1.0;
        o.ProfilesSampleRate = 1.0;
        o.SendDefaultPii = true;
        o.AttachStacktrace = true;
        o.MaxBreadcrumbs = 50;
    });

    // Configure logging first - use built-in logging as fallback
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
    builder.Logging.AddDebug();
    var environment = builder.Environment.EnvironmentName;

    // Try to configure Serilog, but don't fail if it doesn't work
    try
    {
        var isDevelopment = environment == "Development";

        if (isDevelopment)
        {
            string logPath = Path.Combine(AppContext.BaseDirectory, "logs");
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
                .CreateLogger();
        }
        else
        {
            // Production logging configuration - simpler approach
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Warning()
                .WriteTo.Console()
                .CreateLogger();
        }

        // Usa o Serilog no lugar do logger padrão do .NET
        builder.Host.UseSerilog();
    }
    catch (Exception serilogEx)
    {
        // If Serilog fails, continue with built-in logging
        Console.WriteLine($"Serilog configuration failed: {serilogEx.Message}");
        Console.WriteLine("Continuing with built-in logging...");
    }

    builder.Configuration.AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true);

    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            // Configure JSON serialization to use UTC for all DateTime values
            options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            options.JsonSerializerOptions.Converters.Add(new UtcDateTimeConverter());
            options.JsonSerializerOptions.Converters.Add(new UtcNullableDateTimeConverter());
            options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.WriteIndented = true;
        });

    builder.Services.AddRouting(options =>
    {
        options.LowercaseUrls = true;
    });

    Console.WriteLine("=== MEUCATALOGO API STARTUP ===");
    Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
    Console.WriteLine($"Content Root: {builder.Environment.ContentRootPath}");
    Console.WriteLine($"Application Name: {builder.Environment.ApplicationName}");

    string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(connectionString))
    {
        var errorMsg = "Connection string 'DefaultConnection' is not configured.";
        Console.WriteLine($"ERROR: {errorMsg}");
        Log.Fatal(errorMsg);
        throw new InvalidOperationException(errorMsg);
    }

    Console.WriteLine("✓ Connection string configured successfully.");
    Log.Information("Connection string configured successfully.");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString, b => b.MigrationsAssembly("MeuCatalogo.API")));

    builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

    // Configure Password Reset Token to use Short Code (EmailProvider) for better Mobile experience
    builder.Services.Configure<IdentityOptions>(options =>
    {
        options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
    });

    Console.WriteLine("✓ Database context configured successfully.");

    // Configure Email Sender
    var emailSettings = builder.Configuration.GetSection("EmailSettings");
    if (emailSettings.Exists())
    {
        builder.Services.AddTransient<EmailSender>(sp =>
            new EmailSender(
                emailSettings["Username"],
                emailSettings["Password"],
                emailSettings["Host"],
                int.Parse(emailSettings["Port"]),
                bool.Parse(emailSettings["EnableSsl"] ?? "true"),
                emailSettings["SenderName"] ?? "Meu Catálogo"
            ));
        Console.WriteLine("✓ EmailSender configured successfully.");
    }
    else
    {
        Console.WriteLine("⚠ EmailSettings section missing - EmailSender not configured.");
    }

    // Validate JWT configuration
    Console.WriteLine("Validating JWT configuration...");
    string? jwtKey = builder.Configuration["JwtSettings:Key"];
    string? jwtIssuer = builder.Configuration["JwtSettings:Issuer"];
    string? jwtAudience = builder.Configuration["JwtSettings:Audience"];

    if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
    {
        var errorMsg = "JWT settings are not properly configured.";
        Console.WriteLine($"ERROR: {errorMsg}");
        Console.WriteLine($"JWT Key: {(string.IsNullOrEmpty(jwtKey) ? "MISSING" : "OK")}");
        Console.WriteLine($"JWT Issuer: {(string.IsNullOrEmpty(jwtIssuer) ? "MISSING" : "OK")}");
        Console.WriteLine($"JWT Audience: {(string.IsNullOrEmpty(jwtAudience) ? "MISSING" : "OK")}");
        Log.Fatal(errorMsg);
        throw new InvalidOperationException(errorMsg);
    }

    Console.WriteLine("✓ JWT settings configured successfully.");
    Log.Information("JWT settings configured successfully.");

    builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ClockSkew = TimeSpan.Zero
            };
        });
    builder.Services.AddAuthorization();

    builder.Services.AddScoped<ICatalogoService, CatalogoService>();
    builder.Services.AddScoped<IProdutoService, ProdutoService>();
    builder.Services.AddScoped<ICategoriaService, CategoriaService>();
    builder.Services.AddScoped<IPlanoAssinaturaService, PlanoAssinaturaService>();
    builder.Services.AddScoped<IRefreshTokenService, MeuCatalogo.API.Services.RefreshTokenService>();

    // Storage configuration: use Azure when connection string is present; otherwise fallback to local filesystem
    var blobSection = builder.Configuration.GetSection("BlobStorage");
    var blobOptions = blobSection.Get<MeuCatalogo.Application.Infrastructure.Storage.BlobStorageOptions>() ?? new();
    builder.Services.AddSingleton(blobOptions);

    string? storageConn = builder.Configuration["BlobStorage:ConnectionString"];

    if (!string.IsNullOrWhiteSpace(storageConn))
    {
        builder.Services.AddSingleton<MeuCatalogo.Application.Interfaces.IStorageService,
            MeuCatalogo.Application.Infrastructure.Storage.AzureBlobStorageService>();
        Console.WriteLine("✓ Azure Blob Storage enabled");
    }
    else
    {
        builder.Services.AddSingleton<MeuCatalogo.Application.Interfaces.IStorageService,
            MeuCatalogo.Application.Infrastructure.Storage.LocalFileStorageService>();
        Console.WriteLine("⚠ Azure connection string missing — using local file storage.");
    }

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAngularApp", policy =>
        {
            var allowedOrigins = builder.Configuration.GetSection("AllowedCorsOrigins").Get<string[]>() ?? new[]
            {
                "http://localhost:4200",
                "https://localhost:4200"
            };

            policy.WithOrigins(allowedOrigins)
                .SetIsOriginAllowed(origin => true) // Allow any origin in development/production if needed (use with caution)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
    });

    builder.Services.Configure<ApiBehaviorOptions>(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });

    // Add health checks
    builder.Services.AddHealthChecks();
        // .AddDbContext<ApplicationDbContext>();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "MeuCatalogo API",
            Version = "v1",
            Description = "API para gerenciamento de catálogos digitais com sistema de assinaturas",
            Contact = new OpenApiContact
            {
                Name = "MeuCatalogo Team",
                Email = "hi@ozielguimaraes.dev"
            }
        });
        c.DocumentFilter<LowercaseDocumentFilter>();
        c.EnableAnnotations();
        c.AddSecurityDefinition("Bearer",
            new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                new string[] { }
            }
        });
    });

    var app = builder.Build();

    Console.WriteLine("✓ Application built successfully.");
    Console.WriteLine("Starting database migration...");

    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var env = services.GetRequiredService<IHostEnvironment>();

        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();

            // Always run migrations in production to ensure database is up to date
            Console.WriteLine("Running database migration...");
            Log.Information("Starting database migration...");
            await context.Database.MigrateAsync();
            Console.WriteLine("✓ Database migration completed successfully.");
            Log.Information("Database migration completed successfully.");

            // Only run database initialization in development
            if (env.IsDevelopment())
            {
                Console.WriteLine("Initializing database with sample data (development)...");
                await DbInitializer.InitializeAsync(services);
                Console.WriteLine("✓ Database initialized (development).");
                Log.Information("Database initialized (development).");
            }
        }
        catch (Exception ex)
        {
            var errorMsg = $"Database migration failed: {ex.Message}";
            Console.WriteLine($"ERROR: {errorMsg}");
            Console.WriteLine($"Exception Type: {ex.GetType().Name}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            Log.Fatal(ex, "Database migration failed. Application cannot start.");
            throw;
        }
    }

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MeuCatalogo API v1");
        c.RoutePrefix = "swagger";
    });

    app.UseSentryTracing();

    var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "Uploads");

    if (!Directory.Exists(uploadsPath))
    {
        Directory.CreateDirectory(uploadsPath);
    }

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
        RequestPath = "/uploads"
    });

    app.UseHttpsRedirection();

    app.UseCors("AllowAngularApp");
    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseMiddleware<ProblemDetailsStatusCodeMiddleware>();


    app.UseAuthentication();
    app.UseAuthorization();

    // Add health check endpoint
    app.MapHealthChecks("/health");

    app.MapGet("/", () => Results.Redirect("/swagger"));
    app.MapControllers();

    Console.WriteLine("✓ Application configured successfully.");
    Console.WriteLine("✓ Starting web server...");
    Console.WriteLine("=== MEUCATALOGO API READY ===");

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine("=== FATAL ERROR ===");
    Console.WriteLine($"Application start-up failed: {ex.Message}");
    Console.WriteLine($"Exception Type: {ex.GetType().Name}");
    Console.WriteLine($"Stack Trace: {ex.StackTrace}");

    if (ex.InnerException != null)
    {
        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
        Console.WriteLine($"Inner Exception Type: {ex.InnerException.GetType().Name}");
    }

    // Capture exception in Sentry
    SentrySdk.CaptureException(ex);

    Log.Fatal(ex, "Application start-up failed");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
