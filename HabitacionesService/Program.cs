using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Shared.Data;
using Shared.EventBus;
using HabitacionesService.GraphQL;
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
builder.Services.AddScoped<HabitacionRepository>();
builder.Services.AddScoped<ImagenHabitacionRepository>();
builder.Services.AddScoped<AmexHabRepository>();
builder.Services.AddScoped<DescuentoRepository>();
builder.Services.AddScoped<HotelRepository>();
builder.Services.AddScoped<CiudadRepository>();

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
// GRAPHQL (HotChocolate)
// =======================
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddAuthorization()
    .ModifyRequestOptions(opt =>
        opt.IncludeExceptionDetails =
            builder.Environment.IsDevelopment()
    );

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
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

//
// =======================
// ENDPOINTS
// =======================
app.MapGraphQL();

if (app.Environment.IsDevelopment()
    || app.Environment.EnvironmentName == "Docker")
{
    app.MapBananaCakePop("/graphql-ui");
}

app.Run();
