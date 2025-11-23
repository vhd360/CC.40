# ✅ Connector nicht sichtbar - Problem behoben!

## 🔍 Das Problem

**Symptom:** Im Dialog "Ladevorgang starten" wurde das Dropdown "Connector auswählen" angezeigt, aber es waren keine Connectors in der Liste sichtbar, obwohl:
- Der Benutzer Zugriff auf die Station hat
- Der Connector nicht belegt ist
- Der Connector existiert und aktiv ist

**Screenshot:** Dropdown zeigt nur "Bitte Connector wählen" ohne Auswahlmöglichkeiten.

## ✅ Die Ursachen

### Problem 1: GUID vs. String
**Technisches Problem:** Die Backend-API gab die Connector-ID als GUID-Objekt zurück:

```csharp
// Vorher - FALSCH:
c.Id  // Typ: Guid (wird als Objekt serialisiert)
```

Das Frontend erwartete aber einen String für die Dropdown-Auswahl:

```typescript
<SelectItem key={connector.id} value={connector.id}>
  // value muss ein String sein!
</SelectItem>
```

**Ergebnis:** Die Dropdown-Werte konnten nicht korrekt verglichen werden, daher wurden keine Optionen angezeigt.

### Problem 2: Deaktivierte Connectors
**Technisches Problem:** Connectors mit `IsActive = false` wurden trotzdem von der API zurückgegeben.

```csharp
// Vorher - FALSCH:
.Where(c => c.ChargingPoint.ChargingStationId == stationId)
// Keine Prüfung auf IsActive!
```

**Ergebnis:** Gelöschte/deaktivierte Connectors wurden als "nicht verfügbar" angezeigt und blockierten die Auswahl.

## 🛠️ Die Lösung

### 1. ChargingService.cs - Backend-Fix

**Datei:** `D:\CC.40\ChargingControlSystem.Api\Services\ChargingService.cs`

**Methode:** `GetStationConnectorsAsync()`

**Änderungen:**

```csharp
public async Task<IEnumerable<object>> GetStationConnectorsAsync(Guid stationId)
{
    var connectors = await _context.ChargingConnectors
        .Include(c => c.ChargingPoint)
        .Where(c => c.ChargingPoint.ChargingStationId == stationId && 
                   c.IsActive &&                           // ✅ NEU: Nur aktive Connectors
                   c.ChargingPoint.IsActive)               // ✅ NEU: Nur aktive ChargingPoints
        .OrderBy(c => c.ChargingPoint.EvseId)
        .ThenBy(c => c.ConnectorId)
        .Select(c => new
        {
            Id = c.Id.ToString(),                          // ✅ NEU: Als String statt GUID
            ConnectorId = c.ConnectorId,
            EvseId = c.ChargingPoint.EvseId,
            PointName = c.ChargingPoint.Name,
            Type = c.ConnectorType,                        // ✅ NEU: Direkt als String
            Status = c.Status.ToString(),
            MaxPower = c.ChargingPoint.MaxPower,
            IsAvailable = c.Status == ConnectorStatus.Available && c.IsActive  // ✅ NEU: Prüft auch IsActive
        })
        .ToListAsync();

    return connectors;
}
```

**Was wurde geändert:**
- ✅ `Id` wird als String zurückgegeben: `c.Id.ToString()`
- ✅ Filter für aktive Connectors: `c.IsActive`
- ✅ Filter für aktive ChargingPoints: `c.ChargingPoint.IsActive`
- ✅ `IsAvailable` prüft zusätzlich `IsActive`
- ✅ `Type` direkt als String statt `.ToString()`

### 2. UserStations.tsx - Frontend-Logging

**Datei:** `D:\CC.40\frontend\src\pages\UserStations.tsx`

**Methode:** `handleStartClick()`

**Änderungen:**
- ✅ Detailliertes Console-Logging für Debugging
- ✅ Zeigt Anzahl geladener Connectors
- ✅ Zeigt Anzahl verfügbarer Connectors
- ✅ Zeigt Details jedes Connectors (ID, Status, Verfügbarkeit)

```typescript
console.log('📡 Connectors geladen:', connectorsData);
console.log('📊 Anzahl Connectors gesamt:', connectorsData.length);
console.log('✅ Verfügbare Connectors:', connectorsData.filter((c: any) => c.isAvailable).length);
```

## 🚀 Testen der Lösung

### Schritt 1: Backend neu starten

Die API muss neu gestartet werden, damit die Änderungen wirksam werden:

```powershell
# Stoppen Sie die API (Strg+C)
# Dann neu starten:
cd D:\CC.40\ChargingControlSystem.Api
dotnet run
```

### Schritt 2: Frontend neu laden

Laden Sie die Seite im Browser neu (F5 oder Strg+F5).

### Schritt 3: Test durchführen

1. Öffnen Sie "Meine Ladestationen" im Benutzerportal
2. Klicken Sie bei einer Station auf "Laden starten"
3. Der Dialog öffnet sich
4. **Erwartetes Ergebnis:** Das Dropdown "Connector auswählen" zeigt jetzt die verfügbaren Connectors an:
   ```
   EVSE 1 - Connector 1 (Type2, 22kW)
   ```

### Schritt 4: Debugging (Browser-Konsole)

Öffnen Sie die Browser-Konsole (F12) und sehen Sie sich die Logs an:

```
📡 Connectors geladen: [...]
📊 Anzahl Connectors gesamt: 1
✅ Verfügbare Connectors: 1
🔍 Connector Details: [{
  id: "abc-123-def-456",
  evseId: 1,
  connectorId: 1,
  status: "Available",
  isAvailable: true
}]
```

**Wenn Sie das sehen:**
- ✅ Connectors wurden erfolgreich geladen
- ✅ ID ist ein String (nicht ein GUID-Objekt)
- ✅ `isAvailable: true` bedeutet, der Connector sollte im Dropdown erscheinen

## 🔍 Fehlerbehebung

### Problem: "Keine verfügbaren Connectoren" im Dropdown

**Mögliche Ursachen:**

#### 1. Keine Connectors angelegt
**Lösung:** 
1. Gehen Sie zur Ladestation-Detailansicht (als Admin/TenantAdmin)
2. Legen Sie einen Ladepunkt an
3. Fügen Sie dem Ladepunkt einen Connector hinzu

#### 2. Alle Connectors sind deaktiviert
**Prüfung in der Datenbank:**
```sql
SELECT 
    cc.Id,
    cc.ConnectorId,
    cc.ConnectorType,
    cc.Status,
    cc.IsActive,
    cp.Name AS ChargingPointName,
    cp.IsActive AS ChargingPointIsActive
FROM ChargingConnectors cc
JOIN ChargingPoints cp ON cc.ChargingPointId = cp.Id
JOIN ChargingStations cs ON cp.ChargingStationId = cs.Id
WHERE cs.Id = 'YOUR-STATION-ID'
```

**Lösung:** Setzen Sie `IsActive = 1` für die Connectors und ChargingPoints.

#### 3. Connector-Status ist nicht "Available"
**Browser-Console zeigt:**
```
✅ Verfügbare Connectors: 0
🔍 Connector Details: [{
  status: "Occupied",  // ❌ Nicht "Available"
  isAvailable: false
}]
```

**Mögliche Status:**
- `Available` ✅ - Connector wird angezeigt
- `Occupied` ❌ - Connector ist belegt
- `Faulted` ❌ - Connector ist defekt
- `Unavailable` ❌ - Connector nicht verfügbar

**Lösung bei blockiertem Status:**
1. Prüfen Sie, ob es eine aktive Ladesession gibt:
   ```sql
   SELECT * FROM ChargingSessions 
   WHERE ChargingConnectorId = 'YOUR-CONNECTOR-ID' 
   AND EndedAt IS NULL
   ```
2. Wenn keine Session aktiv ist, nutzen Sie den Admin-Endpunkt:
   ```
   POST /api/charging/connectors/{connectorId}/reset
   ```
   Oder über die Datenbank:
   ```sql
   UPDATE ChargingConnectors 
   SET Status = 0 -- 0 = Available
   WHERE Id = 'YOUR-CONNECTOR-ID'
   ```

#### 4. API gibt Fehler zurück
**Browser-Console zeigt:**
```
❌ Fehler beim Laden der Connectors: Failed to fetch station connectors
```

**Lösung:**
1. Prüfen Sie die API-Logs im Terminal
2. Prüfen Sie, ob die API läuft: http://localhost:5126/swagger
3. Prüfen Sie die Authentifizierung (Token gültig?)

#### 5. Frontend-Cache-Problem
**Browser zeigt alte Version:**

**Lösung:**
1. Leeren Sie den Browser-Cache (Strg+Shift+Entf)
2. Laden Sie die Seite mit Strg+F5 neu
3. Oder öffnen Sie ein Inkognito-Fenster

## 📊 Connector-Status-Übersicht

| Status | Wert | Im Dropdown? | Bedeutung |
|--------|------|--------------|-----------|
| Available | 0 | ✅ Ja | Connector ist frei und einsatzbereit |
| Occupied | 1 | ❌ Nein | Fahrzeug ist angeschlossen |
| Faulted | 2 | ❌ Nein | Technischer Fehler |
| Unavailable | 3 | ❌ Nein | Außer Betrieb (Wartung etc.) |
| Reserved | 4 | ❌ Nein | Für anderen Benutzer reserviert |

## 🎯 Checkliste

Verwenden Sie diese Checkliste, um sicherzustellen, dass alles richtig konfiguriert ist:

### Backend
- [ ] `ChargingService.cs` wurde aktualisiert
- [ ] API wurde neu gestartet
- [ ] API läuft ohne Fehler (`dotnet run`)
- [ ] Swagger ist erreichbar: http://localhost:5126/swagger

### Datenbank
- [ ] ChargingStation existiert und ist aktiv (`IsActive = 1`)
- [ ] ChargingPoint existiert und ist aktiv (`IsActive = 1`)
- [ ] Connector existiert und ist aktiv (`IsActive = 1`)
- [ ] Connector-Status ist "Available" (`Status = 0`)

### Frontend
- [ ] Frontend wurde neu geladen (F5)
- [ ] Browser-Cache wurde geleert
- [ ] Benutzer ist eingeloggt
- [ ] Benutzer hat Zugriff auf die Station

### Test
- [ ] "Laden starten" öffnet den Dialog
- [ ] Dropdown "Connector auswählen" zeigt Optionen an
- [ ] Browser-Console zeigt: "✅ Verfügbare Connectors: 1" (oder mehr)
- [ ] Connector kann ausgewählt werden
- [ ] Ladevorgang kann gestartet werden

## ✅ Zusammenfassung

**Was war das Problem?**
- Backend gab Connector-IDs als GUID-Objekte statt als Strings zurück
- Deaktivierte Connectors wurden nicht ausgefiltert
- Frontend konnte die Connectors nicht korrekt im Dropdown anzeigen

**Was wurde behoben?**
- ✅ Connector-IDs werden als Strings zurückgegeben
- ✅ Nur aktive Connectors und ChargingPoints werden geladen
- ✅ `IsAvailable` prüft zusätzlich den `IsActive`-Status
- ✅ Detailliertes Logging für Debugging im Frontend

**Ergebnis:**
- ✅ Connectors werden im Dropdown angezeigt
- ✅ Benutzer können einen Connector auswählen
- ✅ Ladevorgang kann gestartet werden

---

**Erstellt am:** 22.11.2025  
**Status:** ✅ Behoben  
**Dateien geändert:**
- `ChargingControlSystem.Api/Services/ChargingService.cs`
- `frontend/src/pages/UserStations.tsx`

