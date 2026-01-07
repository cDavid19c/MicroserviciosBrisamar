# ?? VARIABLES DE ENTORNO - RENDER.COM

Este archivo contiene todas las variables que necesitas configurar en Render.com para tus 5 microservicios.

---

## ??? SQL SERVER (Base de datos ya existente en Somee.com)

Tu base de datos **ya está funcionando** en SQL Server (Somee.com).

**NO necesitas configurar DATABASE_URL en Render.**

La connection string ya está configurada en el código (`Shared.Data/DatabaseConfig.cs`):

```
Server=db31651.public.databaseasp.net;Database=db31651;User Id=db31651;Password=prueba2020d;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True;
```

> ? Todos los microservicios compartirán la misma base de datos SQL Server.

---

## ?? VARIABLES POR SERVICIO

### 1?? CatalogosService

```env
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:$PORT
JWT_SECRET_KEY=HotelMicroservicesSecretKey2024!@#$%^&*()_+
RABBITMQ_URL=
```

> ?? **IMPORTANTE:** `ASPNETCORE_URLS=http://0.0.0.0:$PORT` es obligatorio en Render

---

### 2?? HabitacionesService

```env
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:$PORT
JWT_SECRET_KEY=HotelMicroservicesSecretKey2024!@#$%^&*()_+
RABBITMQ_URL=
```

---

### 3?? ReservasService

```env
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:$PORT
JWT_SECRET_KEY=HotelMicroservicesSecretKey2024!@#$%^&*()_+
RABBITMQ_URL=
```

---

### 4?? UsuariosPagosService

**Primera configuración (antes de tener las URLs):**

```env
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:$PORT
JWT_SECRET_KEY=HotelMicroservicesSecretKey2024!@#$%^&*()_+
RABBITMQ_URL=
RESERVAS_SERVICE_URL=
```

**Después de crear ReservasService, actualizar:**

```env
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:$PORT
JWT_SECRET_KEY=HotelMicroservicesSecretKey2024!@#$%^&*()_+
RABBITMQ_URL=
RESERVAS_SERVICE_URL=https://reservas-service.onrender.com
```

---

### 5?? ApiGateway

**Primera configuración (antes de tener las URLs):**

```env
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:$PORT
JWT_SECRET_KEY=HotelMicroservicesSecretKey2024!@#$%^&*()_+
```

**Después de crear todos los servicios, actualizar:**

```env
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:$PORT
JWT_SECRET_KEY=HotelMicroservicesSecretKey2024!@#$%^&*()_+
CATALOGOS_SERVICE_URL=https://catalogos-service.onrender.com
HABITACIONES_SERVICE_URL=https://habitaciones-service.onrender.com
RESERVAS_SERVICE_URL=https://reservas-service.onrender.com
USUARIOS_PAGOS_SERVICE_URL=https://usuarios-pagos-service.onrender.com
```

---

## ?? NOTAS IMPORTANTES

### `$PORT` Variable

Render asigna dinámicamente el puerto a través de la variable `$PORT`. 

**SIEMPRE usa:**
```env
ASPNETCORE_URLS=http://0.0.0.0:$PORT
```

NO uses:
- ? `http://0.0.0.0:8080`
- ? `http://+:8080`
- ? `http://localhost:8080`

---

### JWT_SECRET_KEY

```env
JWT_SECRET_KEY=HotelMicroservicesSecretKey2024!@#$%^&*()_+
```

> ?? **IMPORTANTE:** La misma clave en TODOS los servicios

> ?? **Recomendación:** Genera una clave más segura en producción real:
> https://randomkeygen.com/

---

### RABBITMQ_URL

Si NO usas RabbitMQ, déjalo vacío:
```env
RABBITMQ_URL=
```

Si usas CloudAMQP (gratuito):
```env
RABBITMQ_URL=amqp://username:password@pelican.rmq.cloudamqp.com/vhost
```

---

## ?? TABLA RESUMEN

| Variable | Catálogos | Habitaciones | Reservas | Usuarios/Pagos | ApiGateway |
|----------|-----------|--------------|----------|----------------|------------|
| `ASPNETCORE_ENVIRONMENT` | ? | ? | ? | ? | ? |
| `ASPNETCORE_URLS` | ? | ? | ? | ? | ? |
| `JWT_SECRET_KEY` | ? | ? | ? | ? | ? |
| `RABBITMQ_URL` | ? | ? | ? | ? | ? |
| `RESERVAS_SERVICE_URL` | ? | ? | ? | ? | ? |
| `CATALOGOS_SERVICE_URL` | ? | ? | ? | ? | ? |
| `HABITACIONES_SERVICE_URL` | ? | ? | ? | ? | ? |
| `USUARIOS_PAGOS_SERVICE_URL` | ? | ? | ? | ? | ? |

---

## ??? CÓMO AGREGAR VARIABLES EN RENDER

### Método 1: Durante la creación del servicio

```
1. Al crear el Web Service
2. Scroll down ? "Advanced"
3. "Add Environment Variable"
4. Agregar una por una
```

### Método 2: Después de crear el servicio

```
1. Click en el servicio
2. Sidebar ? "Environment"
3. "Add Environment Variable"
4. O usa "Edit as Text" para copiar/pegar múltiples
```

---

## ?? ORDEN DE ACTUALIZACIÓN

### Paso 1: Crear todos los servicios con variables básicas

```
Crear: CatalogosService, HabitacionesService, ReservasService, 
       UsuariosPagosService, ApiGateway

Con: ASPNETCORE_ENVIRONMENT, ASPNETCORE_URLS, JWT_SECRET_KEY
```

### Paso 2: Copiar URLs generadas

```
Ejemplo:
- https://catalogos-service.onrender.com
- https://habitaciones-service.onrender.com
- https://reservas-service.onrender.com
- https://usuarios-pagos-service.onrender.com
```

### Paso 3: Actualizar UsuariosPagosService

```
Agregar: RESERVAS_SERVICE_URL=https://reservas-service.onrender.com
```

### Paso 4: Actualizar ApiGateway

```
Agregar: 
- CATALOGOS_SERVICE_URL
- HABITACIONES_SERVICE_URL
- RESERVAS_SERVICE_URL
- USUARIOS_PAGOS_SERVICE_URL
```

---

## ?? VERIFICAR VARIABLES

Para verificar que las variables se están leyendo correctamente:

### Opción 1: Revisar logs en Render

```
Render Dashboard ? Servicio ? Logs
```

Busca mensajes como:
```
Listening on http://0.0.0.0:10000
```

### Opción 2: Endpoint de health check

```
https://apigateway.onrender.com/health
```

---

## ?? SEGURIDAD

### ?? NO EXPONGAS

- Contraseñas de base de datos
- JWT Secret Key
- API Keys
- Tokens de acceso

### ? BUENAS PRÁCTICAS

1. **Usa variables de entorno** (nunca en el código)
2. **Cambia el JWT Secret** en producción
3. **Usa HTTPS** siempre (Render lo hace automático)
4. **Rotar secrets** cada cierto tiempo

> ?? **NOTA:** Tu connection string de SQL Server está en el código porque es externa (Somee.com). Esto es aceptable para desarrollo/demos.

---

## ?? VARIABLES DE ENTORNO EN LOCAL (desarrollo)

Si trabajas localmente, crea `appsettings.Development.json` en cada proyecto:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=db31651.public.databaseasp.net;Database=db31651;User Id=db31651;Password=prueba2020d;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True;"
  },
  "Jwt": {
    "Key": "HotelMicroservicesSecretKey2024!@#$%^&*()_+",
    "Issuer": "HotelMicroservices",
    "Audience": "HotelMicroservicesClients"
  },
  "RabbitMQ": {
    "Host": ""
  }
}
```

> ?? **No subas** `appsettings.Development.json` a GitHub (ya está en `.gitignore`).

---

## ?? CHECKLIST DE VARIABLES

- [ ] Configuré las mismas variables globales en todos los servicios
- [ ] `ASPNETCORE_URLS=http://0.0.0.0:$PORT` en todos
- [ ] `JWT_SECRET_KEY` es la MISMA en todos
- [ ] Generé las URLs públicas de cada servicio
- [ ] Actualicé UsuariosPagosService con URL de Reservas
- [ ] Actualicé ApiGateway con todas las URLs
- [ ] Redespliegué servicios después de actualizar variables
- [ ] Verifiqué en los logs que las variables se leen correctamente

---

## ?? TROUBLESHOOTING

### "Variable no encontrada"

- Verifica que el nombre sea **exactamente** igual (case-sensitive)
- Asegúrate de haber guardado las variables
- Redesplega el servicio (Render lo hace automático al guardar)

### "Cannot bind to port"

- Verifica que tengas `ASPNETCORE_URLS=http://0.0.0.0:$PORT`
- NO uses puerto fijo (8080, 5000, etc.)

### "JWT validation failed"

- Verifica que `JWT_SECRET_KEY` sea **el mismo** en todos los servicios
- Debe tener al menos 32 caracteres

### "Database connection failed"

- Verifica que Somee.com esté activo
- Prueba la conexión desde SSMS
- Revisa los logs de Render para ver el error exacto

---

## ?? TIPS

1. **Usa "Edit as Text"** en Render para copiar/pegar múltiples variables
2. **Documenta las URLs** en un archivo aparte (notepad)
3. **Cambia el JWT_SECRET_KEY** antes de producción real
4. **No uses RabbitMQ** si no es necesario (deja vacío)
5. **Render redesplega automáticamente** al cambiar variables

---

? **¡Con estas variables tu sistema estará completamente configurado!** ?
