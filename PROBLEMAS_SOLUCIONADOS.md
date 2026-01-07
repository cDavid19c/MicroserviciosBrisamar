# ?? PROBLEMAS ENCONTRADOS Y SOLUCIONADOS

## ? **PROBLEMA PRINCIPAL**

Tus servicios NO podían arrancar en Render porque **los puertos estaban hardcodeados**.

Render asigna dinámicamente el puerto a través de la variable de entorno `$PORT`, pero tu código ignoraba esta variable y escuchaba siempre en el puerto 8080.

---

## ? **SOLUCIONES APLICADAS**

### **1. ApiGateway** - `Program.cs`

**Antes (INCORRECTO):**
```csharp
app.Urls.Add("http://0.0.0.0:8080");  // ? Puerto fijo
app.Run();
```

**Después (CORRECTO):**
```csharp
// NO forzar puerto - usar ASPNETCORE_URLS de variables de entorno
app.Run();  // ? Lee de ASPNETCORE_URLS
```

**Variable de entorno en Render:**
```
ASPNETCORE_URLS=http://0.0.0.0:$PORT
```

---

### **2. ReservasService** - `Program.cs`

**Antes (INCORRECTO):**
```csharp
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080, listenOptions =>  // ? Puerto fijo
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});
```

**Después (CORRECTO):**
```csharp
// Usar puerto dinámico de Render ($PORT)
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(port), listenOptions =>  // ? Puerto dinámico
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});
```

---

### **3. UsuariosPagosService** - `Program.cs`

**Problema adicional:** URL de gRPC hardcodeada

**Antes (INCORRECTO):**
```csharp
builder.Services.AddGrpcClient<ReservasGrpc.ReservasGrpcClient>(options =>
{
    options.Address = new Uri(
        builder.Environment.IsDevelopment()
            ? "http://localhost:5003"
            : "http://reservas:8080");  // ? URL hardcodeada
});
```

**Después (CORRECTO):**
```csharp
var reservasUrl = builder.Configuration["RESERVAS_SERVICE_URL"];

if (string.IsNullOrWhiteSpace(reservasUrl))
{
    reservasUrl = builder.Environment.IsDevelopment()
        ? "http://localhost:5003"
        : "http://reservas:8080";
}

reservasUrl = reservasUrl.TrimEnd('/');

builder.Services.AddGrpcClient<ReservasGrpc.ReservasGrpcClient>(options =>
{
    options.Address = new Uri(reservasUrl);  // ? Lee de variable de entorno
});
```

**Variable en Render:**
```
RESERVAS_SERVICE_URL=https://reservas-service.onrender.com
```

---

### **4. Swagger habilitado en producción**

Se habilitó Swagger en **todos los servicios** para poder probar en Render:

- ? `ApiGateway/Program.cs`
- ? `CatalogosService/Program.cs`
- ? `UsuariosPagosService/Program.cs`

```csharp
if (app.Environment.IsDevelopment()
    || app.Environment.EnvironmentName == "Docker"
    || app.Environment.IsProduction())  // ? Agregado
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

---

## ?? **ARCHIVOS MODIFICADOS**

1. ? `ApiGateway/Program.cs`
2. ? `ReservasService/Program.cs`
3. ? `UsuariosPagosService/Program.cs`
4. ? `CatalogosService/Program.cs`

---

## ?? **PRÓXIMOS PASOS**

### **PASO 1: Subir cambios a GitHub**

Ejecuta el script que creé:

```powershell
cd "D:\Jossue\Desktop\RETO 3\BACK\V1\Microservicios"
.\update-render.ps1
```

O manualmente:

```powershell
git add .
git commit -m "Fix: Eliminar puertos hardcodeados para Render"
git push
```

---

### **PASO 2: Esperar redespliegue en Render**

Render detectará automáticamente el push y redesplegará los servicios.

**Tiempo estimado:** 5-7 minutos por servicio

---

### **PASO 3: Monitorear los LOGS**

1. Ve a https://dashboard.render.com
2. Click en **"apigateway-hyaw"**
3. Click en **"Logs"**
4. Busca este mensaje:

```
? ÉXITO:
Now listening on: http://0.0.0.0:10000
Application started. Press Ctrl+C to shut down.
```

Si ves ese mensaje, el servicio arrancó correctamente.

---

### **PASO 4: Probar Swagger**

Después de que los logs muestren "Application started", prueba:

```
https://apigateway-hyaw.onrender.com/swagger
```

? **Nota:** Si tarda 30-60 segundos, es **normal** (cold start).

---

### **PASO 5: Probar endpoint**

En Swagger, prueba:

```
GET /api/catalogos/ciudades
```

Debería retornar las ciudades de tu SQL Server en Somee.com.

---

## ?? **VERIFICACIÓN DE LOGS**

### **Logs BUENOS (?):**

```
Now listening on: http://0.0.0.0:10000
Application started. Press Ctrl+C to shut down.
Hosting environment: Production
Content root path: /app
```

### **Logs MALOS (?):**

```
Failed to bind to address http://0.0.0.0:8080: address already in use.
```

o

```
Unable to start Kestrel.
System.IO.IOException: Failed to bind to address...
```

---

## ?? **SI SIGUE SIN FUNCIONAR**

### **Opción 1: Verificar variables de entorno**

En Render, asegúrate de tener:

```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:$PORT  ? IMPORTANTE
JWT_SECRET_KEY=HotelMicroservicesSecretKey2024!@#$%^&*()_+
```

### **Opción 2: Revisar Dockerfile**

El Dockerfile NO debe tener `EXPOSE` o `ENV` forzando un puerto específico.

**Correcto:**
```dockerfile
# Sin EXPOSE o con EXPOSE 8080 (solo informativo)
ENV ASPNETCORE_URLS=http://+:8080  
```

Render sobrescribirá esto con `ASPNETCORE_URLS=http://0.0.0.0:$PORT`

### **Opción 3: Rebuild manual**

En Render:
1. Click en el servicio
2. **"Manual Deploy"** ? **"Clear build cache & deploy"**

---

## ?? **POR QUÉ PASÓ ESTO**

Render asigna un puerto **aleatorio** (ejemplo: 10000, 10234, etc.) a cada servicio.

Este puerto se pasa a través de la variable de entorno `$PORT`.

Si tu aplicación:
- ? Lee `ASPNETCORE_URLS=http://0.0.0.0:$PORT` ? Funciona
- ? Usa `app.Urls.Add("http://0.0.0.0:8080")` ? Falla

---

## ? **CHECKLIST**

- [x] Código corregido
- [x] Compilación exitosa
- [ ] Cambios subidos a GitHub
- [ ] Render redesplegando
- [ ] Logs muestran "Now listening on..."
- [ ] Swagger abre correctamente
- [ ] Endpoints retornan datos

---

## ?? **RESULTADO ESPERADO**

Después de subir los cambios, todos tus servicios deberían funcionar:

```
? https://apigateway-hyaw.onrender.com/swagger
? https://catalogos-service.onrender.com/swagger
? https://habitaciones-service.onrender.com/graphql
? https://reservas-service.onrender.com/health
? https://usuarios-pagos-service.onrender.com/swagger
```

---

<div align="center">

# ? **¡LISTO PARA ACTUALIZAR!** ?

**Ejecuta:** `.\update-render.ps1`

**Espera 5-7 minutos**

**Prueba:** `https://apigateway-hyaw.onrender.com/swagger`

</div>
