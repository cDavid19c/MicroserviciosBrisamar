using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ReservasService.Protos;
using Shared.Data;
using Shared.EventBus;
using UsuariosPagosService.Consumers;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

//
// =======================
// CONFIGURATION
// =======================
builder.Configuration
    .AddJsonFile("appsettings.json", false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true)
    .AddJsonFile("appsettings.Docker.json", true)
    .AddEnvironmentVariables();

//
// =======================
// CONTROLLERS + SWAGGER
// =======================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Usuarios y Pagos Service API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        In = ParameterLocation.Header
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

//
// =======================
// JWT
// =======================
var jwtKey = builder.Configuration["Jwt:Key"] 
             ?? builder.Configuration["JWT_SECRET_KEY"]
             ?? "HotelMicroservicesSecretKey2024!@#$%^&*()_+";

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
                ?? "HotelMicroservices";

var jwtAudience = builder.Configuration["Jwt:Audience"]
                  ?? "HotelMicroservicesClients";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
            IssuerSigningKey =
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

//
// =======================
// REPOSITORIOS
// =======================
builder.Services.AddScoped<UsuarioRepository>();
builder.Services.AddScoped<PagoRepository>();
builder.Services.AddScoped<FacturaRepository>();
builder.Services.AddScoped<PdfRepository>();

//
// =======================
// CONSUMERS
// =======================
builder.Services.AddSingleton<UsuariosEventsConsumer>();
builder.Services.AddSingleton<PagosEventsConsumer>();
builder.Services.AddSingleton<FacturasEventsConsumer>();
builder.Services.AddSingleton<PdfsEventsConsumer>();

//
// =======================
// EVENT BUS (RABBITMQ OPCIONAL)
// =======================
builder.Services.AddSingleton<IEventBus>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var logger = sp.GetRequiredService<ILoggerFactory>()
                   .CreateLogger("EventBus");

    var host = config["RabbitMQ:Host"];

    if (string.IsNullOrWhiteSpace(host))
    {
        logger.LogWarning("RabbitMQ no configurado, usando NullEventBus");
        return new NullEventBus();
    }

    try
    {
        return new RabbitMqEventBus(host);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "RabbitMQ no disponible, usando NullEventBus");
        return new NullEventBus();
    }
});

//
// =======================
// HOSTED SERVICE
// =======================
builder.Services.AddHostedService<RabbitMqConsumersHostedService>();

//
// =======================
// gRPC CLIENT
// =======================
var reservasUrl = builder.Configuration["RESERVAS_SERVICE_URL"];

if (string.IsNullOrWhiteSpace(reservasUrl))
{
    reservasUrl = builder.Environment.IsDevelopment()
        ? "http://localhost:5003"
        : "http://reservas:8080";
}

// Asegurar que la URL no termine con /
reservasUrl = reservasUrl.TrimEnd('/');

builder.Services.AddGrpcClient<ReservasGrpc.ReservasGrpcClient>(options =>
{
    options.Address = new Uri(reservasUrl);
});

//
// =======================
// CORS + HEALTH
// =======================
builder.Services.AddCors(p =>
{
    p.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

builder.Services.AddHealthChecks();

//
// =======================
// BUILD
// =======================
var app = builder.Build();

//
// =======================
// MIDDLEWARE
// =======================
if (app.Environment.IsDevelopment()
    || app.Environment.EnvironmentName == "Docker"
    || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

//
// =======================
// CLASES AUXILIARES
// =======================



public class RabbitMqConsumersHostedService : BackgroundService
{
    private readonly IServiceProvider _provider;
    private readonly Microsoft.Extensions.Logging.ILogger<RabbitMqConsumersHostedService> _logger;

    public RabbitMqConsumersHostedService(
        IServiceProvider provider,
        Microsoft.Extensions.Logging.ILogger<RabbitMqConsumersHostedService> logger)
    {
        _provider = provider;
        _logger = logger;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _provider.CreateScope();

                scope.ServiceProvider.GetRequiredService<UsuariosEventsConsumer>().Subscribe();
                scope.ServiceProvider.GetRequiredService<PagosEventsConsumer>().Subscribe();
                scope.ServiceProvider.GetRequiredService<FacturasEventsConsumer>().Subscribe();
                scope.ServiceProvider.GetRequiredService<PdfsEventsConsumer>().Subscribe();

                _logger.LogInformation("RabbitMQ Consumers activos");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "RabbitMQ no disponible, reintentando en 10 segundos...");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}
