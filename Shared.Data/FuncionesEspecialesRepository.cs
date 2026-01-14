using Microsoft.Data.SqlClient;
using Shared.DTOs;
using Shared.Data;
using System.Data;

namespace UsuariosPagosService.Repositories;

public class FuncionesEspecialesRepository
{
    private readonly string _connectionString;

    private static readonly DateTime SqlMin = new(1753, 1, 1);

    // ✅ CONSTRUCTOR EXACTAMENTE COMO LO PEDISTE
    public FuncionesEspecialesRepository(string? connectionString = null)
    {
        _connectionString = connectionString ?? DatabaseConfig.ConnectionString;
    }

    // ============================================================
    // CREAR PRE-RESERVA (USUARIO INTERNO)
    // ============================================================
    public PreReservaResultDto CrearPreReservaInterna(
        string idHabitacion,
        DateTime fechaInicio,
        DateTime fechaFin,
        int numeroHuespedes,
        int? duracionSeg = null,
        decimal? precioActual = null)
    {
        ValidarFechas(fechaInicio, fechaFin);

        using var cn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand("dbo.sp_crearPreReserva_usuario_interno", cn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.Add("@ID_HABITACION", SqlDbType.Char, 10).Value = idHabitacion;
        cmd.Parameters.Add("@FECHA_INICIO", SqlDbType.DateTime).Value = fechaInicio;
        cmd.Parameters.Add("@FECHA_FIN", SqlDbType.DateTime).Value = fechaFin;
        cmd.Parameters.Add("@NUMERO_HUESPEDES", SqlDbType.Int).Value = numeroHuespedes;

        cmd.Parameters.Add("@NOMBRE_USUARIO", SqlDbType.VarChar, 200).Value = DBNull.Value;
        cmd.Parameters.Add("@APELLIDO_USUARIO", SqlDbType.VarChar, 200).Value = DBNull.Value;
        cmd.Parameters.Add("@EMAIL_USUARIO", SqlDbType.VarChar, 200).Value = DBNull.Value;
        cmd.Parameters.Add("@TIPO_DOCUMENTO_USUARIO", SqlDbType.VarChar, 150).Value = DBNull.Value;
        cmd.Parameters.Add("@DOCUMENTO_USUARIO", SqlDbType.VarChar, 20).Value = DBNull.Value;

        var pNormal = cmd.Parameters.Add("@PRECIO_NORMAL_HABITACION", SqlDbType.Decimal);
        pNormal.Precision = 8;
        pNormal.Scale = 2;
        pNormal.Value = DBNull.Value;

        var pActual = cmd.Parameters.Add("@PRECIO_ACTUAL_HABITACION", SqlDbType.Decimal);
        pActual.Precision = 8;
        pActual.Scale = 2;
        pActual.Value = (object?)precioActual ?? DBNull.Value;

        cmd.Parameters.Add("@DURACION_HOLD_SEG", SqlDbType.Int)
            .Value = (object?)duracionSeg ?? DBNull.Value;

        cn.Open();
        using var rd = cmd.ExecuteReader();

        if (!rd.Read())
            throw new Exception("No se recibió respuesta al crear la pre-reserva.");

        if (!string.Equals(rd["ESTADO"]?.ToString(), "OK", StringComparison.OrdinalIgnoreCase))
            throw new Exception(rd["MENSAJE"]?.ToString() ?? "Error al crear la pre-reserva.");

        return new PreReservaResultDto
        {
            IdHold = rd["ID_HOLD"]?.ToString() ?? string.Empty
        };
    }

    // ============================================================
    // EMITIR FACTURA (USUARIO INTERNO)
    // ============================================================
    public FacturaEmitidaDto? EmitirInterno(
        int idReserva,
        string? correo = null,
        string? nombre = null,
        string? apellido = null,
        string? tipoDocumento = null,
        string? documento = null)
    {
        using var cn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand("dbo.sp_emitirFacturaHotel_Interno", cn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.AddWithValue("@ID_RESERVA", idReserva);
        cmd.Parameters.AddWithValue("@CORREO", (object?)correo ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@NOMBRE_USUARIO", (object?)nombre ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@APELLIDO_USUARIO", (object?)apellido ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@TIPO_DOCUMENTO_USUARIO", (object?)tipoDocumento ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@DOCUMENTO_USUARIO", (object?)documento ?? DBNull.Value);

        cn.Open();
        using var rd = cmd.ExecuteReader();

        if (!rd.Read()) return null;

        return new FacturaEmitidaDto
        {
            IdFactura = (int)rd["ID_FACTURA"],
            IdReserva = (int)rd["ID_RESERVA"],
            Subtotal = rd["SUBTOTAL"] as decimal?,
            Descuento = rd["DESCUENTO"] as decimal?,
            Impuesto = rd["IMPUESTO"] as decimal?,
            Total = rd["TOTAL"] as decimal?,
            UrlPdf = rd["URL_PDF"] as string
        };
    }

    // ============================================================
    // CONFIRMAR RESERVA (USUARIO INTERNO)
    // ============================================================
    public ReservaConfirmadaDto? ConfirmarReservaUsuarioInterno(
        string idHabitacion,
        string idHold,
        string nombre,
        string apellido,
        string correo,
        string tipoDocumento,
        string documento,
        DateTime fechaInicio,
        DateTime fechaFin,
        int numeroHuespedes)
    {
        ValidarFechas(fechaInicio, fechaFin);

        // Validar que el hold existe antes de intentar confirmar
        if (!ValidarHoldExiste(idHold))
        {
            throw new ArgumentException($"El hold '{idHold}' no existe o ha expirado.");
        }

        using var cn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand("dbo.sp_reservarHabitacionUsuarioInterno", cn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.Add("@ID_HABITACION", SqlDbType.Char, 10).Value = idHabitacion;
        cmd.Parameters.Add("@ID_HOLD", SqlDbType.Char, 10).Value = idHold;
        cmd.Parameters.Add("@NOMBRE_USUARIO", SqlDbType.VarChar, 200).Value = nombre;
        cmd.Parameters.Add("@APELLIDO_USUARIO", SqlDbType.VarChar, 200).Value = apellido;
        cmd.Parameters.Add("@EMAIL_USUARIO", SqlDbType.VarChar, 200).Value = correo;
        cmd.Parameters.Add("@TIPO_DOCUMENTO_USUARIO", SqlDbType.VarChar, 150).Value = tipoDocumento;
        cmd.Parameters.Add("@DOCUMENTO_USUARIO", SqlDbType.VarChar, 20).Value = documento;
        cmd.Parameters.Add("@FECHA_INICIO", SqlDbType.DateTime).Value = fechaInicio;
        cmd.Parameters.Add("@FECHA_FIN", SqlDbType.DateTime).Value = fechaFin;
        cmd.Parameters.Add("@NUMERO_HUESPEDES", SqlDbType.Int).Value = numeroHuespedes;

        try
        {
            cn.Open();
            using var rd = cmd.ExecuteReader();

            if (!rd.Read())
            {
                throw new Exception("El stored procedure no retornó ningún resultado. Verifique que el hold sea válido.");
            }

            return new ReservaConfirmadaDto
            {
                IdReserva = rd.GetInt32(rd.GetOrdinal("ID_RESERVA")),
                CostoTotalReserva = GetDecimal(rd, "COSTO_TOTAL_RESERVA"),
                FechaRegistro = GetDateTime(rd, "FECHA_REGISTRO_RESERVA"),
                FechaInicio = GetDateTime(rd, "FECHA_INICIO_RESERVA"),
                FechaFin = GetDateTime(rd, "FECHA_FINAL_RESERVA"),
                EstadoGeneral = GetString(rd, "ESTADO_GENERAL_RESERVA"),
                Estado = GetBool(rd, "ESTADO_RESERVA"),
                Nombre = GetString(rd, "NOMBRE_USUARIO"),
                Apellido = GetString(rd, "APELLIDO_USUARIO"),
                Correo = GetString(rd, "EMAIL_USUARIO"),
                TipoDocumento = GetString(rd, "TIPO_DOCUMENTO_USUARIO"),
                Habitacion = GetString(rd, "NOMBRE_HABITACION"),
                PrecioNormal = GetDecimal(rd, "PRECIO_NORMAL_HABITACION"),
                PrecioActual = GetDecimal(rd, "PRECIO_ACTUAL_HABITACION"),
                Capacidad = GetInt(rd, "CAPACIDAD_HABITACION")
            };
        }
        catch (SqlException sqlEx)
        {
            // Proporcionar mensajes más específicos según el error SQL
            if (sqlEx.Number == 2627 || sqlEx.Number == 2601) // Violación de clave única
            {
                throw new Exception("Ya existe una reserva con estos datos.", sqlEx);
            }
            else if (sqlEx.Number == 547) // Violación de clave foránea
            {
                throw new Exception("Los datos proporcionados no son válidos (habitación o hold no existe).", sqlEx);
            }
            else
            {
                throw new Exception($"Error en la base de datos al confirmar la reserva: {sqlEx.Message}", sqlEx);
            }
        }
    }

    // ============================================================
    // VALIDAR SI UN HOLD EXISTE Y ESTÁ ACTIVO
    // ============================================================
    private bool ValidarHoldExiste(string idHold)
    {
        using var cn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(@"
            SELECT COUNT(1)
            FROM HOLD
            WHERE ID_HOLD = @ID_HOLD
        ", cn);

        cmd.Parameters.Add("@ID_HOLD", SqlDbType.Char, 10).Value = idHold;

        cn.Open();
        var count = (int)cmd.ExecuteScalar();
        return count > 0;
    }

    // ============================================================
    // CANCELAR PRE-RESERVA
    // ============================================================
    public void CancelarPreReserva(string idHold)
    {
        using var cn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand("dbo.sp_cancelarPreReserva", cn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.Add("@ID_HOLD", SqlDbType.Char, 10).Value = idHold;

        cn.Open();
        cmd.ExecuteNonQuery();
    }
    // ============================================================
    // OBTENER ID RESERVA DESDE HOLD
    // ============================================================
        public int ObtenerIdReservaPorHold(string idHold)
        {
            using var cn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(@"
            SELECT ID_RESERVA
            FROM HOLD
            WHERE ID_HOLD = @ID_HOLD
        ", cn);

            cmd.Parameters.Add("@ID_HOLD", SqlDbType.Char, 10).Value = idHold;

            cn.Open();
            var result = cmd.ExecuteScalar();

            if (result == null)
                throw new Exception($"No se encontró reserva asociada al HOLD {idHold}");

            return Convert.ToInt32(result);
        }
// ============================================================
// EXPIRAR PRE-RESERVA / RESERVA
// ============================================================
        public void ExpirarReserva(int idReserva)
        {
            using var cn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(@"
        UPDATE RESERVA
        SET
            ESTADO_GENERAL_RESERVA = 'EXPIRADO',
            ESTADO_RESERVA = 0,
            FECHA_MODIFICACION_RESERVA = GETDATE()
        WHERE ID_RESERVA = @ID_RESERVA
    ", cn);

            cmd.Parameters.Add("@ID_RESERVA", SqlDbType.Int).Value = idReserva;

            cn.Open();
            cmd.ExecuteNonQuery();
        }

    // ============================================================
    // Helpers
    // ============================================================
    private static void ValidarFechas(DateTime inicio, DateTime fin)
    {
        if (inicio < SqlMin || fin < SqlMin)
            throw new ArgumentOutOfRangeException("Fechas inválidas.");

        if (inicio >= fin)
            throw new ArgumentException("fechaInicio debe ser menor que fechaFin.");
    }

    private static string? GetString(IDataRecord r, string col)
        => r.IsDBNull(r.GetOrdinal(col)) ? null : r[col].ToString();

    private static DateTime? GetDateTime(IDataRecord r, string col)
        => r.IsDBNull(r.GetOrdinal(col)) ? null : r.GetDateTime(r.GetOrdinal(col));

    private static decimal? GetDecimal(IDataRecord r, string col)
        => r.IsDBNull(r.GetOrdinal(col)) ? null : r.GetDecimal(r.GetOrdinal(col));

    private static int? GetInt(IDataRecord r, string col)
        => r.IsDBNull(r.GetOrdinal(col)) ? null : r.GetInt32(r.GetOrdinal(col));

    private static bool? GetBool(IDataRecord r, string col)
        => r.IsDBNull(r.GetOrdinal(col)) ? null : r.GetBoolean(r.GetOrdinal(col));
}
