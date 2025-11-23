# Diagnose-Skript für Ladestation gvh-01
Write-Host "=== Diagnose für Ladestation gvh-01 ===" -ForegroundColor Cyan
Write-Host ""

# 1. Prüfe Datenbank-Einträge
Write-Host "1. Datenbank-Status:" -ForegroundColor Yellow
sqlcmd -S localhost -d ChargingControlSystem -Q "SELECT cs.StationId, cs.Name, cs.ChargeBoxId, cs.Status, cs.LastHeartbeat, cs.IsActive FROM ChargingStations cs WHERE cs.StationId = 'gvh-01' OR cs.ChargeBoxId = 'gvh-01'" -W -s ","

Write-Host ""
Write-Host "2. OCPP WebSocket Server Port:" -ForegroundColor Yellow
$ocppPort = netstat -ano | Select-String ":9000" | Select-String "LISTENING"
if ($ocppPort) {
    Write-Host "✓ OCPP Server läuft auf Port 9000" -ForegroundColor Green
    Write-Host $ocppPort
} else {
    Write-Host "✗ OCPP Server läuft NICHT auf Port 9000!" -ForegroundColor Red
}

Write-Host ""
Write-Host "3. Backend API Status:" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5126/health" -UseBasicParsing -TimeoutSec 3
    Write-Host "✓ Backend API läuft (Port 5126)" -ForegroundColor Green
} catch {
    Write-Host "✗ Backend API antwortet nicht!" -ForegroundColor Red
}

Write-Host ""
Write-Host "4. Backend Logs (letzte 20 Zeilen mit 'gvh-01'):" -ForegroundColor Yellow
$logFile = "c:\Users\User\.cursor\projects\d-CC-40\terminals\1.txt"
if (Test-Path $logFile) {
    Get-Content $logFile | Select-String "gvh-01" | Select-Object -Last 20
} else {
    Write-Host "Log-Datei nicht gefunden"
}

Write-Host ""
Write-Host "=== Diagnose abgeschlossen ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Mögliche Ursachen wenn Station offline angezeigt wird:" -ForegroundColor Yellow
Write-Host "1. ChargeBoxId in Datenbank stimmt nicht mit der ID überein, die die Station sendet"
Write-Host "2. Station sendet keine BootNotification oder Heartbeat"
Write-Host "3. LastHeartbeat ist älter als 10 Minuten"
Write-Host "4. Station ist in Datenbank nicht aktiv (IsActive = 0)"

