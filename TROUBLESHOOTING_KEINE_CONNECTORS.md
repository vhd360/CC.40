# 🔍 Troubleshooting: Keine Connectors werden angezeigt

## Problem

Im Dialog "Ladevorgang starten" werden keine Connectors im Dropdown angezeigt.

## Schritt-für-Schritt Diagnose

### ✅ Schritt 1: API neu gestartet?

**Wichtig:** Die camelCase-Konfiguration wird erst nach einem API-Neustart aktiv!

```powershell
# Im API-Terminal:
# 1. Stoppen Sie die API (Strg+C)
# 2. Neu starten:
cd D:\CC.40\ChargingControlSystem.Api
dotnet run
```

**Warten Sie bis:**
```
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

### ✅ Schritt 2: Browser neu geladen?

- **Hard Reload:** Strg+F5 (Windows) oder Cmd+Shift+R (Mac)
- **Oder:** Browser-Cache leeren (Strg+Shift+Entf)

### ✅ Schritt 3: Browser-Console überprüfen (F12)

1. Öffnen Sie die Browser-Entwicklertools (F12)
2. Wechseln Sie zum Tab "Console"
3. Klicken Sie auf "Laden starten" bei einer Station
4. Suchen Sie nach den Logs:

**Erwartete Logs:**
```javascript
📡 Connectors geladen: Array(1)
📊 Anzahl Connectors gesamt: 1
✅ Verfügbare Connectors: 1     ← Muss > 0 sein!
🔍 Connector Details: [{
  id: "...",                    ← String (nicht GUID-Objekt)
  connectorId: 1,               ← camelCase!
  evseId: 1,                    ← camelCase!
  isAvailable: true             ← camelCase! Frontend findet es!
}]
```

**Wenn `Verfügbare Connectors: 0`** → Gehen Sie zu Schritt 4

**Wenn `Connectors geladen: Array(0)`** → Keine Connectors in DB → Gehen Sie zu Schritt 5

**Wenn Properties PascalCase sind (`IsAvailable` statt `isAvailable`)** → API nicht neu gestartet → Zurück zu Schritt 1

### ✅ Schritt 4: Connector-Status in DB prüfen

Führen Sie das SQL-Diagnose-Skript aus:

```powershell
# Im Terminal:
cd D:\CC.40
# SQL Server Management Studio öffnen oder via sqlcmd:
sqlcmd -S localhost -d ChargingControl -i check_connectors.sql
```

**Oder in SQL Server Management Studio:**

```sql
-- Prüfen Sie den Status aller Connectors
SELECT 
    cs.Name AS StationName,
    cp.EvseId,
    cc.ConnectorId,
    cc.ConnectorType,
    cc.Status,
    cc.IsActive AS ConnectorActive,
    cp.IsActive AS PointActive,
    CASE 
        WHEN cc.Status = 0 AND cc.IsActive = 1 AND cp.IsActive = 1 
        THEN '✅ VERFÜGBAR'
        ELSE '❌ NICHT VERFÜGBAR'
    END AS Verfuegbarkeit
FROM ChargingStations cs
JOIN ChargingPoints cp ON cs.Id = cp.ChargingStationId
JOIN ChargingConnectors cc ON cp.Id = cc.ChargingPointId
WHERE cs.IsActive = 1
ORDER BY cs.Name, cp.EvseId, cc.ConnectorId;
```

**Problem A: Status ist nicht 0 (Available)**

Connector-Status-Werte:
- **0 = Available** ✅ (wird angezeigt)
- **1 = Occupied** ❌ (belegt)
- **2 = Faulted** ❌ (defekt)
- **3 = Unavailable** ❌ (nicht verfügbar)
- **4 = Reserved** ❌ (reserviert)

**Lösung:**
```sql
-- Status auf Available zurücksetzen:
UPDATE ChargingConnectors 
SET Status = 0 
WHERE Id = 'YOUR-CONNECTOR-ID';
```

**Problem B: IsActive = 0**

**Lösung:**
```sql
-- Connector reaktivieren:
UPDATE ChargingConnectors 
SET IsActive = 1 
WHERE Id = 'YOUR-CONNECTOR-ID';

-- ChargingPoint reaktivieren (falls deaktiviert):
UPDATE ChargingPoints 
SET IsActive = 1 
WHERE Id = 'YOUR-CHARGINGPOINT-ID';
```

### ✅ Schritt 5: Keine Connectors in der Datenbank?

**Prüfen:**
```sql
SELECT COUNT(*) as AnzahlConnectors
FROM ChargingConnectors cc
JOIN ChargingPoints cp ON cc.ChargingPointId = cp.Id
JOIN ChargingStations cs ON cp.ChargingStationId = cs.Id
WHERE cs.IsActive = 1
  AND cp.IsActive = 1
  AND cc.IsActive = 1;
```

**Wenn Ergebnis = 0:**

#### Option A: Keine ChargingPoints vorhanden

```sql
-- Prüfen:
SELECT 
    cs.Name,
    COUNT(cp.Id) AS AnzahlChargingPoints
FROM ChargingStations cs
LEFT JOIN ChargingPoints cp ON cs.Id = cp.ChargingStationId
WHERE cs.IsActive = 1
GROUP BY cs.Name
HAVING COUNT(cp.Id) = 0;
```

**Lösung:** ChargingPoints anlegen via Frontend:
1. Öffnen Sie die Ladestation-Detailansicht
2. Klicken Sie "Ladepunkt hinzufügen"
3. Füllen Sie das Formular aus
4. Speichern

#### Option B: Keine Connectors an ChargingPoints

```sql
-- Prüfen:
SELECT 
    cs.Name AS StationName,
    cp.EvseId,
    cp.Name AS PointName,
    COUNT(cc.Id) AS AnzahlConnectors
FROM ChargingStations cs
JOIN ChargingPoints cp ON cs.Id = cp.ChargingStationId
LEFT JOIN ChargingConnectors cc ON cp.Id = cc.ChargingPointId
WHERE cs.IsActive = 1 AND cp.IsActive = 1
GROUP BY cs.Name, cp.EvseId, cp.Name
HAVING COUNT(cc.Id) = 0;
```

**Lösung:** Stecker anlegen via Frontend:
1. Öffnen Sie die Ladestation-Detailansicht
2. Beim ChargingPoint klicken Sie "Stecker hinzufügen"
3. Füllen Sie das Formular aus:
   - Connector ID: 1
   - Typ: Type2, CCS, CHAdeMO, etc.
   - Leistung: z.B. 22 kW
   - Strom: z.B. 32 A
   - Spannung: z.B. 230 V
4. Speichern

### ✅ Schritt 6: API-Endpunkt direkt testen (Swagger)

1. Öffnen Sie: http://localhost:5126/swagger
2. Navigieren Sie zu: `GET /api/charging/stations/{stationId}/connectors`
3. Klicken Sie "Try it out"
4. Geben Sie eine Station-ID ein
5. Klicken Sie "Execute"

**Erwartete Response (mit camelCase!):**
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

**Wenn Response leer `[]` ist:**
- Keine Connectors für diese Station in der DB
- Siehe Schritt 5

**Wenn Properties PascalCase sind:**
- API wurde nicht neu gestartet
- Zurück zu Schritt 1

### ✅ Schritt 7: Netzwerk-Traffic prüfen

In den Browser-DevTools (F12):

1. Wechseln Sie zum Tab "Network" / "Netzwerk"
2. Klicken Sie "Laden starten"
3. Suchen Sie nach dem Request: `/api/charging/stations/.../connectors`
4. Klicken Sie auf den Request
5. Überprüfen Sie:
   - **Status:** Sollte 200 OK sein
   - **Response:** Sollte ein Array mit Connectors sein
   - **Headers:** Content-Type: application/json

**Wenn Status 401 Unauthorized:**
- Token ist abgelaufen
- Loggen Sie sich erneut ein

**Wenn Status 404 Not Found:**
- Station-ID ist ungültig
- Prüfen Sie die Station-ID

**Wenn Status 500 Internal Server Error:**
- Backend-Fehler
- Prüfen Sie die API-Logs im Terminal

## Schnell-Checkliste

Arbeiten Sie diese Liste von oben nach unten durch:

- [ ] API wurde neu gestartet (nach camelCase-Änderung)
- [ ] Browser wurde neu geladen (Strg+F5)
- [ ] Browser-Console zeigt: "✅ Verfügbare Connectors: 1" (oder mehr)
- [ ] Connector-Details in Console zeigen camelCase (`isAvailable`)
- [ ] Datenbank: ChargingPoints existieren
- [ ] Datenbank: Connectors existieren
- [ ] Datenbank: Connector.Status = 0 (Available)
- [ ] Datenbank: Connector.IsActive = 1
- [ ] Datenbank: ChargingPoint.IsActive = 1
- [ ] Swagger-Test erfolgreich (Response mit camelCase)
- [ ] Network-Tab zeigt 200 OK Response

## Häufigste Ursachen

### 1. API nicht neu gestartet (70% der Fälle)
**Symptom:** Console zeigt PascalCase (`IsAvailable`)
**Lösung:** API stoppen (Strg+C) und neu starten

### 2. Keine Connectors in DB (20% der Fälle)
**Symptom:** Console zeigt "Connectors geladen: Array(0)"
**Lösung:** ChargingPoints und Connectors anlegen

### 3. Connector-Status nicht Available (5% der Fälle)
**Symptom:** Console zeigt "Verfügbare Connectors: 0" aber Connectors vorhanden
**Lösung:** Status auf 0 setzen via SQL oder Reset-Button

### 4. Browser-Cache veraltet (5% der Fälle)
**Symptom:** Alte Version wird angezeigt
**Lösung:** Strg+F5 oder Inkognito-Fenster

## Sofort-Hilfe

**Wenn gar nichts funktioniert:**

1. **API komplett neu bauen:**
   ```powershell
   cd D:\CC.40\ChargingControlSystem.Api
   dotnet clean
   dotnet build
   dotnet run
   ```

2. **Frontend neu bauen:**
   ```powershell
   cd D:\CC.40\frontend
   npm run build
   npm run dev
   ```

3. **Test-Connector manuell in DB erstellen:**
   ```sql
   -- 1. Station-ID finden
   SELECT Id, Name FROM ChargingStations WHERE IsActive = 1;
   
   -- 2. ChargingPoint anlegen
   DECLARE @StationId UNIQUEIDENTIFIER = 'YOUR-STATION-ID';
   DECLARE @PointId UNIQUEIDENTIFIER = NEWID();
   
   INSERT INTO ChargingPoints (Id, ChargingStationId, EvseId, Name, MaxPower, Status, IsActive, CreatedAt)
   VALUES (@PointId, @StationId, 1, 'Test Ladepunkt', 22, 0, 1, GETUTCDATE());
   
   -- 3. Connector anlegen
   INSERT INTO ChargingConnectors (Id, ChargingPointId, ConnectorId, ConnectorType, PowerType, MaxPower, MaxCurrent, MaxVoltage, Status, IsActive, CreatedAt)
   VALUES (NEWID(), @PointId, 1, 'Type2', 'AC_3_PHASE', 22, 32, 230, 0, 1, GETUTCDATE());
   ```

## Support-Dateien

Zur Diagnose wurden folgende Dateien erstellt:

- `check_connectors.sql` - Vollständige Datenbank-Diagnose
- `JSON_CAMELCASE_FIX.md` - Erklärung des camelCase-Problems
- `CONNECTOR_NICHT_SICHTBAR_BEHOBEN.md` - Ursprüngliche Problem-Dokumentation

---

**Stand:** 22.11.2025  
**Zuletzt aktualisiert:** Nach camelCase-Fix in Program.cs


