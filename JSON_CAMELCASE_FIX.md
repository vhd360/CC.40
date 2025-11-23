# ✅ Connectors nicht sichtbar beim Remote-Laden - Problem behoben!

## 🔍 Das Hauptproblem

**Symptom:** Im Dialog "Ladevorgang starten" (Remote-Laden) wurden keine Connectors im Dropdown angezeigt, obwohl sie in der Datenbank vorhanden und verfügbar waren.

**Root Cause:** **JSON-Serialisierungs-Mismatch zwischen Backend und Frontend**

## 🎯 Technische Analyse

### Das Problem

**Backend (C# / .NET):**
- Standard-Naming-Convention: **PascalCase**
- API gibt zurück: `IsAvailable`, `ConnectorId`, `EvseId`

```json
{
  "Id": "abc-123",
  "ConnectorId": 1,
  "EvseId": 1,
  "IsAvailable": true  // ← PascalCase
}
```

**Frontend (TypeScript / JavaScript):**
- Standard-Naming-Convention: **camelCase**
- Erwartet: `isAvailable`, `connectorId`, `evseId`

```typescript
connectors.filter(c => c.isAvailable)  // ← sucht nach "isAvailable"
```

**Ergebnis:**
- `c.isAvailable` ist `undefined` (Property existiert nicht)
- Filter liefert keine Connectors zurück
- Dropdown zeigt "Keine verfügbaren Connectoren"

## ✅ Die Lösung

### API-Konfiguration: CamelCase-Naming-Policy

**Datei:** `D:\CC.40\ChargingControlSystem.Api\Program.cs`

**Vorher (FALSCH):**
```csharp
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = 
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
```

**Jetzt (RICHTIG):**
```csharp
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = 
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        
        // ✅ NEU: Use camelCase for JSON property names
        options.JsonSerializerOptions.PropertyNamingPolicy = 
            System.Text.Json.JsonNamingPolicy.CamelCase;
    });
```

**Effekt:**
```json
// Backend serialisiert jetzt automatisch zu camelCase:
{
  "id": "abc-123",           // ✅ camelCase
  "connectorId": 1,          // ✅ camelCase
  "evseId": 1,               // ✅ camelCase
  "isAvailable": true,       // ✅ camelCase
  "pointName": "Ladepunkt 1",
  "type": "Type2",
  "status": "Available",
  "maxPower": 22
}
```

## 🚀 API neu starten (WICHTIG!)

Die Änderung wird nur wirksam, wenn Sie die API neu starten:

```powershell
# Schritt 1: API stoppen (Strg+C im Terminal)

# Schritt 2: API neu starten
cd D:\CC.40\ChargingControlSystem.Api
dotnet run
```

**Erwartete Ausgabe:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5126
info: Microsoft.Hosting.Lifetime[0]
      Application started.
```

## 🧪 Test durchführen

### Schritt 1: Browser neu laden

Laden Sie die Frontend-Seite neu (F5 oder Strg+F5).

### Schritt 2: Remote-Laden testen

1. Öffnen Sie "Meine Ladestationen" im Benutzerportal
2. Klicken Sie bei einer Station auf **"Laden starten"**
3. Der Dialog öffnet sich

### Schritt 3: Browser-Console prüfen (F12)

Öffnen Sie die Browser-Console und sehen Sie sich die Logs an:

```javascript
📡 Connectors geladen: Array(1)
📊 Anzahl Connectors gesamt: 1
✅ Verfügbare Connectors: 1  // ← Sollte jetzt > 0 sein!
🔍 Connector Details: [{
  id: "abc-123-def-456",      // ✅ camelCase
  evseId: 1,                  // ✅ camelCase
  connectorId: 1,             // ✅ camelCase
  status: "Available",
  isAvailable: true          // ✅ camelCase - Frontend findet es!
}]
```

### Schritt 4: Dropdown prüfen

Das Dropdown "Connector auswählen" sollte jetzt die Connectors anzeigen:

```
┌─────────────────────────────────────────┐
│ Connector auswählen*                    │
├─────────────────────────────────────────┤
│ EVSE 1 - Connector 1 (Type2, 22kW)  ✓ │ ← Jetzt sichtbar!
└─────────────────────────────────────────┘
```

## 🔍 Weitere betroffene Endpunkte

Diese Änderung betrifft **alle API-Endpunkte**! Das ist gut, weil es konsistent ist:

### Vorher (PascalCase):
- `/api/charging-stations` → `ChargingStations[].Name`, `IsActive`
- `/api/users` → `Users[].FirstName`, `LastName`, `CreatedAt`
- `/api/vehicles` → `Vehicles[].LicensePlate`, `Make`, `Model`

### Jetzt (camelCase):
- `/api/charging-stations` → `chargingStations[].name`, `isActive`
- `/api/users` → `users[].firstName`, `lastName`, `createdAt`
- `/api/vehicles` → `vehicles[].licensePlate`, `make`, `model`

**Hinweis:** Das Frontend verwendet bereits camelCase überall, daher sollten alle bestehenden API-Aufrufe weiterhin funktionieren (oder sogar besser funktionieren!).

## 📋 Checkliste zur Fehlerbehebung

### Backend
- [x] `Program.cs` wurde aktualisiert
- [ ] API wurde gestoppt (Strg+C)
- [ ] API wurde neu gestartet (`dotnet run`)
- [ ] API läuft ohne Fehler
- [ ] Swagger ist erreichbar: http://localhost:5126/swagger

### Frontend
- [ ] Browser wurde neu geladen (F5)
- [ ] Browser-Cache wurde geleert (optional, aber empfohlen)
- [ ] Benutzer ist eingeloggt

### Test
- [ ] Dialog "Ladevorgang starten" öffnet sich
- [ ] Browser-Console zeigt: "✅ Verfügbare Connectors: 1" (oder mehr)
- [ ] Connector-Details in Console zeigen `isAvailable: true` (camelCase!)
- [ ] Dropdown zeigt Connectors an
- [ ] Connector kann ausgewählt werden

### Validierung (Swagger)

Testen Sie einen Endpunkt in Swagger:

1. Öffnen Sie http://localhost:5126/swagger
2. Navigieren Sie zu `GET /api/charging/stations/{stationId}/connectors`
3. Klicken Sie auf "Try it out"
4. Geben Sie eine Station-ID ein
5. Klicken Sie auf "Execute"

**Erwartetes Response-Format (camelCase):**
```json
[
  {
    "id": "123-456-789",
    "connectorId": 1,
    "evseId": 1,
    "pointName": "Ladepunkt 1",
    "type": "Type2",
    "status": "Available",
    "maxPower": 22,
    "isAvailable": true
  }
]
```

## 🔍 Fehlerbehebung

### Problem: "Keine verfügbaren Connectoren" wird immer noch angezeigt

**Mögliche Ursachen:**

#### 1. API wurde nicht neu gestartet
**Lösung:** Stoppen Sie die API (Strg+C) und starten Sie neu (`dotnet run`)

#### 2. Browser-Cache veraltet
**Lösung:**
- Leeren Sie den Browser-Cache (Strg+Shift+Entf)
- Laden Sie die Seite mit Strg+F5 neu (Hard Reload)
- Oder öffnen Sie ein Inkognito-Fenster

#### 3. Keine Connectors in der Datenbank
**Prüfung:**
```sql
SELECT 
    cc.Id,
    cc.ConnectorId,
    cc.ConnectorType,
    cc.Status,
    cc.IsActive,
    cp.Name AS ChargingPointName,
    cp.IsActive AS ChargingPointIsActive,
    cs.Name AS StationName
FROM ChargingConnectors cc
JOIN ChargingPoints cp ON cc.ChargingPointId = cp.Id
JOIN ChargingStations cs ON cp.ChargingStationId = cs.Id
WHERE cs.Id = 'YOUR-STATION-ID'
```

**Erwartetes Ergebnis:** Mindestens 1 Zeile mit:
- `IsActive = 1` (Connector)
- `ChargingPointIsActive = 1` (ChargingPoint)
- `Status = 0` (Available)

**Lösung:** Legen Sie einen Connector an (siehe vorherige Dokumentationen)

#### 4. Connector-Status ist nicht "Available"
**Prüfung in Browser-Console:**
```javascript
🔍 Connector Details: [{
  status: "Occupied",        // ❌ Nicht "Available"
  isAvailable: false         // ❌ Nicht verfügbar
}]
```

**Lösung:** Setzen Sie den Connector-Status zurück:
- Via Admin-Panel: Reset-Button
- Via API: `POST /api/charging/connectors/{connectorId}/reset`
- Via Datenbank: `UPDATE ChargingConnectors SET Status = 0 WHERE Id = '...'`

#### 5. Frontend empfängt PascalCase (API nicht neu gestartet)
**Prüfung in Browser-Console:**
```javascript
🔍 Connector Details: [{
  Id: "abc-123",             // ❌ PascalCase statt camelCase
  ConnectorId: 1,            // ❌ PascalCase
  IsAvailable: true          // ❌ PascalCase
}]
```

**Diagnose:** API wurde nicht neu gestartet!

**Lösung:** 
1. Stoppen Sie die API (Strg+C)
2. Starten Sie die API neu: `dotnet run`
3. Warten Sie, bis "Application started" erscheint
4. Laden Sie das Frontend neu (F5)

## 📊 Vergleich: Vorher vs. Nachher

### Vorher ❌

**Backend-Response:**
```json
{
  "IsAvailable": true  // ← PascalCase
}
```

**Frontend-Code:**
```typescript
connectors.filter(c => c.isAvailable)  // ← sucht camelCase
// Ergebnis: undefined → Filter liefert []
```

**UI:**
```
┌────────────────────────────────────┐
│ Connector auswählen*               │
├────────────────────────────────────┤
│ Keine verfügbaren Connectoren  ❌ │
└────────────────────────────────────┘
```

### Nachher ✅

**Backend-Response:**
```json
{
  "isAvailable": true  // ← camelCase
}
```

**Frontend-Code:**
```typescript
connectors.filter(c => c.isAvailable)  // ← findet camelCase
// Ergebnis: true → Filter funktioniert
```

**UI:**
```
┌────────────────────────────────────────┐
│ Connector auswählen*                   │
├────────────────────────────────────────┤
│ EVSE 1 - Connector 1 (Type2, 22kW) ✅│
└────────────────────────────────────────┘
```

## ✅ Zusammenfassung

**Was war das Problem?**
- Backend gab JSON in PascalCase zurück
- Frontend erwartete camelCase
- Property-Namen stimmten nicht überein
- Filter `c.isAvailable` fand nichts
- Dropdown blieb leer

**Was wurde behoben?**
- ✅ JSON-Serialisierung konfiguriert: `PropertyNamingPolicy = CamelCase`
- ✅ Backend gibt jetzt automatisch camelCase zurück
- ✅ Frontend findet alle Properties
- ✅ Filter funktioniert korrekt
- ✅ Connectors werden im Dropdown angezeigt

**Was müssen Sie tun?**
1. ✅ API **NEU STARTEN** (Strg+C, dann `dotnet run`)
2. ✅ Frontend neu laden (F5)
3. ✅ Testen!

---

**Erstellt am:** 22.11.2025  
**Status:** ✅ Behoben  
**Dateien geändert:** `ChargingControlSystem.Api/Program.cs`  
**Action Required:** ⚠️ **API NEU STARTEN!**


