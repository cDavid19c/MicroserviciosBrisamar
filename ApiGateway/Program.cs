using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Shared.EventBus;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

//
// =======================
// CONFIGURATION
// =======================
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddJsonFile("appsettings.Docker.json", optional: true)
    .AddEnvironmentVariables();

//
// =======================
// CONTROLLERS
// =======================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

//
// =======================
// SWAGGER + JWT
// =======================
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Hotel Microservices API Gateway",
        Version = "v1",
        Description = "API Gateway centralizado para el sistema hotelero"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Ingresa solo el token JWT (sin 'Bearer')",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
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
// JWT AUTHENTICATION
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
// HTTP CLIENT (RECA)
// =======================
builder.Services.AddHttpClient("RecaApi", (sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var baseUrl = config["Integrations:RecaApi:BaseUrl"];

    if (!string.IsNullOrWhiteSpace(baseUrl))
    {
        client.BaseAddress = new Uri(baseUrl);
        client.DefaultRequestHeaders.Add("Accept", "application/json");
    }
});

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
// YARP REVERSE PROXY
// =======================
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

//
// =======================
// CORS
// =======================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

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

// Logging simple
app.Use(async (context, next) =>
{
    var start = DateTime.UtcNow;
    await next();
    var elapsed = DateTime.UtcNow - start;

    Console.WriteLine(
        $"{context.Request.Method} {context.Request.Path} → " +
        $"{context.Response.StatusCode} " +
        $"({elapsed.TotalMilliseconds} ms)");
});

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

//
// =======================
// ENDPOINTS
// =======================
app.MapControllers();
app.MapReverseProxy();

app.Run();
