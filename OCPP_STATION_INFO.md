# OCPP Station Information Management

## Übersicht

Dieses Dokument beschreibt, welche Informationen wir automatisch von Ladestationen über OCPP-Nachrichten erhalten und wie wir diese verarbeiten.

## ✅ Aktuell implementiert: BootNotification

### Verarbeitete Felder

Die **BootNotification** wird von der Ladestation nach jedem Neustart gesendet. Wir extrahieren und speichern folgende Informationen:

| Feld | Entity-Feld | Beschreibung | Status |
|------|------------|--------------|--------|
| `chargePointVendor` | `Vendor` | Herstellername (z.B. "ABB", "Schneider Electric") | ✅ Gespeichert |
| `chargePointModel` | `Model` | Modellname (z.B. "Terra AC", "EVlink") | ✅ Gespeichert |
| `chargePointSerialNumber` / `chargeBoxSerialNumber` | `SerialNumber` | Seriennummer der Station | ✅ Gespeichert |
| `firmwareVersion` | `FirmwareVersion` | Aktuelle Firmware-Version | ✅ Gespeichert |
| `iccid` | `Iccid` | SIM-Karten ICCID (für mobile Verbindungen) | ✅ Gespeichert |
| `imsi` | `Imsi` | SIM-Karten IMSI (für mobile Verbindungen) | ✅ Gespeichert |
| `meterType` | `MeterType` | Typ des Energiezählers | ✅ Gespeichert |
| `meterSerialNumber` | `MeterSerialNumber` | Seriennummer des Energiezählers | ✅ Gespeichert |

### Automatische Aktualisierung

- **Wann**: Bei jedem Neustart der Ladestation
- **Was wird aktualisiert**: Alle oben genannten Felder werden automatisch aktualisiert
- **Status**: Station wird auf "Available" gesetzt und `LastHeartbeat` aktualisiert

## 🔄 Weitere OCPP-Nachrichten mit nützlichen Informationen

### 1. StatusNotification (✅ Bereits implementiert)

**Zweck**: Informiert über Statusänderungen von Connectors

**Verarbeitete Informationen**:
- Connector-Status (Available, Occupied, Faulted, etc.)
- Fehlercodes
- Automatische Aktualisierung der Station/Connector-Status

**Nutzen**:
- Echtzeit-Statusüberwachung
- Fehlererkennung
- Verfügbarkeitsanzeige im Frontend

### 2. Heartbeat (✅ Bereits implementiert)

**Zweck**: Regelmäßige Lebenszeichen der Station

**Verarbeitete Informationen**:
- `LastHeartbeat` wird aktualisiert
- Station wird als "online" markiert wenn Heartbeat < 10 Minuten alt

**Nutzen**:
- Online/Offline-Erkennung
- Verbindungsüberwachung

### 3. FirmwareStatusNotification (✅ Bereits implementiert, aber nur geloggt)

**Zweck**: Informiert über Firmware-Update-Status

**Aktueller Status**: Wird nur geloggt, nicht gespeichert

**Mögliche Erweiterungen**:
- Firmware-Update-Status in Entity speichern
- Update-Historie verwalten
- Benachrichtigungen bei fehlgeschlagenen Updates

### 4. MeterValues (✅ Bereits implementiert)

**Zweck**: Liefert Energieverbrauchsdaten während des Ladevorgangs

**Verarbeitete Informationen**:
- Energieverbrauch (kWh)
- Leistung (kW)
- Spannung, Strom
- Zeitstempel

**Nutzen**:
- Ladevorgang-Tracking
- Abrechnung
- Energieverbrauchsanalyse

### 5. GetConfiguration (❌ Nicht implementiert)

**Zweck**: Abruf von Konfigurationsparametern der Station

**Mögliche Informationen**:
- Heartbeat-Intervall
- Meter-Werte-Intervall
- Unterstützte Features
- Lokale Autorisierung aktiviert?
- Reservierungszeitlimit
- Maximale Ladeleistung pro Connector
- Zahlungsmethoden

**Nutzen**:
- Automatische Konfigurationsprüfung
- Feature-Erkennung
- Konfigurationsvalidierung

**Implementierungsvorschlag**:
```csharp
// In ChargingStation Entity hinzufügen:
public string? ConfigurationJson { get; set; } // JSON mit Konfigurationsparametern
public DateTime? LastConfigurationUpdate { get; set; }
```

### 6. GetDiagnostics (❌ Nicht implementiert)

**Zweck**: Abruf von Diagnoseinformationen

**Mögliche Informationen**:
- Log-Dateien
- System-Status
- Hardware-Informationen
- Netzwerk-Status
- Fehlerprotokolle

**Nutzen**:
- Fehlerdiagnose
- Wartungsplanung
- Systemüberwachung

**Implementierungsvorschlag**:
```csharp
// Neue Entity: ChargingStationDiagnostics
public class ChargingStationDiagnostics
{
    public Guid Id { get; set; }
    public Guid ChargingStationId { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? DiagnosticsUrl { get; set; }
    public string? Status { get; set; }
}
```

### 7. ChangeConfiguration (❌ Nicht implementiert)

**Zweck**: Änderung von Konfigurationsparametern

**Mögliche Anwendungen**:
- Heartbeat-Intervall anpassen
- Meter-Werte-Intervall ändern
- Features aktivieren/deaktivieren

**Nutzen**:
- Zentrale Konfigurationsverwaltung
- Automatische Optimierung
- Feature-Aktivierung

### 8. GetLocalListVersion (❌ Nicht implementiert)

**Zweck**: Abruf der Version der lokalen Autorisierungsliste

**Nutzen**:
- Synchronisation der Autorisierungslisten
- Offline-Autorisierung verwalten

### 9. SendLocalList (❌ Nicht implementiert)

**Zweck**: Aktualisierung der lokalen Autorisierungsliste

**Nutzen**:
- Offline-Autorisierung ermöglichen
- Schnellere Autorisierung ohne Server-Kommunikation

## 📊 Zusammenfassung: Automatisch verfügbare Informationen

### ✅ Bereits implementiert und gespeichert:

1. **BootNotification**:
   - Hersteller, Modell, Seriennummer
   - Firmware-Version
   - SIM-Karten-Informationen (ICCID, IMSI)
   - Zähler-Informationen

2. **StatusNotification**:
   - Connector-Status
   - Fehlercodes

3. **Heartbeat**:
   - Online/Offline-Status

4. **MeterValues**:
   - Energieverbrauch
   - Leistungsdaten

### ✅ Neu implementiert:

1. **FirmwareStatusNotification** (erweitert):
   - ✅ Status wird in `ChargingStation.FirmwareStatus` gespeichert
   - ✅ Historie wird in `ChargingStationFirmwareHistory` gespeichert
   - ✅ Benachrichtigungen bei fehlgeschlagenen Updates
   - ✅ Benachrichtigungen bei erfolgreichen Updates

2. **GetConfiguration**:
   - ✅ Konfigurationsparameter abrufen
   - ✅ Speicherung in `ChargingStation.ConfigurationJson`
   - ✅ Filterung nach spezifischen Keys möglich
   - ✅ Feature-Erkennung durch Konfigurationsanalyse

3. **ChangeConfiguration**:
   - ✅ Konfiguration ändern
   - ✅ Validierung von readonly-Keys
   - ✅ Automatische Speicherung

4. **GetDiagnostics**:
   - ✅ Diagnoseinformationen anfordern
   - ✅ Historie in `ChargingStationDiagnostics`
   - ✅ Zeitbereichs-Filterung (StartTime/StopTime)
   - ✅ Status-Tracking (Pending/Completed/Failed)

### ❌ Noch nicht implementiert:

1. **GetLocalListVersion**: Lokale Autorisierungsliste verwalten
2. **SendLocalList**: Lokale Autorisierungsliste aktualisieren

## 🎯 Implementierte Features

### ✅ FirmwareStatusNotification (erweitert)
- Status wird in `ChargingStation.FirmwareStatus` gespeichert
- Historie wird in `ChargingStationFirmwareHistory` gespeichert
- Benachrichtigungen bei fehlgeschlagenen Updates
- Benachrichtigungen bei erfolgreichen Updates

### ✅ GetConfiguration
- Konfigurationsparameter abrufen und speichern
- Filterung nach spezifischen Keys
- Feature-Erkennung durch Konfigurationsanalyse
- Automatische Validierung

### ✅ ChangeConfiguration
- Konfiguration ändern
- Validierung von readonly-Keys
- Automatische Speicherung

### ✅ GetDiagnostics
- Diagnoseinformationen anfordern
- Historie in `ChargingStationDiagnostics`
- Zeitbereichs-Filterung
- Status-Tracking

## 🔮 Zukünftige Erweiterungen

### Priorität 1: GetLocalListVersion & SendLocalList
- Lokale Autorisierungsliste verwalten
- Offline-Autorisierung ermöglichen
- Schnellere Autorisierung ohne Server-Kommunikation

### Priorität 2: RemoteTrigger
- Remote-Start/Stop von Ladevorgängen
- Remote-Reset der Station
- Remote-Unlock von Connectors

## 💡 Best Practices

1. **Automatische Aktualisierung**: BootNotification sollte immer alle verfügbaren Felder aktualisieren
2. **Fehlerbehandlung**: Unbekannte Stationen sollten geloggt werden
3. **Validierung**: Eingesendete Daten sollten validiert werden
4. **Historie**: Wichtige Änderungen (z.B. Firmware-Updates) sollten protokolliert werden
5. **Performance**: Regelmäßige Abfragen (z.B. GetConfiguration) sollten nicht zu häufig erfolgen

## 📝 Code-Beispiele

### BootNotification-Verarbeitung (aktuell)

```csharp
// In OcppMessageHandler.cs
station.Vendor = request.ChargePointVendor;
station.Model = request.ChargePointModel;
station.SerialNumber = request.ChargePointSerialNumber ?? request.ChargeBoxSerialNumber;
station.FirmwareVersion = request.FirmwareVersion;
station.Iccid = request.Iccid;
station.Imsi = request.Imsi;
station.MeterType = request.MeterType;
station.MeterSerialNumber = request.MeterSerialNumber;
```

### GetConfiguration-Verarbeitung (Vorschlag)

```csharp
private async Task<GetConfigurationResponse> HandleGetConfigurationAsync(
    string chargeBoxId, 
    GetConfigurationRequest request)
{
    // Station finden
    var station = await context.ChargingStations
        .FirstOrDefaultAsync(s => s.ChargeBoxId == chargeBoxId);
    
    if (station == null)
        throw new Exception("Station not found");
    
    // Konfiguration abrufen (müsste an Station gesendet werden)
    // Response würde ConfigurationKeys enthalten
    
    // Konfiguration speichern
    station.ConfigurationJson = JsonConvert.SerializeObject(configurationKeys);
    station.LastConfigurationUpdate = DateTime.UtcNow;
    
    await context.SaveChangesAsync();
    
    return new GetConfigurationResponse { ConfigurationKeys = ... };
}
```

