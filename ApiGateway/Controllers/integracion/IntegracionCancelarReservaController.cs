using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ApiGateway.Controllers.Integracion;

[ApiController]
[Route("api/integracion/reservas")]
public class IntegracionCancelarReservaController : ControllerBase
{
    private readonly HttpClient _http;
    private readonly ILogger<IntegracionCancelarReservaController> _logger;

    public IntegracionCancelarReservaController(
        IHttpClientFactory factory,
        ILogger<IntegracionCancelarReservaController> logger)
    {
        _http = factory.CreateClient("RecaApi");
        _logger = logger;
    }

    [HttpDelete("cancelar")]
    public async Task<IActionResult> CancelarReserva(
        [FromQuery] int idReserva)
    {
        try
        {
            _logger.LogInformation("Cancelando reserva {IdReserva} en RECA API", idReserva);
            
            // Construir URL hacia RECA
            var response = await _http.DeleteAsync(
                $"/api/v1/hoteles/cancel?idReserva={idReserva}"
            );

            var content = await response.Content.ReadAsStringAsync();
            
            _logger.LogInformation("RECA API response status: {StatusCode}, content: {Content}", 
                response.StatusCode, content);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, content);
            }

            // Deserializar la respuesta de RECA
            var result = JsonSerializer.Deserialize<CancelarReservaResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result == null)
            {
                _logger.LogWarning("Failed to deserialize RECA response: {Content}", content);
                return Ok(new CancelarReservaResponse
                {
                    Success = false,
                    MontoPagado = 0,
                    Mensaje = "Error al procesar respuesta de RECA"
                });
            }

            // 🔔 Opcional: publicar evento RabbitMQ
            // if (result.Success)
            // {
            //     await _eventBus.PublishAsync(new ReservaCanceladaEvent
            //     {
            //         IdReserva = idReserva,
            //         MontoPagado = result.MontoPagado,
            //         FechaCancelacion = DateTime.UtcNow
            //     });
            // }

            _logger.LogInformation("Reserva {IdReserva} cancelada. Success: {Success}, Monto: {Monto}", 
                idReserva, result.Success, result.MontoPagado);

            return Ok(result);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error de conexión al cancelar reserva {IdReserva}", idReserva);
            return StatusCode(503, new CancelarReservaResponse
            {
                Success = false,
                MontoPagado = 0,
                Mensaje = "Error de conexión con el servicio de reservas"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al cancelar reserva {IdReserva}", idReserva);
            return StatusCode(500, new CancelarReservaResponse
            {
                Success = false,
                MontoPagado = 0,
                Mensaje = "Error interno al cancelar la reserva"
            });
        }
    }
}

/// <summary>
/// Respuesta de la API RECA para cancelación de reserva
/// </summary>
public class CancelarReservaResponse
{
    /// <summary>
    /// Indica si la cancelación fue exitosa
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Monto que fue pagado y será reembolsado
    /// </summary>
    public decimal MontoPagado { get; set; }
    
    /// <summary>
    /// Mensaje descriptivo del resultado
    /// </summary>
    public string Mensaje { get; set; } = string.Empty;
}
