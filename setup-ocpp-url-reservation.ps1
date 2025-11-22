# OCPP Server URL-Reservation Setup
# Dieses Skript muss als Administrator ausgeführt werden!

Write-Host "==================================================================" -ForegroundColor Cyan
Write-Host "  OCPP Server URL-Reservation Setup" -ForegroundColor Cyan
Write-Host "==================================================================" -ForegroundColor Cyan
Write-Host ""

# Prüfe Admin-Rechte
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host "FEHLER: Dieses Skript muss als Administrator ausgeführt werden!" -ForegroundColor Red
    Write-Host "Rechtsklick auf die Datei -> 'Als Administrator ausführen'" -ForegroundColor Yellow
    Write-Host ""
    pause
    exit 1
}

Write-Host "Administrator-Rechte: OK" -ForegroundColor Green
Write-Host ""

# IP-Adresse und Port
$ipAddress = "192.168.178.121"
$port = "9000"
$url = "http://${ipAddress}:${port}/"

Write-Host "Konfiguration:" -ForegroundColor Yellow
Write-Host "  IP-Adresse: $ipAddress" -ForegroundColor White
Write-Host "  Port:       $port" -ForegroundColor White
Write-Host "  URL:        $url" -ForegroundColor White
Write-Host ""

# Prüfe, ob bereits eine Reservation existiert
Write-Host "Prüfe bestehende URL-Reservationen..." -ForegroundColor Yellow
$existing = netsh http show urlacl | Select-String "$ipAddress:$port"

if ($existing) {
    Write-Host "Es existiert bereits eine Reservation für diese URL:" -ForegroundColor Yellow
    Write-Host $existing -ForegroundColor White
    Write-Host ""
    $remove = Read-Host "Möchten Sie diese zuerst entfernen? (j/n)"
    
    if ($remove -eq "j" -or $remove -eq "J") {
        Write-Host "Entferne alte Reservation..." -ForegroundColor Yellow
        netsh http delete urlacl url=$url
        Write-Host "Alte Reservation entfernt." -ForegroundColor Green
        Write-Host ""
    }
}

# Erstelle neue URL-Reservation
Write-Host "Erstelle URL-Reservation..." -ForegroundColor Yellow
$result = netsh http add urlacl url=$url user=JEDER

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "✓ URL-Reservation erfolgreich erstellt!" -ForegroundColor Green
    Write-Host ""
} else {
    Write-Host ""
    Write-Host "✗ Fehler beim Erstellen der URL-Reservation!" -ForegroundColor Red
    Write-Host $result -ForegroundColor Red
    Write-Host ""
    pause
    exit 1
}

# Prüfe Firewall-Regel
Write-Host "Prüfe Firewall-Regel..." -ForegroundColor Yellow
$firewallRule = Get-NetFirewallRule -DisplayName "OCPP Server Port 9000" -ErrorAction SilentlyContinue

if ($firewallRule) {
    Write-Host "✓ Firewall-Regel existiert bereits" -ForegroundColor Green
} else {
    Write-Host "Erstelle Firewall-Regel..." -ForegroundColor Yellow
    New-NetFirewallRule -DisplayName "OCPP Server Port 9000" -Direction Inbound -Protocol TCP -LocalPort $port -Action Allow | Out-Null
    Write-Host "✓ Firewall-Regel erstellt" -ForegroundColor Green
}

Write-Host ""
Write-Host "==================================================================" -ForegroundColor Cyan
Write-Host "  Setup abgeschlossen!" -ForegroundColor Green
Write-Host "==================================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Die folgenden Einstellungen wurden konfiguriert:" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. URL-Reservation:" -ForegroundColor White
Write-Host "   $url" -ForegroundColor Cyan
Write-Host ""
Write-Host "2. Firewall-Regel:" -ForegroundColor White
Write-Host "   Port $port (TCP Inbound) erlaubt" -ForegroundColor Cyan
Write-Host ""
Write-Host "Sie können nun die Anwendung OHNE Admin-Rechte starten:" -ForegroundColor Yellow
Write-Host "   cd D:\CC.40\ChargingControlSystem.Api" -ForegroundColor White
Write-Host "   dotnet run" -ForegroundColor White
Write-Host ""
Write-Host "WebSocket-URL für Ihre Ladestation:" -ForegroundColor Yellow
Write-Host "   ws://${ipAddress}:${port}/ocpp/IHR-STATION-ID" -ForegroundColor Cyan
Write-Host ""
Write-Host "Beispiel:" -ForegroundColor Yellow
Write-Host "   ws://${ipAddress}:${port}/ocpp/HOME-001" -ForegroundColor Cyan
Write-Host ""

# Zeige aktuelle Reservationen
Write-Host "==================================================================" -ForegroundColor Cyan
Write-Host "  Aktuelle URL-Reservationen für Port $port:" -ForegroundColor Cyan
Write-Host "==================================================================" -ForegroundColor Cyan
Write-Host ""
netsh http show urlacl | Select-String ":$port" -Context 0,5

Write-Host ""
Write-Host "Drücken Sie eine beliebige Taste zum Beenden..." -ForegroundColor Yellow
pause

