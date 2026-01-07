using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Shared.Data;
using Shared.EventBus;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

//
// =======================
// CONFIGURATION
// =======================
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddJsonFile("appsettings.Docker.json", optional: true)
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
        Title = "Catálogos Service API",
        Version = "v1",
        Description = "Microservicio REST para gestión de catálogos"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
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
builder.Services.AddScoped<HotelRepository>();
builder.Services.AddScoped<CiudadRepository>();
builder.Services.AddScoped<PaisRepository>();
builder.Services.AddScoped<TipoHabitacionRepository>();
builder.Services.AddScoped<AmenidadRepository>();
builder.Services.AddScoped<RolRepository>();
builder.Services.AddScoped<MetodoPagoRepository>();

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

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
