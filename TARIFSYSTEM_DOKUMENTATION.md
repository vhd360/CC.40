# Tarifsystem Dokumentation

## Übersicht

Das flexible Tarifsystem ermöglicht es, verschiedene Preismodelle für Ladevorgänge zu definieren und Benutzergruppen zuzuordnen.

## Features

✅ **Flexible Tarifkomponenten**
- Energiebasiert (€/kWh)
- Zeitbasiert (€/Minute für Laden oder Parken)
- Pauschale Sitzungsgebühr
- Zeittarife (Tag/Nacht)

✅ **Erweiterte Optionen**
- Schrittweise Abrechnung
- Mindest- und Höchstbeträge
- Kulanzzeiten
- Wochentags-spezifische Tarife
- Gültigkeitszeiträume

✅ **Benutzergruppen-Zuordnung**
- Tarife werden Benutzergruppen zugewiesen
- Prioritäten für mehrere Tarife
- Fallback auf Standard-Tarif

## Datenbank-Struktur

### Tariffs
Haupttabelle für Tarife

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| Id | GUID | Eindeutige ID |
| TenantId | GUID | Mandant |
| Name | String | Tarifname |
| Description | String | Beschreibung |
| Currency | String | Währung (ISO 4217) |
| IsDefault | Boolean | Standard-Tarif? |
| IsActive | Boolean | Aktiv? |
| ValidFrom | DateTime? | Gültig ab |
| ValidUntil | DateTime? | Gültig bis |

### TariffComponents
Einzelne Preiskomponenten eines Tarifs

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| Id | GUID | Eindeutige ID |
| TariffId | GUID | Zugehöriger Tarif |
| Type | Enum | Art der Komponente |
| Price | Decimal | Preis pro Einheit |
| StepSize | Int? | Schrittgröße |
| TimeStart | String? | Startzeit (HH:mm) |
| TimeEnd | String? | Endzeit (HH:mm) |
| DaysOfWeek | String? | Wochentage (0-6) |
| MinimumCharge | Decimal? | Mindestbetrag |
| MaximumCharge | Decimal? | Höchstbetrag |
| GracePeriodMinutes | Int? | Kulanzzeit in Minuten |
| DisplayOrder | Int | Anzeigereihenfolge |

### UserGroupTariffs
Zuordnung von Tarifen zu Benutzergruppen

| Feld | Typ | Beschreibung |
|------|-----|--------------|
| Id | GUID | Eindeutige ID |
| UserGroupId | GUID | Benutzergruppe |
| TariffId | GUID | Tarif |
| Priority | Int | Priorität |

## Tarifkomponenten-Typen

### 0 - Energy (Energiebasiert)
Berechnung basierend auf verbrauchten kWh
```json
{
  "Type": 0,
  "Price": 0.35
}
```
Beispiel: 0,35 €/kWh

### 1 - ChargingTime (Ladezeit)
Berechnung basierend auf Lademinuten
```json
{
  "Type": 1,
  "Price": 0.05,
  "GracePeriodMinutes": 120
}
```
Beispiel: 0,05 €/Minute nach 2 Stunden Kulanzzeit

### 2 - ParkingTime (Parkzeit)
Berechnung der gesamten Parkdauer
```json
{
  "Type": 2,
  "Price": 0.02,
  "MaximumCharge": 10.00
}
```
Beispiel: 0,02 €/Minute, maximal 10,00 €

### 3 - SessionFee (Sitzungsgebühr)
Einmalige Pauschale pro Ladevorgang
```json
{
  "Type": 3,
  "Price": 1.50
}
```
Beispiel: 1,50 € pro Ladevorgang

### 4 - IdleTime (Standzeit nach Ladeende)
Berechnung für Zeit nach Ladeende
```json
{
  "Type": 4,
  "Price": 0.10,
  "GracePeriodMinutes": 15
}
```
Beispiel: 0,10 €/Minute nach 15 Minuten Kulanzzeit

### 5 - TimeOfDay (Zeittarif)
Preise abhängig von Tageszeit
```json
{
  "Type": 5,
  "Price": 0.25,
  "TimeStart": "22:00",
  "TimeEnd": "06:00",
  "DaysOfWeek": "0,1,2,3,4,5,6"
}
```
Beispiel: 0,25 €/kWh nachts (22-6 Uhr)

## API-Endpunkte

### GET /api/tariffs
Liste aller Tarife des aktuellen Mandanten

### GET /api/tariffs/{id}
Einzelnen Tarif abrufen

### POST /api/tariffs
Neuen Tarif erstellen

**Request Body:**
```json
{
  "name": "Standard AC-Tarif",
  "description": "Standard-Tarif für AC-Laden",
  "currency": "EUR",
  "isDefault": true,
  "isActive": true,
  "components": [
    {
      "type": 0,
      "price": 0.35,
      "displayOrder": 0
    },
    {
      "type": 3,
      "price": 1.00,
      "displayOrder": 1
    }
  ]
}
```

### PUT /api/tariffs/{id}
Tarif aktualisieren

### DELETE /api/tariffs/{id}
Tarif löschen

### POST /api/tariffs/{tariffId}/usergroups/{userGroupId}
Tarif einer Benutzergruppe zuweisen

**Request Body:**
```json
5
```
(Priorität als Integer)

### DELETE /api/tariffs/{tariffId}/usergroups/{userGroupId}
Tarifzuweisung entfernen

## Beispiel-Tarife

### 1. Einfacher Energietarif
```json
{
  "name": "Basis Energietarif",
  "description": "0,30 € pro kWh",
  "currency": "EUR",
  "isDefault": true,
  "isActive": true,
  "components": [
    {
      "type": 0,
      "price": 0.30,
      "displayOrder": 0
    }
  ]
}
```

### 2. Energie + Sitzungsgebühr
```json
{
  "name": "Standard-Tarif",
  "description": "0,35 €/kWh + 1,50 € Startgebühr",
  "currency": "EUR",
  "isDefault": false,
  "isActive": true,
  "components": [
    {
      "type": 0,
      "price": 0.35,
      "displayOrder": 0
    },
    {
      "type": 3,
      "price": 1.50,
      "displayOrder": 1
    }
  ]
}
```

### 3. Blockiergebühr-Tarif
```json
{
  "name": "Blockiergebühr",
  "description": "0,30 €/kWh + 0,10 €/Min Blockiergebühr nach 3 Stunden",
  "currency": "EUR",
  "isDefault": false,
  "isActive": true,
  "components": [
    {
      "type": 0,
      "price": 0.30,
      "displayOrder": 0
    },
    {
      "type": 1,
      "price": 0.10,
      "gracePeriodMinutes": 180,
      "displayOrder": 1
    }
  ]
}
```

### 4. Tag/Nacht-Tarif
```json
{
  "name": "Tag/Nacht-Tarif",
  "description": "Günstiger nachts laden",
  "currency": "EUR",
  "isDefault": false,
  "isActive": true,
  "components": [
    {
      "type": 5,
      "price": 0.25,
      "timeStart": "22:00",
      "timeEnd": "06:00",
      "displayOrder": 0
    },
    {
      "type": 0,
      "price": 0.40,
      "displayOrder": 1
    }
  ]
}
```

### 5. Geschäftskunden-Tarif
```json
{
  "name": "Geschäftskunden",
  "description": "Spezial-Tarif für Geschäftskunden",
  "currency": "EUR",
  "isDefault": false,
  "isActive": true,
  "validFrom": "2025-01-01T00:00:00Z",
  "validUntil": "2025-12-31T23:59:59Z",
  "components": [
    {
      "type": 0,
      "price": 0.28,
      "displayOrder": 0
    },
    {
      "type": 2,
      "price": 0.01,
      "maximumCharge": 5.00,
      "displayOrder": 1
    }
  ]
}
```

## Preisberechnung

Die Preisberechnung erfolgt automatisch:

1. **OCPP-Handler**: Bei `StopTransaction` und `MeterValues`
2. **TariffService**: Über `CalculateCostAsync()`

### Priorität der Tarifauswahl

1. **Benutzergruppen-Tarif** (höchste Priorität zuerst)
2. **Standard-Tarif** des Mandanten
3. **Fallback**: 0,30 €/kWh

### Kostenaufschlüsselung

Der TariffService liefert eine detaillierte Kostenaufschlüsselung:

```csharp
{
  "TotalCost": 12.50,
  "Currency": "EUR",
  "Breakdown": {
    "Energy (0.3500 EUR)": 10.50,
    "SessionFee (1.5000 EUR)": 1.50,
    "ChargingTime (0.0500 EUR)": 0.50
  },
  "AppliedTariff": { ... }
}
```

## Workflow: Tarif anlegen

1. **Tarif erstellen**
   ```bash
   POST /api/tariffs
   ```

2. **Benutzergruppe erstellen** (falls noch nicht vorhanden)
   ```bash
   POST /api/usergroups
   ```

3. **Tarif der Gruppe zuweisen**
   ```bash
   POST /api/tariffs/{tariffId}/usergroups/{userGroupId}
   ```

4. **Benutzer der Gruppe hinzufügen**
   ```bash
   POST /api/usergroups/{userGroupId}/members/{userId}
   ```

## Testen

### SQL: Standard-Tarif erstellen
```sql
-- Standard-Tarif erstellen
DECLARE @TariffId UNIQUEIDENTIFIER = NEWID();
DECLARE @TenantId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';

INSERT INTO Tariffs (Id, TenantId, Name, Description, Currency, IsDefault, IsActive, CreatedAt)
VALUES (@TariffId, @TenantId, 'Standard-Tarif', '0,30 EUR pro kWh', 'EUR', 1, 1, GETUTCDATE());

-- Energie-Komponente
INSERT INTO TariffComponents (Id, TariffId, Type, Price, DisplayOrder, IsActive)
VALUES (NEWID(), @TariffId, 0, 0.30, 0, 1);

PRINT 'Standard-Tarif erstellt: ' + CAST(@TariffId AS VARCHAR(50));
```

### SQL: Tarif einer Benutzergruppe zuweisen
```sql
DECLARE @TariffId UNIQUEIDENTIFIER = '...'; -- Ihre Tarif-ID
DECLARE @UserGroupId UNIQUEIDENTIFIER = '33333333-3333-3333-3333-333333333333'; -- Admin-Gruppe

INSERT INTO UserGroupTariffs (Id, UserGroupId, TariffId, Priority, CreatedAt)
VALUES (NEWID(), @UserGroupId, @TariffId, 10, GETUTCDATE());

PRINT 'Tarif der Gruppe zugewiesen';
```

## Logs

Das System loggt alle Tarifberechnungen:

```
[Information] Calculated cost for session {SessionId}: 12.50 EUR (Tariff: Standard-Tarif)
[Warning] No tariff found for user {UserId}, using default rate
```

## Best Practices

1. **Immer einen Standard-Tarif definieren** (`IsDefault = true`)
2. **Gültigkeitszeiträume nutzen** für saisonale Tarife
3. **Prioritäten** bei mehreren Gruppen-Tarifen setzen
4. **Kulanzzeiten** für Blockiergebühren verwenden
5. **Mindest-/Höchstbeträge** zur Fairness

## Troubleshooting

### Problem: Kosten werden nicht berechnet
**Lösung**: Prüfen Sie, ob:
- Ein aktiver Tarif existiert (`IsActive = true`)
- Der Tarif im Gültigkeitszeitraum liegt
- Die Benutzergruppe dem Tarif zugeordnet ist
- Der Benutzer Mitglied der Gruppe ist

### Problem: Falscher Tarif wird verwendet
**Lösung**: Prüfen Sie die Prioritäten der Tarifzuordnungen

### Problem: TransactionId = 0
**Lösung**: Siehe `diagnose_charging_structure.sql` und `fix_missing_chargingpoints.sql`

## Weiterführende Dokumentation

- API-Dokumentation: `/swagger`
- OCPP-Integration: `OCPP-DOKUMENTATION.md`
- Berechtigungen: `PERMISSIONS.md`

