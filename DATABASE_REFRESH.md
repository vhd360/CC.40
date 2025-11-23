# Datenbank Refresh - Anleitung

## Übersicht

Dieses Dokument erklärt, wie Sie die Datenbank aktualisieren, zurücksetzen oder neu erstellen können.

## Optionen

### 1. Migrationen ausführen (Update)

Führt alle ausstehenden Migrationen aus, ohne Daten zu löschen:

```powershell
.\refresh-database.ps1 -Action update
```

Oder manuell:
```powershell
cd ChargingControlSystem.Data
dotnet ef database update --startup-project ../ChargingControlSystem.Api
```

### 2. Datenbank löschen (Drop)

Löscht die komplette Datenbank:

```powershell
.\refresh-database.ps1 -Action drop
```

Oder manuell:
```powershell
cd ChargingControlSystem.Data
dotnet ef database drop --startup-project ../ChargingControlSystem.Api --force
```

### 3. Datenbank zurücksetzen (Reset)

Löscht die Datenbank und erstellt sie neu mit allen Migrationen:

```powershell
.\refresh-database.ps1 -Action reset
```

**⚠️ WARNUNG:** Alle Daten gehen verloren!

### 4. Migrationen anzeigen

Zeigt alle verfügbaren Migrationen:

```powershell
cd ChargingControlSystem.Data
dotnet ef migrations list --startup-project ../ChargingControlSystem.Api
```

## Neue Migration erstellen

Wenn Sie Änderungen an den Entities vorgenommen haben:

```powershell
cd ChargingControlSystem.Data
dotnet ef migrations add BeschreibungDerAenderung --startup-project ../ChargingControlSystem.Api
```

Dann Migration ausführen:
```powershell
dotnet ef database update --startup-project ../ChargingControlSystem.Api
```

## Seed-Daten

Seed-Daten werden automatisch beim Erstellen der Datenbank eingefügt (siehe `ApplicationDbContext.cs`):

- **Tenants**: ChargingControl GmbH, Acme GmbH
- **Users**: Admin-User für beide Tenants
- **User Groups**: Admin-Gruppen
- **Permissions**: Alle Standard-Berechtigungen
- **Vehicles**: Beispiel-Fahrzeuge
- **Billing Accounts**: Beispiel-Konten

## Häufige Probleme

### Problem: "dotnet-ef command not found"

**Lösung:**
```powershell
dotnet tool install --global dotnet-ef
```

### Problem: "Database does not exist"

**Lösung:** Führen Sie `reset` aus, um die Datenbank neu zu erstellen.

### Problem: "Migration already applied"

**Lösung:** Das ist normal. Die Migration wurde bereits ausgeführt.

### Problem: "Cannot drop database because it is in use"

**Lösung:** 
1. Stoppen Sie die API
2. Schließen Sie alle Verbindungen zur Datenbank (SQL Server Management Studio, etc.)
3. Versuchen Sie es erneut

## SQL Server Connection String

Die Connection String wird in `appsettings.json` oder `appsettings.Development.json` konfiguriert:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ChargingControlSystem;Integrated Security=True;TrustServerCertificate=True;"
  }
}
```

## Empfohlener Workflow

1. **Entwicklung:** Verwenden Sie `update` um Migrationen anzuwenden
2. **Nach Schema-Änderungen:** Erstellen Sie eine neue Migration und führen Sie `update` aus
3. **Bei Problemen:** Verwenden Sie `reset` um von vorne zu beginnen (⚠️ Daten gehen verloren!)


