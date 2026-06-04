using System.Text.Json;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using text_cooker.Controllers;
using text_cooker.Core;
using text_cooker.Core.Interfaces;
using text_cooker.Engines;
using text_cooker.Entities;
using text_cooker.Helper;
using text_cooker.Models;
using text_cooker.Pipeline;
using text_cooker.Repositories;
using text_cooker.ServiceHandler;
using text_cooker.Validators;

namespace text_cooker;

static class Program
{
    private static IConfigurationRoot? _configurationRoot;

    static async Task Main(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") 
                          ?? throw new NullReferenceException();
        
        _configurationRoot = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
        
        var services = new ServiceCollection();
        
        // Регистрация middleware
        services.AddSingleton<ErrorHandlingMiddleware, ErrorHandlingMiddleware>();
        services.AddSingleton<AuthMiddleware, AuthMiddleware>();
        services.AddSingleton<ValidatorMiddleware, ValidatorMiddleware>();
        services.AddSingleton<RoutingMiddleware, RoutingMiddleware>();
        services.AddSingleton(new StaticFileMiddleware());
        services.AddSingleton<ExecutorMiddleware, ExecutorMiddleware>();
        
        // регистрация валидаторов
        services.AddTransient<AbstractValidator<AuthModels.LoginRequest>, LoginRequestValidator>();
        services.AddTransient<AbstractValidator<AuthModels.RegisterRequest>, RegisterRequestValidator>();
        
        // вспомогательное
        services.AddSingleton<IConfiguration>(_configurationRoot);
        services.AddSingleton(new JwtOptions(
            _configurationRoot["Jwt:Issuer"],
            _configurationRoot["Jwt:Audience"],
            _configurationRoot["Jwt:SecretKey"],
            TimeSpan.FromMinutes(int.Parse(_configurationRoot["Jwt:AccessTokenLifetimeMinutes"])),
            TimeSpan.FromDays(int.Parse(_configurationRoot["Jwt:RefreshTokenLifetimeDays"]))
            )
        );
            
        services.AddSingleton<IJwtService, JwtService>();
        services.AddTransient<ITemplateEngine, TemplateEngine>();
        services.AddSingleton(new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });
        
        services.AddSingleton<IEmbeddingService>(new EmbeddingService());
        services.AddSingleton<IParaphraseService>(new ParaphraseService(    
            model: _configurationRoot["Ollama:ParaphraseModelName"],
            baseUrl: _configurationRoot["Ollama:BaseUrl"]
            )
        );
        services.AddSingleton<RouteRegistry, RouteRegistry>();
        
        // repositories
        var connectionString = _configurationRoot["ConnectionString"] ?? throw new NullReferenceException();
        services.AddSingleton<IUserRepository>(new UserRepository(connectionString));
        services.AddSingleton<IRefreshTokenRepository>(new RefreshTokenRepository(connectionString));
        services.AddSingleton<ITemplateRepository>(new TemplateRepository(connectionString));
        services.AddSingleton<IDocumentRepository>(new DocumentRepository(connectionString));
        services.AddSingleton<IDocumentCommitRepository>(new DocumentCommitRepository(connectionString));
        
        // controllers
        services.AddTransient<DocumentController, DocumentController>();
        services.AddTransient<AuthController, AuthController>();
        services.AddTransient<TemplateController, TemplateController>();
        
        services.ValidateDependencies();
        // ---
        var serviceProvider = new ServiceProvider(services);

        var routeRegistry = serviceProvider.GetRequiredService<RouteRegistry>();
        var documentController = serviceProvider.GetRequiredService<DocumentController>();
        var authController = serviceProvider.GetRequiredService<AuthController>();
        var templateController = serviceProvider.GetRequiredService<TemplateController>();

        routeRegistry
            .Map("POST", "/api/login", authController.Login)
            .Map("POST", "/api/register", authController.Register)
            .Map("POST", "/api/refresh", authController.Refresh, Role.User)
            .Map("POST", "/api/logout", authController.Logout, Role.User);

        routeRegistry
            .Map("POST", "/api/documents/create", documentController.CreateDocument, Role.User)
            .Map("POST", "/api/documents/{id}/commit", documentController.FullCommit, Role.User)
            .Map("POST", "/api/documents/{id}/commit/partial", documentController.PartialCommit, Role.User)
            .Map("GET", "/api/documents/{id}/history", documentController.GetHistory, Role.User)
            .Map("POST", "/api/documents/{id}/rollback/{version}", documentController.Rollback, Role.User)
            .Map("GET", "/api/templates/all", templateController.GetAll, Role.User)
            .Map("POST", "/api/templates/create", templateController.CreateTemplate, Role.User)
            .Map("GET", "/api/documents/all", documentController.GetAll, Role.User);
        
        
        var pipeline = new MiddlewarePipeline();
        
        pipeline
            .Use<ErrorHandlingMiddleware>()
            .Use<AuthMiddleware>()
            .Use<RoutingMiddleware>()
            .Use<ValidatorMiddleware>()
            .Use<ExecutorMiddleware>()
            .Use<StaticFileMiddleware>();
        
        var url = _configurationRoot["BaseUrl"] ?? "http://localhost:9999/";

        var server = new HttpServer(url, pipeline, serviceProvider);
        
        var cts = new CancellationTokenSource();
        await server.StartAsync(cts);
    }
}