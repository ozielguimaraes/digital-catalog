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
using Microsoft.AspNetCore.Mvc;
using Serilog;

try
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

    var builder = WebApplication.CreateBuilder(args);

    // Usa o Serilog no lugar do logger padrão do .NET
    builder.Host.UseSerilog();

    builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

    builder.Services.AddControllers();
    builder.Services.AddRouting(options =>
    {
        options.LowercaseUrls = true;
    });

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                ValidAudience = builder.Configuration["JwtSettings:Audience"],
                IssuerSigningKey =
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]))
            };
        });
    builder.Services.AddAuthorization();

    builder.Services.AddScoped<ICatalogoService, CatalogoService>();
    builder.Services.AddScoped<IProdutoService, ProdutoService>();
    builder.Services.AddScoped<ICategoriaService, CategoriaService>();
    builder.Services.AddScoped<IPlanoAssinaturaService, PlanoAssinaturaService>();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAngularApp", policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
    });

    builder.Services.Configure<ApiBehaviorOptions>(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "MeuCatalogo API", Version = "v1" });
        c.DocumentFilter<LowercaseDocumentFilter>();
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

    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();

            if (context.Database.GetPendingMigrations().Any())
            {
                context.Database.Migrate();
            }

            await DbInitializer.InitializeAsync(services);

            Log.Information("Database initialized successfully.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while creating or seeding the database.");
            throw; // opcional, depende se quer parar a aplicação aqui
        }
    }

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MeuCatalogo API v1");
        c.RoutePrefix = "swagger";
    });
    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseMiddleware<ProblemDetailsStatusCodeMiddleware>();

    app.UseHttpsRedirection();

    app.UseCors("AllowAngularApp");

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapGet("/", () => Results.Redirect("/swagger"));
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
