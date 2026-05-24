$ErrorActionPreference = "Stop"

Write-Host "Verificando disponibilidad de Docker..." -ForegroundColor Cyan

try {
    # Ejecuta 'docker info' y captura solo el código de salida
    $process = Start-Process -FilePath "docker" -ArgumentList "info" -Wait -NoNewWindow -PassThru
    
    if ($process.ExitCode -eq 0) {
        Write-Host "✅ Docker está ejecutándose y accesible." -ForegroundColor Green
        exit 0
    } else {
        Write-Host "❌ Docker devolvió el código de salida: $($process.ExitCode). Asegúrese de que el demonio de Docker esté ejecutándose." -ForegroundColor Red
        exit 1
    }
}
catch {
    Write-Host "❌ No se encontró el comando 'docker'. Instale Docker o añádalo al PATH." -ForegroundColor Red
    exit 1
}
