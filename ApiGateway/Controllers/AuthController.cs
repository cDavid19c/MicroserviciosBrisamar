using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ApiGateway.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Generar token JWT para autenticación
    /// </summary>
    [HttpPost("token")]
    public ActionResult<TokenResponse> GenerateToken([FromBody] TokenRequest request)
    {
        // En producción, validar contra la base de datos de usuarios
        // Por ahora, validación simple para demostración
        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
        {
            return BadRequest(new { message = "Username y password son requeridos" });
        }

        // Aquí deberías validar las credenciales contra tu base de datos
        // Por ejemplo, llamando al UsuariosPagosService

        var token = GenerateJwtToken(request.Username, request.Role ?? "User");

        return Ok(new TokenResponse
        {
            Token = token,
            ExpiresIn = int.Parse(_configuration["Jwt:ExpireMinutes"] ?? "60") * 60,
            TokenType = "Bearer"
        });
    }

    /// <summary>
    /// Validar un token JWT
    /// </summary>
    [HttpPost("validate")]
    public ActionResult<ValidateTokenResponse> ValidateToken([FromBody] ValidateTokenRequest request)
    {
        try
        {
            var jwtKey = _configuration["Jwt:Key"] 
                         ?? _configuration["JWT_SECRET_KEY"]
                         ?? "HotelMicroservicesSecretKey2024!@#$%^&*()_+";
            
            var jwtIssuer = _configuration["Jwt:Issuer"] 
                            ?? "HotelMicroservices";
            
            var jwtAudience = _configuration["Jwt:Audience"] 
                              ?? "HotelMicroservicesClients";

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(jwtKey);

            tokenHandler.ValidateToken(request.Token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(key)
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var username = jwtToken.Claims.First(x => x.Type == ClaimTypes.Name).Value;
            var role = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value;

            return Ok(new ValidateTokenResponse
            {
                Valid = true,
                Username = username,
                Role = role,
                ExpiresAt = jwtToken.ValidTo
            });
        }
        catch
        {
            return Ok(new ValidateTokenResponse { Valid = false });
        }
    }

    /// <summary>
    /// Refrescar un token JWT
    /// </summary>
    [HttpPost("refresh")]
    public ActionResult<TokenResponse> RefreshToken([FromBody] ValidateTokenRequest request)
    {
        try
        {
            var jwtKey = _configuration["Jwt:Key"] 
                         ?? _configuration["JWT_SECRET_KEY"]
                         ?? "HotelMicroservicesSecretKey2024!@#$%^&*()_+";
            
            var jwtIssuer = _configuration["Jwt:Issuer"] 
                            ?? "HotelMicroservices";
            
            var jwtAudience = _configuration["Jwt:Audience"] 
                              ?? "HotelMicroservicesClients";

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(jwtKey);

            // Validar el token actual (permitiendo tokens expirados para refresh)
            tokenHandler.ValidateToken(request.Token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false, // Permitir tokens expirados
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(key)
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var username = jwtToken.Claims.First(x => x.Type == ClaimTypes.Name).Value;
            var role = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value ?? "User";

            var newToken = GenerateJwtToken(username, role);

            return Ok(new TokenResponse
            {
                Token = newToken,
                ExpiresIn = int.Parse(_configuration["Jwt:ExpireMinutes"] ?? "60") * 60,
                TokenType = "Bearer"
            });
        }
        catch
        {
            return Unauthorized(new { message = "Token inválido" });
        }
    }

    private string GenerateJwtToken(string username, string role)
    {
        var jwtKey = _configuration["Jwt:Key"] 
                     ?? _configuration["JWT_SECRET_KEY"]
                     ?? "HotelMicroservicesSecretKey2024!@#$%^&*()_+";
        
        var jwtIssuer = _configuration["Jwt:Issuer"] 
                        ?? "HotelMicroservices";
        
        var jwtAudience = _configuration["Jwt:Audience"] 
                          ?? "HotelMicroservicesClients";
        
        var expireMinutes = int.Parse(_configuration["Jwt:ExpireMinutes"] ?? "60");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expireMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public record TokenRequest(string Username, string Password, string? Role = null);
public record TokenResponse
{
    public string Token { get; set; } = "";
    public int ExpiresIn { get; set; }
    public string TokenType { get; set; } = "Bearer";
}

public record ValidateTokenRequest(string Token);
public record ValidateTokenResponse
{
    public bool Valid { get; set; }
    public string? Username { get; set; }
    public string? Role { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
