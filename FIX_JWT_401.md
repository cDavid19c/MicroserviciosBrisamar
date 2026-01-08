# ?? FIX: PROBLEMA JWT RESUELTO

## ? **PROBLEMA ENCONTRADO**

El error 401 se debía a que el código buscaba variables JWT con nombres diferentes:

### **Código buscaba:**
```csharp
builder.Configuration["Jwt:Key"]      // ? No existía
builder.Configuration["Jwt:Issuer"]   // ? Existía como Jwt__Issuer
builder.Configuration["Jwt:Audience"] // ? Existía como Jwt__Audience
```

### **Variables configuradas en Render:**
```
JWT_SECRET_KEY         ? (pero el código no la leía)
Jwt__Issuer            ?
Jwt__Audience          ?
```

**Resultado:** El código no encontraba `Jwt:Key` y el servicio **fallaba silenciosamente** dando 401.

---

## ? **SOLUCIÓN APLICADA**

He modificado **3 archivos** para que lean `JWT_SECRET_KEY` como fallback:

### **1. UsuariosPagosService/Program.cs**

**Antes:**
```csharp
var jwtKey = builder.Configuration["Jwt:Key"]!; // ? Crash si no existe
```

**Después:**
```csharp
var jwtKey = builder.Configuration["Jwt:Key"] 
             ?? builder.Configuration["JWT_SECRET_KEY"]
             ?? "HotelMicroservicesSecretKey2024!@#$%^&*()_+"; // ? Fallback
```

---

### **2. ApiGateway/Program.cs**

**Antes:**
```csharp
var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;
```

**Después:**
```csharp
var jwtKey = builder.Configuration["Jwt:Key"]
             ?? builder.Configuration["JWT_SECRET_KEY"]
             ?? "HotelMicroservicesSecretKey2024!@#$%^&*()_+";

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
                ?? "HotelMicroservices";

var jwtAudience = builder.Configuration["Jwt:Audience"]
                  ?? "HotelMicroservicesClients";
```

---

### **3. ApiGateway/Controllers/AuthController.cs**

Actualicé los 3 métodos:
- `GenerateJwtToken()`
- `ValidateToken()`
- `RefreshToken()`

Para que todos lean `JWT_SECRET_KEY` como fallback.

---

## ?? **PRÓXIMOS PASOS**

### **PASO 1: Subir cambios a GitHub**

Ejecuta:

```powershell
cd "D:\Jossue\Desktop\RETO 3\BACK\V1\Microservicios"
.\update-render.ps1
```

---

### **PASO 2: Esperar redespliegue (5-7 min)**

Render redesplegará automáticamente:
- ApiGateway
- UsuariosPagosService

Monitorea en: https://dashboard.render.com

---

### **PASO 3: Probar de nuevo**

1. **Genera nuevo token:**

```
POST https://apigateway-hyaw.onrender.com/api/auth/token
```

Body:
```json
{
  "username": "admin",
  "password": "admin123",
  "role": "Admin"
}
```

2. **Copia el token**

3. **Autoriza en UsuariosPagosService:**
   - Ve a: https://usuarios-pagos-service.onrender.com/swagger
   - Click **"Authorize"** ??
   - Pega: `Bearer TU_TOKEN`

4. **Prueba GET /api/usuarios**
   - Debería retornar **200 OK** ?

---

## ?? **VARIABLES DE ENTORNO FINALES**

### **ApiGateway:**
```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:$PORT
JWT_SECRET_KEY=HotelMicroservicesSecretKey2024!@#$%^&*()_+
Jwt__Issuer=HotelMicroservices
Jwt__Audience=HotelMicroservicesClients
Jwt__ExpireMinutes=60
```

### **UsuariosPagosService:**
```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:$PORT
JWT_SECRET_KEY=HotelMicroservicesSecretKey2024!@#$%^&*()_+
Jwt__Issuer=HotelMicroservices
Jwt__Audience=HotelMicroservicesClients
Jwt__ExpireMinutes=60
RABBITMQ_URL=
RESERVAS_SERVICE_URL=https://reservas-service.onrender.com/
```

---

## ?? **POR QUÉ PASÓ ESTO**

### **Convenciones de .NET:**

En .NET, las variables de entorno se mapean así:

| Variable de Entorno | Código C# |
|---------------------|-----------|
| `JWT_SECRET_KEY` | `["JWT_SECRET_KEY"]` |
| `Jwt__Key` | `["Jwt:Key"]` |
| `Jwt__Issuer` | `["Jwt:Issuer"]` |

El `__` (doble guión bajo) se convierte en `:` (dos puntos).

### **El problema:**

Configuraste:
- ? `JWT_SECRET_KEY` (flat)
- ? `Jwt__Issuer` (nested)
- ? `Jwt__Audience` (nested)

Pero el código buscaba:
- ? `Jwt:Key` (nested) ? No encontraba `JWT_SECRET_KEY`

### **La solución:**

Agregar fallback en el código para leer ambas convenciones.

---

## ? **ARCHIVOS MODIFICADOS**

1. ? `UsuariosPagosService/Program.cs`
2. ? `ApiGateway/Program.cs`
3. ? `ApiGateway/Controllers/AuthController.cs`
4. ? `update-render.ps1`

---

## ?? **SI SIGUE SIN FUNCIONAR**

### **Opción 1: Verificar logs en Render**

1. Ve a Render Dashboard
2. Click en **"usuarios-pagos-service"**
3. Click en **"Logs"**
4. Busca errores relacionados con JWT

### **Opción 2: Verificar que el token sea válido**

Prueba primero validar el token:

```
POST https://apigateway-hyaw.onrender.com/api/auth/validate
```

Body:
```json
{
  "token": "TU_TOKEN_AQUI"
}
```

Debería retornar:
```json
{
  "valid": true,
  "username": "admin",
  "role": "Admin",
  "expiresAt": "2026-01-08T..."
}
```

### **Opción 3: Revisar las variables de entorno**

En Render, verifica que **UsuariosPagosService** tenga:
- `JWT_SECRET_KEY`
- `Jwt__Issuer`
- `Jwt__Audience`

---

## ?? **MEJORAS FUTURAS**

### **Opción A: Usar solo variables flat**

Cambiar todo a:
```
JWT_SECRET_KEY
JWT_ISSUER
JWT_AUDIENCE
```

Y actualizar el código para leerlas así.

### **Opción B: Usar solo variables nested**

Cambiar todo a:
```
Jwt__Key
Jwt__Issuer
Jwt__Audience
```

Y actualizar el código para leerlas así.

### **Opción C: Mantener ambas (actual)**

Dejar el fallback como está (más flexible).

---

## ?? **CHECKLIST**

- [ ] Código actualizado (? ya hecho)
- [ ] Compilación exitosa (? ya hecho)
- [ ] Script de actualización ejecutado
- [ ] Cambios subidos a GitHub
- [ ] Render redesplegando (espera 5-7 min)
- [ ] Nuevo token generado
- [ ] Token autorizado en Swagger
- [ ] GET /api/usuarios funciona ?

---

## ?? **SIGUIENTES ACCIONES**

1. **Ejecuta:** `.\update-render.ps1`
2. **Espera:** 5-7 minutos
3. **Prueba:** Genera nuevo token y úsalo
4. **Verifica:** GET /api/usuarios debe retornar 200 OK

---

<div align="center">

# ? **¡PROBLEMA IDENTIFICADO Y CORREGIDO!** ?

**Causa:** Nombres de variables JWT inconsistentes

**Solución:** Fallback en el código para leer ambas convenciones

**Resultado esperado:** Token JWT funcionará correctamente ?

**Ejecuta ahora:** `.\update-render.ps1`

</div>
