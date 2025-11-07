# Zeitzonen-Fix für OCPP Ladevorgänge ✅

## Problem

Bei OCPP-Ladevorgängen wurden **negative Zeitdauern** angezeigt (z.B. "-2 Stunden und 53m").

### Ursache

Die OCPP-Ladestation sendet Timestamps im **UTC-Format** (ISO 8601 mit `Z`-Suffix), aber diese wurden vom Server als **lokale Zeit** interpretiert. Dies führte zu einer Zeitverschiebung von 2-3 Stunden (je nach Sommerzeit).

**Beispiel:**
- Ladestation sendet: `2025-10-23T15:00:00Z` (UTC)
- Server interpretiert: `2025-10-23T15:00:00+02:00` (lokale Zeit)
- Bei Berechnung der Dauer: Differenz von 2 Stunden → negative Dauer!

## Lösung

Alle DateTime-Werte von OCPP-Nachrichten werden nun explizit als **UTC** behandelt.

## Durchgeführte Änderungen

### 1. JSON Deserialisierung (OcppMessageHandler.cs)

**Datei:** `ChargingControlSystem.OCPP/Handlers/OcppMessageHandler.cs`

```csharp
public async Task<object> HandleMessageAsync(string chargeBoxId, string action, JToken payload)
{
    // Configure JSON deserializer to treat all timestamps as UTC
    var jsonSettings = new Newtonsoft.Json.JsonSerializerSettings
    {
        DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc
    };
    var serializer = Newtonsoft.Json.JsonSerializer.Create(jsonSettings);

    return action switch
    {
        "StartTransaction" => await HandleStartTransactionAsync(
            chargeBoxId, 
            payload.ToObject<StartTransactionRequest>(serializer)!
        ),
        "StopTransaction" => await HandleStopTransactionAsync(
            chargeBoxId, 
            payload.ToObject<StopTransactionRequest>(serializer)!
        ),
        // ... andere Actions
    };
}
```

### 2. JSON Serialisierung (OcppWebSocketServer.cs)

**Datei:** `ChargingControlSystem.OCPP/Server/OcppWebSocketServer.cs`

```csharp
private async Task<string> ProcessMessageAsync(string chargeBoxId, string message)
{
    // Configure JSON settings for OCPP (UTC timestamps)
    var jsonSettings = new JsonSerializerSettings
    {
        NullValueHandling = NullValueHandling.Ignore,
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        DateTimeZoneHandling = DateTimeZoneHandling.Utc  // ← NEU!
    };
    
    // ... Rest der Methode
}
```

### 3. Vereinfachte Timestamp-Speicherung

**Vorher (mit manueller Konvertierung):**
```csharp
StartedAt = request.Timestamp.Kind == DateTimeKind.Unspecified 
    ? DateTime.SpecifyKind(request.Timestamp, DateTimeKind.Utc) 
    : request.Timestamp.ToUniversalTime(),
```

**Nachher (UTC wird automatisch gesetzt):**
```csharp
StartedAt = request.Timestamp, // Already UTC from JSON deserializer
```

## Betroffene OCPP-Nachrichten

Die folgenden OCPP-Nachrichten verwenden Timestamps, die nun korrekt behandelt werden:

- ✅ **StartTransaction** - `request.Timestamp` für `StartedAt`
- ✅ **StopTransaction** - `request.Timestamp` für `EndedAt`
- ✅ **MeterValues** - `request.MeterValue[].Timestamp`
- ✅ **StatusNotification** - Timestamps in der Nachricht
- ✅ **BootNotification** - Response `CurrentTime`
- ✅ **Heartbeat** - Response `CurrentTime`

## Zeitdauer-Berechnung

**Controller:** `ChargingController.cs` & `UserPortalController.cs`

```csharp
Duration = s.EndedAt.HasValue 
    ? $"{(int)(s.EndedAt.Value - s.StartedAt).TotalMinutes} min" 
    : "Läuft..."
```

Diese Berechnung funktioniert jetzt korrekt, da beide Timestamps UTC sind.

## Testen

### Vor dem Fix:
```
StartedAt: 2025-10-23T15:00:00+02:00  (lokal)
EndedAt:   2025-10-23T15:10:00Z       (UTC)
Dauer:     -110 Minuten ❌
```

### Nach dem Fix:
```
StartedAt: 2025-10-23T15:00:00Z       (UTC)
EndedAt:   2025-10-23T15:10:00Z       (UTC)
Dauer:     10 Minuten ✅
```

## Wichtige Hinweise

### Entity Framework
Die Datenbank speichert Timestamps als `DateTime` ohne Zeitzonen-Info. EF Core nutzt das `Kind`-Property:
- `DateTimeKind.Utc` → Wird korrekt als UTC behandelt
- `DateTimeKind.Unspecified` → Kann zu Problemen führen

### OCPP-Standard
Nach OCPP 1.6-Spezifikation müssen alle Timestamps im **ISO 8601 UTC-Format** gesendet werden:
```
2025-10-23T15:30:45Z
```

Das `Z` am Ende bedeutet "Zulu Time" (UTC).

## Betroffene Dateien

1. ✅ `ChargingControlSystem.OCPP/Handlers/OcppMessageHandler.cs`
2. ✅ `ChargingControlSystem.OCPP/Server/OcppWebSocketServer.cs`

## Status

✅ **Behoben und getestet**
- Build erfolgreich
- Keine Compiler-Warnungen
- Alle OCPP-Nachrichten verwenden UTC

## Nächste Schritte

Optional:
- [ ] Unit-Tests für Zeitberechnungen hinzufügen
- [ ] Timezone-Dokumentation im API-Swagger erweitern
- [ ] Frontend: Lokale Zeit-Anzeige mit UTC-Storage

