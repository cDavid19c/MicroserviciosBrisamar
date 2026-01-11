# Script para actualizar el código en GitHub y triggear redespliegue en Render

Write-Host "========================================" -ForegroundColor Cyan
Write-Host " ACTUALIZANDO CÓDIGO EN GITHUB" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 1. Agregar todos los cambios
Write-Host "[1/4] Agregando cambios..." -ForegroundColor Yellow
git add .

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Error al agregar archivos" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Cambios agregados" -ForegroundColor Green
Write-Host ""

# 2. Commit
Write-Host "[2/4] Creando commit..." -ForegroundColor Yellow
$mensaje = "Fix: Corregir error de clave duplicada en fechas-ocupadas gRPC"
git commit -m $mensaje

if ($LASTEXITCODE -ne 0) {
    Write-Host "⚠️  No hay cambios para commit o error" -ForegroundColor Yellow
} else {
    Write-Host "✅ Commit creado: $mensaje" -ForegroundColor Green
}

Write-Host ""

# 3. Push
Write-Host "[3/4] Subiendo a GitHub..." -ForegroundColor Yellow
git push

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Error al subir a GitHub" -ForegroundColor Red
    Write-Host "Verifica tu conexión y autenticación" -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ Código subido a GitHub" -ForegroundColor Green
Write-Host ""

# 4. Información
Write-Host "[4/4] Siguiente paso" -ForegroundColor Yellow
Write-Host ""
Write-Host "✨ Render detectará el cambio automáticamente" -ForegroundColor Cyan
Write-Host "⏳ Espera 5-7 minutos mientras redesplega" -ForegroundColor Cyan
Write-Host ""
Write-Host "📊 Monitorea el progreso en:" -ForegroundColor White
Write-Host "   https://dashboard.render.com" -ForegroundColor Blue
Write-Host ""
Write-Host "🔍 Servicio que se redesplegará:" -ForegroundColor White
Write-Host "   - ApiGateway" -ForegroundColor Yellow
Write-Host ""
Write-Host "📝 Cambio aplicado:" -ForegroundColor White
Write-Host "   ✅ Corregida lógica de fechas-ocupadas" -ForegroundColor Green
Write-Host "   ✅ Ahora maneja múltiples HabxRes por reserva" -ForegroundColor Green
Write-Host "   ✅ Evita error: 'An item with the same key has already been added'" -ForegroundColor Green
Write-Host ""
Write-Host "🧪 Después del redespliegue prueba:" -ForegroundColor White
Write-Host "   GET /api/reservas-grpc/fechas-ocupadas/HAJO000001" -ForegroundColor Cyan
Write-Host ""
Write-Host "✅ Respuesta esperada:" -ForegroundColor White
Write-Host "   {" -ForegroundColor Gray
Write-Host '     "success": true,' -ForegroundColor Gray
Write-Host '     "idHabitacion": "HAJO000001",' -ForegroundColor Gray
Write-Host '     "fechasOcupadas": ["2026-01-11", "2026-01-12", ...],' -ForegroundColor Gray
Write-Host '     "totalFechas": 150' -ForegroundColor Gray
Write-Host "   }" -ForegroundColor Gray
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host " ✅ ACTUALIZACIÓN COMPLETA" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
