using HotChocolate.Authorization;
using Shared.Data;
using Shared.DTOs;

namespace HabitacionesService.GraphQL;

/// <summary>
/// Queries GraphQL para Habitaciones - AUTENTICACIÓN OBLIGATORIA
/// </summary>
public class Query
{
    // ============ HABITACIONES ============

    public async Task<IEnumerable<HabitacionDto>> GetHabitaciones([Service] HabitacionRepository repo)
        => await repo.ObtenerTodasAsync();

    public async Task<HabitacionDto?> GetHabitacionById([Service] HabitacionRepository repo, string id)
        => await repo.ObtenerPorIdAsync(id);

    public async Task<IEnumerable<HabitacionDto>> GetHabitacionesPorHotel([Service] HabitacionRepository repo, int idHotel)
        => await repo.ObtenerPorHotelAsync(idHotel);

    // ============ RESOLVERS PARA NAVEGACIÓN ============
    
    /// <summary>
    /// Resuelve la propiedad 'hotel' para HabitacionDto
    /// </summary>
    public async Task<HotelDto?> GetHotel(
        [Parent] HabitacionDto habitacion,
        [Service] HotelRepository hotelRepo)
        => await hotelRepo.ObtenerPorIdAsync(habitacion.IdHotel);

    /// <summary>
    /// Resuelve la propiedad 'ciudad' para HabitacionDto
    /// </summary>
    public async Task<CiudadDto?> GetCiudad(
        [Parent] HabitacionDto habitacion,
        [Service] CiudadRepository ciudadRepo)
        => await ciudadRepo.ObtenerPorIdAsync(habitacion.IdCiudad);

    /// <summary>
    /// Resuelve la propiedad 'imagenes' para HabitacionDto
    /// </summary>
    public async Task<IEnumerable<ImagenHabitacionDto>> GetImagenes(
        [Parent] HabitacionDto habitacion,
        [Service] ImagenHabitacionRepository imagenRepo)
        => await imagenRepo.ObtenerPorHabitacionAsync(habitacion.IdHabitacion);

    // ============ IMÁGENES HABITACIÓN ============

    public async Task<IEnumerable<ImagenHabitacionDto>> GetImagenesHabitacion([Service] ImagenHabitacionRepository repo)
        => await repo.ObtenerTodosAsync();

    public async Task<ImagenHabitacionDto?> GetImagenHabitacionById([Service] ImagenHabitacionRepository repo, int id)
        => await repo.ObtenerPorIdAsync(id);

    public async Task<IEnumerable<ImagenHabitacionDto>> GetImagenesPorHabitacion([Service] ImagenHabitacionRepository repo, string idHabitacion)
        => await repo.ObtenerPorHabitacionAsync(idHabitacion);

    // ============ AMENIDADES POR HABITACIÓN ============

    public async Task<IEnumerable<AmexHabDto>> GetAmenidadesPorHabitacion([Service] AmexHabRepository repo)
        => await repo.ObtenerTodosAsync();

    public async Task<AmexHabDto?> GetAmexHab([Service] AmexHabRepository repo, string idHabitacion, int idAmenidad)
        => await repo.ObtenerPorIdAsync(idHabitacion, idAmenidad);

    public async Task<IEnumerable<AmexHabDto>> GetAmenidadesDeHabitacion([Service] AmexHabRepository repo, string idHabitacion)
        => await repo.ObtenerPorHabitacionAsync(idHabitacion);

    // ============ DESCUENTOS ============

    [Authorize]
    public async Task<IEnumerable<DescuentoDto>> GetDescuentos(
        [Service] DescuentoRepository repo)
        => await repo.ObtenerTodosAsync();

    [Authorize]
    public async Task<DescuentoDto?> GetDescuentoById(
        [Service] DescuentoRepository repo, int id)
        => await repo.ObtenerPorIdAsync(id);


}
