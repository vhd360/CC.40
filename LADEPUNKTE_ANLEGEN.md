# ✅ Ladepunkte anlegen - Problem behoben!

## 🔍 Das Problem

Sie konnten keine Ladepunkte im Frontend anlegen, weil:

1. **API-Endpunkte fehlten**: Es gab keine Controller für `/api/charging-points` und `/api/connectors`
2. **UI-Bug**: Der "Ladepunkt hinzufügen"-Button wurde nur angezeigt, wenn bereits Ladepunkte vorhanden waren

## ✅ Die Lösung

### 1. Backend - Neue API-Controller erstellt

**ChargingPointsController.cs** (`D:\CC.40\ChargingControlSystem.Api\Controllers\ChargingPointsController.cs`)
- `GET /api/charging-points/station/{stationId}` - Alle Ladepunkte einer Station
- `GET /api/charging-points/{id}` - Einzelnen Ladepunkt abrufen
- `POST /api/charging-points` - Neuen Ladepunkt erstellen ✨
- `PUT /api/charging-points/{id}` - Ladepunkt aktualisieren
- `DELETE /api/charging-points/{id}` - Ladepunkt löschen (soft delete)

**ConnectorsController.cs** (`D:\CC.40\ChargingControlSystem.Api\Controllers\ConnectorsController.cs`)
- `GET /api/connectors/charging-point/{chargingPointId}` - Alle Stecker eines Ladepunkts
- `GET /api/connectors/{id}` - Einzelnen Stecker abrufen
- `POST /api/connectors` - Neuen Stecker erstellen ✨
- `PUT /api/connectors/{id}` - Stecker aktualisieren
- `DELETE /api/connectors/{id}` - Stecker löschen (soft delete)

**Features der neuen Controller:**
- ✅ Tenant-basierte Sicherheit
- ✅ Validierung von EVSE-IDs (keine Duplikate)
- ✅ Prüfung auf aktive Ladevorgänge vor Löschung
- ✅ Soft Delete (IsActive = false)
- ✅ Swagger-Dokumentation

### 2. Frontend - UI-Verbesserungen

**ChargingStationDetail.tsx**
- ✅ Ladepunkte-Karte wird jetzt **immer** angezeigt, auch wenn noch keine vorhanden sind
- ✅ Leere-Ansicht mit "Ersten Ladepunkt anlegen"-Button
- ✅ Konvertierung von camelCase (Frontend) zu PascalCase (Backend) in API-Requests
- ✅ Bessere Fehlerbehandlung mit Server-Fehlermeldungen

## 🚀 So starten Sie die Änderungen

### Schritt 1: API neu starten

Die API muss neu gestartet werden, damit die neuen Controller geladen werden:

```powershell
# Stoppen Sie die laufende API (z.B. mit Ctrl+C im Terminal oder im Visual Studio)
# Dann starten Sie sie neu:
cd D:\CC.40\ChargingControlSystem.Api
dotnet run
```

### Schritt 2: Frontend neu laden

Laden Sie die Frontend-Seite im Browser neu (F5 oder Strg+F5).

### Schritt 3: Ladepunkt anlegen

1. Öffnen Sie eine Ladestation in der Detailansicht
2. Sie sehen nun die Karte "Ladepunkte (EVSE)"
3. Wenn noch keine Ladepunkte vorhanden sind, sehen Sie eine Willkommensmeldung
4. Klicken Sie auf "Ersten Ladepunkt anlegen" oder "Ladepunkt hinzufügen"
5. Füllen Sie das Formular aus:
   - **EVSE-ID**: Interne OCPP ConnectorId (z.B. 1, 2, 3...)
   - **Name**: Sprechender Name (z.B. "Ladepunkt 1", "Linke Säule")
   - **Maximale Leistung**: z.B. 22 kW für AC, 50 kW für DC
   - **Funktionen**: Smart Charging, Remote Start/Stop, Reservierung
6. Klicken Sie auf "Ladepunkt anlegen"

### Schritt 4: Stecker hinzufügen

Nach dem Anlegen eines Ladepunkts können Sie physische Stecker hinzufügen:

1. Klicken Sie auf "Stecker hinzufügen" beim entsprechenden Ladepunkt
2. Füllen Sie das Formular aus:
   - **Connector ID**: Physische Stecker-ID (z.B. 1)
   - **Typ**: Type2, CCS, CHAdeMO, Tesla
   - **Leistung**: Maximale Leistung in kW
   - **Strom/Spannung**: z.B. 32A @ 230V für 22kW AC
3. Klicken Sie auf "Stecker anlegen"

## 📋 Beispiel-Konfiguration

### Typische AC-Ladestation (22 kW):

**Ladepunkt 1:**
- EVSE-ID: 1
- Name: "Ladepunkt 1"
- Maximale Leistung: 22 kW
- Remote Start/Stop: Ja

**Stecker 1:**
- Connector ID: 1
- Typ: Type2
- PowerType: AC_3_PHASE
- MaxPower: 22 kW
- MaxCurrent: 32 A
- MaxVoltage: 230 V

### Typische DC-Schnellladestation (50 kW):

**Ladepunkt 1:**
- EVSE-ID: 1
- Name: "DC CCS"
- Maximale Leistung: 50 kW
- Remote Start/Stop: Ja

**Stecker 1:**
- Connector ID: 1
- Typ: CCS
- PowerType: DC
- MaxPower: 50 kW
- MaxCurrent: 125 A
- MaxVoltage: 400 V

## 🔍 Fehlerbehebung

### Problem: "Failed to save charging point"

**Ursache:** API ist noch nicht neu gestartet oder Backend-Fehler.

**Lösung:**
1. Prüfen Sie die Browser-Konsole (F12) für Details
2. Starten Sie die API neu
3. Prüfen Sie die API-Logs im Terminal

### Problem: "EVSE ID already exists"

**Ursache:** Die EVSE-ID wird bereits von einem anderen Ladepunkt dieser Station verwendet.

**Lösung:**
- Verwenden Sie eine eindeutige EVSE-ID (z.B. 1, 2, 3...)
- Die EVSE-ID entspricht der OCPP ConnectorId

### Problem: Button wird nicht angezeigt

**Ursache:** Alte Frontend-Version im Browser-Cache.

**Lösung:**
- Leeren Sie den Browser-Cache (Strg+Shift+Entf)
- Laden Sie die Seite mit Strg+F5 neu

## 📚 Weiterführende Informationen

### OCPP-Struktur

```
ChargingStation (Ladestation)
  └── ChargingPoint (Ladepunkt/EVSE)
      └── ChargingConnector (physischer Stecker)
```

- **ChargingStation**: Die physische Ladestation (Hardware)
- **ChargingPoint (EVSE)**: Elektrischer Ladepunkt mit eigener OCPP ConnectorId
- **ChargingConnector**: Physischer Stecker (Typ2, CCS, CHAdeMO, etc.)

### EVSE-ID vs. Connector-ID

- **EVSE-ID**: OCPP ConnectorId für die Kommunikation mit der Station (1-basiert)
- **Connector-ID**: Physischer Stecker am Ladepunkt (1-basiert innerhalb des Ladepunkts)
- **Externe EVSE-ID**: ISO 15118 konforme ID für eRoaming (z.B. DE*ABC*E1234*5678)

## ✅ Fertig!

Sie können jetzt:
- ✅ Ladepunkte an Ladestationen anlegen
- ✅ Stecker an Ladepunkten hinzufügen
- ✅ OCPP-Konfiguration pro Ladepunkt verwalten
- ✅ Plug & Charge (ISO 15118) Zertifikate hinterlegen
- ✅ Ladepunkte bearbeiten und löschen

---

**Erstellt am:** 22.11.2025  
**Status:** ✅ Behoben


