# ? MEJORA: SWAGGER AUTO-AGREGA "BEARER"

## ?? **CAMBIO APLICADO**

He configurado Swagger para que **automáticamente** agregue el prefijo "Bearer" al token JWT.

---

## ?? **ANTES (Manual)**

```
1. Generar token
2. Click "Authorize"
3. Escribir: "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
              ^^^^^^ Tenías que escribir esto manualmente
4. Click "Authorize"
```

---

## ? **AHORA (Automático)**

```
1. Generar token
2. Click "Authorize"
3. Pegar: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
          Solo el token, sin "Bearer"
4. Click "Authorize"
5. Swagger agrega "Bearer" automáticamente ?
```

---

## ?? **CAMBIOS TÉCNICOS**

He modificado la configuración de Swagger en **3 servicios**:

### **1. UsuariosPagosService/Program.cs**

**Antes:**
```csharp
c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
{
    Name = "Authorization",
    Type = SecuritySchemeType.ApiKey,  // ? ApiKey
    Scheme = "Bearer",
    In = ParameterLocation.Header
});
```

**Después:**
```csharp
c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
{
    Name = "Authorization",
    Type = SecuritySchemeType.Http,        // ? Http
    Scheme = "Bearer",
    BearerFormat = "JWT",                  // ? Formato JWT
    In = ParameterLocation.Header,
    Description = "Ingresa solo el token JWT (sin 'Bearer')"
});
```

**Cambios clave:**
- `Type = SecuritySchemeType.Http` ? Activa la funcionalidad Bearer automática
- `BearerFormat = "JWT"` ? Especifica que es un token JWT
- `Description` actualizado ? Instrucciones claras

---

### **2. ApiGateway/Program.cs**

Mismo cambio aplicado.

---

### **3. CatalogosService/Program.cs**

Mismo cambio aplicado.

---

## ?? **DESPLEGAR CAMBIOS**

```powershell
cd "D:\Jossue\Desktop\RETO 3\BACK\V1\Microservicios"
.\update-render.ps1
```

Espera 5-7 minutos mientras Render redesplega.

---

## ?? **CÓMO PROBARLO**

### **PASO 1: Generar token**

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

Respuesta:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 3600,
  "tokenType": "Bearer"
}
```

---

### **PASO 2: Copiar SOLO el token**

Copia esto:
```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**NO copies:**
```
Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
       ^^^^^^ NO incluyas esto
```

---

### **PASO 3: Autorizar en Swagger**

1. Ve a: https://usuarios-pagos-service.onrender.com/swagger
2. Click **"Authorize"** ??
3. **Pega SOLO el token:**
   ```
   eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
   ```
4. Click **"Authorize"**
5. Click **"Close"**

Swagger enviará automáticamente:
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

### **PASO 4: Probar endpoint**

Prueba:
```
GET /api/usuarios
```

Debería retornar **200 OK** ?

---

## ?? **SERVICIOS ACTUALIZADOS**

| Servicio | Swagger URL | Auto-Bearer |
|----------|-------------|-------------|
| **ApiGateway** | /swagger | ? Sí |
| **UsuariosPagosService** | /swagger | ? Sí |
| **CatalogosService** | /swagger | ? Sí |
| **HabitacionesService** | /graphql | N/A (GraphQL) |
| **ReservasService** | /health | N/A (gRPC) |

---

## ?? **COMPARACIÓN VISUAL**

### **Antes:**
```
+------------------------+
| Authorize              |
+------------------------+
| Value:                 |
| Bearer eyJhbGc...      |  ? Escribías manualmente
|                        |
| [Authorize] [Cancel]   |
+------------------------+
```

### **Ahora:**
```
+------------------------+
| Authorize              |
+------------------------+
| Value:                 |
| eyJhbGc...             |  ? Solo pegas el token
|                        |
| [Authorize] [Cancel]   |
+------------------------+

Swagger envía:
Authorization: Bearer eyJhbGc...
              ^^^^^^ Auto-agregado
```

---

## ? **VENTAJAS**

1. ? **Más fácil** - Solo copias y pegas el token
2. ? **Menos errores** - No olvidas escribir "Bearer"
3. ? **Más rápido** - Un paso menos
4. ? **Mejor UX** - Más intuitivo
5. ? **Estándar** - Así lo hacen la mayoría de APIs

---

## ?? **NOTA IMPORTANTE**

Esta mejora solo afecta a **Swagger UI**.

Si llamas a la API directamente (Postman, curl, código):
```
? CORRECTO:
Authorization: Bearer eyJhbGc...

? INCORRECTO:
Authorization: eyJhbGc...
```

Siempre necesitas el prefijo "Bearer" en el header.

---

## ?? **CHECKLIST**

- [x] Código actualizado (? ya hecho)
- [x] Compilación exitosa (? ya hecho)
- [ ] Script ejecutado
- [ ] Cambios subidos a GitHub
- [ ] Render redesplegando (5-7 min)
- [ ] Nuevo token generado
- [ ] Token pegado en Swagger (sin "Bearer")
- [ ] Endpoint probado (200 OK)

---

## ?? **PRÓXIMO PASO**

```powershell
.\update-render.ps1
```

Espera 5-7 minutos y prueba de nuevo.

---

<div align="center">

# ? **¡SWAGGER MEJORADO!** ?

**Ahora solo pegas el token**

**Sin escribir "Bearer" manualmente**

**Más fácil • Más rápido • Menos errores**

**Ejecuta:** `.\update-render.ps1`

</div>
