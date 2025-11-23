# ========================================
# Datenbank Refresh-Skript
# ========================================

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("update", "drop", "reset", "seed")]
    [string]$Action = "update",
    
    [Parameter(Mandatory=$false)]
    [string]$ConnectionString = ""
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Datenbank Refresh Tool" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$projectPath = "ChargingControlSystem.Data"
$startupProject = "ChargingControlSystem.Api"

# Prüfen ob dotnet ef tools installiert sind
$efInstalled = dotnet tool list -g | Select-String "dotnet-ef"
if (-not $efInstalled) {
    Write-Host "⚠️  dotnet-ef tools nicht gefunden. Installiere..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef
}

function Update-Database {
    Write-Host "🔄 Führe Datenbank-Migrationen aus..." -ForegroundColor Yellow
    Write-Host ""
    
    Push-Location $projectPath
    try {
        dotnet ef database update --startup-project "../$startupProject"
        if ($LASTEXITCODE -eq 0) {
            Write-Host ""
            Write-Host "✅ Datenbank erfolgreich aktualisiert!" -ForegroundColor Green
        } else {
            Write-Host ""
            Write-Host "❌ Fehler beim Aktualisieren der Datenbank!" -ForegroundColor Red
            exit 1
        }
    } finally {
        Pop-Location
    }
}

function Drop-Database {
    Write-Host "🗑️  Lösche Datenbank..." -ForegroundColor Yellow
    Write-Host ""
    
    $confirm = Read-Host "⚠️  WARNUNG: Alle Daten werden gelöscht! Fortfahren? (j/n)"
    if ($confirm -ne "j" -and $confirm -ne "J" -and $confirm -ne "y" -and $confirm -ne "Y") {
        Write-Host "Abgebrochen." -ForegroundColor Yellow
        exit 0
    }
    
    Push-Location $projectPath
    try {
        dotnet ef database drop --startup-project "../$startupProject" --force
        if ($LASTEXITCODE -eq 0) {
            Write-Host ""
            Write-Host "✅ Datenbank erfolgreich gelöscht!" -ForegroundColor Green
        } else {
            Write-Host ""
            Write-Host "❌ Fehler beim Löschen der Datenbank!" -ForegroundColor Red
            exit 1
        }
    } finally {
        Pop-Location
    }
}

function Reset-Database {
    Write-Host "🔄 Setze Datenbank zurück (Drop + Update)..." -ForegroundColor Yellow
    Write-Host ""
    
    $confirm = Read-Host "⚠️  WARNUNG: Alle Daten werden gelöscht und neu erstellt! Fortfahren? (j/n)"
    if ($confirm -ne "j" -and $confirm -ne "J" -and $confirm -ne "y" -and $confirm -ne "Y") {
        Write-Host "Abgebrochen." -ForegroundColor Yellow
        exit 0
    }
    
    Drop-Database
    Write-Host ""
    Update-Database
}

function Show-Migrations {
    Write-Host "📋 Zeige Migrationen..." -ForegroundColor Yellow
    Write-Host ""
    
    Push-Location $projectPath
    try {
        dotnet ef migrations list --startup-project "../$startupProject"
    } finally {
        Pop-Location
    }
}

# Hauptlogik
switch ($Action) {
    "update" {
        Update-Database
    }
    "drop" {
        Drop-Database
    }
    "reset" {
        Reset-Database
    }
    "seed" {
        Write-Host "ℹ️  Seed-Daten werden automatisch beim Erstellen der Datenbank eingefügt." -ForegroundColor Cyan
        Write-Host "   Verwenden Sie 'reset' um die Datenbank neu zu erstellen." -ForegroundColor Cyan
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Fertig!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan


