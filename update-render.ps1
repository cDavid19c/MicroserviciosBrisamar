# Script para actualizar el código en GitHub y triggear redespliegue en Render

Write-Host "========================================" -ForegroundColor Cyan
Write-Host " ACTUALIZANDO CÓDIGO EN GITHUB" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 1. Agregar todos los cambios
Write-Host "[1/4] Agregando cambios..." -ForegroundColor Yellow
git add .

if ($LASTEXITCODE -ne 0) {
    Write-Host "? Error al agregar archivos" -ForegroundColor Red
    exit 1
}

Write-Host "? Cambios agregados" -ForegroundColor Green
Write-Host ""

# 2. Commit
Write-Host "[2/4] Creando commit..." -ForegroundColor Yellow
$mensaje = "Improve: Swagger auto-agrega Bearer en token JWT"
git commit -m $mensaje

if ($LASTEXITCODE -ne 0) {
    Write-Host "??  No hay cambios para commit o error" -ForegroundColor Yellow
} else {
    Write-Host "? Commit creado: $mensaje" -ForegroundColor Green
}

Write-Host ""

# 3. Push
Write-Host "[3/4] Subiendo a GitHub..." -ForegroundColor Yellow
git push

if ($LASTEXITCODE -ne 0) {
    Write-Host "? Error al subir a GitHub" -ForegroundColor Red
    Write-Host "Verifica tu conexión y autenticación" -ForegroundColor Yellow
    exit 1
}

Write-Host "? Código subido a GitHub" -ForegroundColor Green
Write-Host ""

# 4. Información
Write-Host "[4/4] Siguiente paso" -ForegroundColor Yellow
Write-Host ""
Write-Host "? Render detectará el cambio automáticamente" -ForegroundColor Cyan
Write-Host "? Espera 5-7 minutos mientras redesplega" -ForegroundColor Cyan
Write-Host ""
Write-Host "?? Monitorea el progreso en:" -ForegroundColor White
Write-Host "   https://dashboard.render.com" -ForegroundColor Blue
Write-Host ""
Write-Host "?? Servicios que se redespliegan:" -ForegroundColor White
Write-Host "   - ApiGateway" -ForegroundColor Yellow
Write-Host "   - UsuariosPagosService" -ForegroundColor Yellow
Write-Host "   - CatalogosService" -ForegroundColor Yellow
Write-Host ""
Write-Host "?? Después del redespliegue:" -ForegroundColor White
Write-Host "   1. Genera token en ApiGateway" -ForegroundColor Cyan
Write-Host "   2. En Swagger, click 'Authorize'" -ForegroundColor Cyan
Write-Host "   3. Pega SOLO el token (sin 'Bearer')" -ForegroundColor Green
Write-Host "   4. Swagger agregará 'Bearer' automáticamente" -ForegroundColor Green
Write-Host "   5. Prueba GET /api/usuarios" -ForegroundColor Cyan
Write-Host ""
Write-Host "?? MEJORA APLICADA:" -ForegroundColor White
Write-Host "   Ahora solo necesitas pegar el token" -ForegroundColor Green
Write-Host "   NO escribas 'Bearer' manualmente" -ForegroundColor Green
Write-Host ""
Write-Host "?? URLs:" -ForegroundColor White
Write-Host "   ApiGateway:           https://apigateway-hyaw.onrender.com/swagger" -ForegroundColor Blue
Write-Host "   UsuariosPagosService: https://usuarios-pagos-service.onrender.com/swagger" -ForegroundColor Blue
Write-Host "   CatalogosService:     https://catalogos-service.onrender.com/swagger" -ForegroundColor Blue
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host " ? ACTUALIZACIÓN COMPLETA" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
