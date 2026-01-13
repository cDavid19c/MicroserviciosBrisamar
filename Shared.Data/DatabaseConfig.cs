namespace Shared.Data;

public static class DatabaseConfig
{
    // Cadena de conexión a la base de datos SQL Server en Somee.com
    // NOTA: En producción (Railway), esto será sobrescrito por variables de entorno
    public const string ConnectionString = "Server=db32030.public.databaseasp.net;Database=db32030;User Id=db32030;Password=3s%K-7Hxn_9E;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True;";
}
