# Charging Control System - Implementierungs-Zusammenfassung

## ✅ Implementierte Features

### 1. **Identifikationsmethoden (RFID, Autocharge, etc.)**

#### Backend Entity: `AuthorizationMethod`
- **Typen**: RFID, Autocharge, App, QRCode, CreditCard, PlugAndCharge
- **Eigenschaften**:
  - `Identifier`: RFID-Tag-Nummer, VIN, etc.
  - `FriendlyName`: Benutzerfreundlicher Name (z.B. "Meine RFID-Karte")
  - `ValidFrom` / `ValidUntil`: Gültigkeitszeitraum
  - `IsActive`: Aktiv/Inaktiv Status
  - `LastUsedAt`: Letzte Verwendung
  - `Metadata`: Zusätzliche Daten als JSON

#### Backend API-Endpunkte
- `GET /api/authorization-methods` - Alle Identifikationsmethoden eines Tenants
- `GET /api/authorization-methods/user/{userId}` - Methoden eines Benutzers
- `POST /api/authorization-methods` - Neue Methode anlegen
- `PUT /api/authorization-methods/{id}` - Methode aktualisieren
- `DELETE /api/authorization-methods/{id}` - Methode löschen (Soft Delete)
- `POST /api/authorization-methods/verify` - Autorisierung verifizieren (für OCPP)

#### Integration in ChargingSession
- `ChargingSession` hat jetzt `AuthorizationMethodId`
- Beim Start einer Ladesession wird die verwendete Autorisierungsmethode gespeichert

### 2. **Erweiterte Beziehungen**

#### Ladeparks ↔ Ladepunkt-Gruppen
- `ChargingStationGroup` hat jetzt optional `ChargingParkId`
- Ladepunkt-Gruppen können einem bestimmten Ladepark zugeordnet werden
- Oder tenantübergreifend (innerhalb eines Tenants) verwendet werden

#### Nutzergruppen ↔ Ladepunkt-Gruppen
- Neue Entity: `UserGroupChargingStationGroupPermission`
- Nutzergruppen können Zugriff auf bestimmte Ladepunkt-Gruppen erhalten
- Tenantübergreifend innerhalb eines Tenants

### 3. **Datenbank-Schema**

Neue Tabellen:
- `AuthorizationMethods` - Speichert RFID, Autocharge, etc.
- `UserGroupChargingStationGroupPermissions` - Berechtigungen

Erweiterte Tabellen:
- `ChargingStationGroups` - +ChargingParkId
- `ChargingSessions` - +AuthorizationMethodId

## 📍 Wo finde ich was?

### Im Backend (.NET 8):

1. **Identifikationsmethoden verwalten**
   - Controller: `ChargingControlSystem.Api/Controllers/AuthorizationMethodsController.cs`
   - Entity: `ChargingControlSystem.Data/Entities/AuthorizationMethod.cs`

2. **RFID-Karte hinzufügen (Beispiel via API)**
```json
POST /api/authorization-methods
{
  "userId": "guid",
  "type": 0,  // 0=RFID, 1=Autocharge, 2=App, 3=QRCode, 4=CreditCard, 5=PlugAndCharge
  "identifier": "0123456789ABCDEF",
  "friendlyName": "Meine RFID-Karte",
  "validFrom": null,
  "validUntil": null,
  "metadata": null
}
```

3. **Autocharge hinzufügen (Beispiel)**
```json
POST /api/authorization-methods
{
  "userId": "guid",
  "type": 1,  // Autocharge
  "identifier": "WVW1234567890123",  // VIN des Fahrzeugs
  "friendlyName": "Tesla Model 3",
  "validFrom": null,
  "validUntil": null,
  "metadata": "{\"manufacturer\": \"Tesla\", \"model\": \"Model 3\"}"
}
```

4. **OCPP Integration - Autorisierung verifizieren**
```json
POST /api/authorization-methods/verify
{
  "type": 0,  // RFID
  "identifier": "0123456789ABCDEF"
}
```

### Im Frontend (React):

**TODO**: Frontend-Seite für Identifikationsmethoden erstellen
- Empfohlener Pfad: `/authorization-methods` oder `/user-profile/:id/authorization-methods`
- Sollte folgendes enthalten:
  - Liste aller Identifikationsmethoden eines Benutzers
  - Formular zum Hinzufügen neuer RFID-Karten
  - Formular zum Hinzufügen von Autocharge (VIN-basiert)
  - Aktivieren/Deaktivieren von Methoden
  - Setzen von Gültigkeitszeiträumen

## 🔑 Verwendungsszenarien

### 1. RFID-Karte registrieren
1. Benutzer navigiert zu seinem Profil
2. Klickt auf "Neue Identifikationsmethode"
3. Wählt "RFID"
4. Gibt RFID-Tag-Nummer ein
5. Optional: Gültigkeitszeitraum festlegen

### 2. Autocharge (Plug & Charge) einrichten
1. Benutzer wählt sein Fahrzeug
2. System extrahiert VIN
3. Erstellt AuthorizationMethod vom Typ "Autocharge"
4. Bei Plug-In am Ladepunkt: System erkennt VIN automatisch

### 3. Ladesession mit RFID starten
1. Benutzer hält RFID-Karte an Ladestation
2. OCPP-Server ruft `/api/authorization-methods/verify` auf
3. System prüft Berechtigung
4. Bei Erfolg: Session wird gestartet mit `AuthorizationMethodId`

## 📊 Datenbankstruktur

```
User (Benutzer)
  └─► AuthorizationMethods (Identifikationsmethoden)
       └─► ChargingSessions (verwendet bei Sessions)

UserGroup (Nutzergruppe)
  └─► UserGroupChargingStationGroupPermissions
       └─► ChargingStationGroup (Zugriff auf Ladepunkt-Gruppen)

ChargingPark (Ladepark)
  └─► ChargingStationGroup (optional zugeordnet)
       └─► ChargingStationGroupMemberships
            └─► ChargingStation (Ladestationen)
```

## 🚀 Nächste Schritte

1. **Frontend für Identifikationsmethoden erstellen**
   - Komponente: `frontend/src/pages/AuthorizationMethods.tsx`
   - API-Services erweitern in `frontend/src/services/api.ts`
   - Route hinzufügen in `frontend/src/App.tsx`

2. **OCPP-Integration**
   - OCPP-Server muss `/api/authorization-methods/verify` aufrufen
   - Bei RemoteStartTransaction: AuthorizationMethodId mitgeben

3. **Berechtigungen erweitern**
   - UI für UserGroup ↔ ChargingStationGroup Berechtigungen
   - Zugriffskontrollen bei Session-Start

## 📝 Hinweise

- **Sicherheit**: RFID-Nummern sollten gehasht gespeichert werden (für Produktionssystem)
- **Autocharge**: Benötigt ISO 15118 Support an Ladestationen
- **Gültigkeitszeiträume**: Werden bei Verify automatisch geprüft
- **Soft Delete**: Deaktivierte Methoden bleiben für Audit-Trail erhalten

## 🎯 Backend läuft auf
- **API**: http://localhost:5126
- **Swagger**: http://localhost:5126/swagger

## 🎯 Frontend läuft auf
- **React App**: http://localhost:3000

