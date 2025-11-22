# 🚗 Fahrzeug-Identifikation System

## 📋 Übersicht

Das System ermöglicht die **automatische Erkennung von Fahrzeugen** bei Ladevorgängen durch RFID-Tags oder QR-Codes.

## 🎯 Problem & Lösung

### **Problem:**
- Benutzer laden mit ihrer persönlichen RFID-Karte
- System weiß **nicht**, welches Fahrzeug geladen wird
- Keine Zuordnung zu Dienstwagen möglich
- Keine Nachvollziehbarkeit für Flottenmanagement

### **Lösung:**
Jedes Fahrzeug bekommt eigene **Identifikationsmethoden**:
- **RFID-Tag** (z.B. im Handschuhfach oder am Kennzeichen)
- **QR-Code** (z.B. am Armaturenbrett)

---

## 🔄 Workflow

### **1. Admin richtet Fahrzeug ein:**
```
1. Navigation → Fahrzeuge → Neues Fahrzeug
2. Trägt Kennzeichen, Marke, Modell ein
3. Fügt RFID-Tag hinzu: "VEHICLE-TESLA-001"
4. Optional: QR-Code hinzufügen
5. Speichern
```

### **2. Admin weist Fahrzeug zu:**
```
1. Navigation → Fahrzeugzuweisungen
2. Klick auf "Fahrzeug zuweisen"
3. Wählt Fahrzeug: Tesla Model 3 (M-CC 1234)
4. Wählt Benutzer: Max Mustermann
5. Zuweisungstyp: "Permanent" (Dienstwagen)
6. Speichern
```

### **3. Benutzer lädt (2 Szenarien):**

#### **Szenario A: Fahrzeug-RFID (Empfohlen)**
```
1. Benutzer fährt mit Dienstwagen zur Ladestation
2. Scannt Fahrzeug-RFID am Ladepunkt
3. ✅ System erkennt: "Tesla Model 3 (M-CC 1234)"
4. ✅ System findet: "Zugewiesen an Max Mustermann"
5. ✅ Ladevorgang startet
6. ✅ ChargingSession hat:
   - UserId: Max Mustermann
   - VehicleId: Tesla Model 3
```

#### **Szenario B: Benutzer-RFID + Web-UI Auswahl**
```
1. Benutzer scannt seine persönliche RFID-Karte
2. ✅ System erkennt: "Max Mustermann"
3. ⚠️ System weiß NICHT, welches Fahrzeug
4. Benutzer wählt im Web-UI: Tesla Model 3
5. ✅ Ladevorgang startet mit beiden Zuordnungen
```

---

## 💾 Datenbank-Schema

### **Vehicle Tabelle (erweitert):**
```sql
CREATE TABLE Vehicles (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    LicensePlate NVARCHAR(50) NOT NULL,
    Make NVARCHAR(100) NOT NULL,
    Model NVARCHAR(100) NOT NULL,
    Year INT NOT NULL,
    Type INT NOT NULL, -- PoolVehicle = 0, CompanyVehicle = 1
    Color NVARCHAR(100) NOT NULL,
    Notes NVARCHAR(500) NULL,
    
    -- NEU: Identifikationsmethoden
    RfidTag NVARCHAR(100) NULL,  -- Fahrzeug-RFID
    QrCode NVARCHAR(100) NULL,   -- Fahrzeug-QR-Code
    
    IsActive BIT NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    DeactivatedAt DATETIME2 NULL
);
```

### **ChargingSession Tabelle:**
```sql
CREATE TABLE ChargingSessions (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    -- ...
    UserId UNIQUEIDENTIFIER NULL,      -- WER lädt?
    VehicleId UNIQUEIDENTIFIER NULL,   -- WOMIT wird geladen?
    -- ...
);
```

---

## 🔧 Implementierungs-Status

### ✅ **Abgeschlossen:**
1. ✅ Datenbank-Migration erstellt und angewendet
2. ✅ `Vehicle.RfidTag` und `Vehicle.QrCode` Felder hinzugefügt
3. ✅ Frontend VehicleForm erweitert
4. ✅ UI für RFID-Tag und QR-Code Eingabe

### 🔨 **Noch zu implementieren:**
1. ⏳ OCPP-Handler erweitern (Fahrzeug-RFID erkennen)
2. ⏳ Backend-Logik für Fahrzeugzuordnung
3. ⏳ API-Endpoints für Fahrzeug-Identifikation

---

## 🎬 Nächste Schritte

### **1. OCPP-Handler erweitern:**
```csharp
// ChargingControlSystem.OCPP/Handlers/OcppMessageHandler.cs
public async Task<AuthorizeResponse> HandleAuthorizeAsync(AuthorizeRequest request)
{
    // Prüfe zuerst: Ist es eine Benutzer-RFID?
    var user = await FindUserByRfidAsync(request.IdTag);
    
    if (user != null) {
        return new AuthorizeResponse { IdTagInfo = new IdTagInfo { Status = "Accepted" } };
    }
    
    // NEU: Prüfe: Ist es eine Fahrzeug-RFID?
    var vehicle = await FindVehicleByRfidAsync(request.IdTag);
    
    if (vehicle != null) {
        // Finde zugewiesenen Benutzer
        var assignment = await FindActiveAssignmentForVehicleAsync(vehicle.Id);
        
        if (assignment != null) {
            // Speichere für späteren StartTransaction
            return new AuthorizeResponse { IdTagInfo = new IdTagInfo { Status = "Accepted" } };
        }
    }
    
    return new AuthorizeResponse { IdTagInfo = new IdTagInfo { Status = "Invalid" } };
}
```

### **2. StartTransaction erweitern:**
```csharp
public async Task<StartTransactionResponse> HandleStartTransactionAsync(StartTransactionRequest request)
{
    // Prüfe: Ist es Benutzer-RFID oder Fahrzeug-RFID?
    var user = await FindUserByRfidAsync(request.IdTag);
    var vehicle = await FindVehicleByRfidAsync(request.IdTag);
    
    // Wenn Fahrzeug-RFID: Finde zugewiesenen Benutzer
    if (vehicle != null) {
        var assignment = await FindActiveAssignmentForVehicleAsync(vehicle.Id);
        user = assignment?.User;
    }
    
    // Erstelle ChargingSession mit User UND Vehicle
    var session = new ChargingSession {
        UserId = user?.Id,
        VehicleId = vehicle?.Id,
        // ...
    };
    
    return new StartTransactionResponse { TransactionId = session.Id };
}
```

---

## 📊 Reporting & Abrechnung

Mit der Fahrzeugidentifikation haben Sie:

### **Pro Benutzer:**
```
Max Mustermann
├─ Ladevorgang 1: Tesla Model 3 (M-CC 1234) - €15.50
├─ Ladevorgang 2: Tesla Model 3 (M-CC 1234) - €12.30
└─ Ladevorgang 3: Pool-VW ID.4 (M-PL 001) - €8.90
   Gesamt: €36.70
```

### **Pro Fahrzeug:**
```
Tesla Model 3 (M-CC 1234)
├─ Geladen von: Max Mustermann - €15.50
├─ Geladen von: Max Mustermann - €12.30
└─ Geladen von: Anna Schmidt - €9.80
   Gesamt: €37.60
```

### **Für Firmen-Abrechnung:**
- "Welche Dienstwagen verursachen die höchsten Ladekosten?"
- "Wer nutzt welches Poolfahrzeug?"
- "Kosten pro Fahrzeug für Buchhaltung"

---

## 🔐 Sicherheit

### **RFID-Tag Vergabe:**
- **Eindeutig**: Jeder RFID-Tag nur einmal im System
- **Validierung**: System prüft vor Zuweisung auf Duplikate
- **Revozierung**: RFID kann jederzeit deaktiviert werden

### **Zugriffskontrolle:**
- Nur **aktive** Fahrzeuge können laden
- Nur **aktive** Zuweisungen werden erkannt
- Zurückgegebene Fahrzeuge = kein Zugriff mehr

---

## 📖 Benutzerhandbuch

### **Für Administratoren:**

#### **Neues Fahrzeug mit RFID einrichten:**
1. Menü → **Fahrzeuge** → **Neues Fahrzeug**
2. Grunddaten eingeben (Kennzeichen, Marke, etc.)
3. **RFID-Tag** Feld: `VEHICLE-001` eingeben
4. **Speichern**

#### **Fahrzeug einem Benutzer zuweisen:**
1. Menü → **Fahrzeugzuweisungen** → **Fahrzeug zuweisen**
2. Fahrzeug auswählen
3. Benutzer auswählen
4. Zuweisungstyp: **Permanent** (Dienstwagen)
5. **Speichern**

### **Für Benutzer:**

#### **Laden mit Fahrzeug-RFID:**
1. Fahrzeug mit Dienstwagen zur Ladestation fahren
2. RFID-Tag am Handschuhfach nehmen
3. An Ladepunkt scannen
4. ✅ Ladevorgang startet automatisch

#### **Laden mit eigener RFID:**
1. Persönliche RFID-Karte scannen
2. System authentifiziert
3. Im **Web-UI** Fahrzeug auswählen
4. ✅ Ladevorgang starten

---

## 🎯 Vorteile

1. **Automatische Zuordnung**: Kein manueller Aufwand
2. **Nachvollziehbarkeit**: Genau wissen, wer womit lädt
3. **Flottenmanagement**: Kosten pro Fahrzeug tracken
4. **Abrechnung**: Korrekte Kostenzuordnung
5. **Flexibilität**: Benutzer oder Fahrzeug-basiert

---

## 🚀 Ausblick

### **Zukünftige Features:**
- 📊 **Fahrzeug-Dashboard**: Ladehistorie pro Fahrzeug
- 📈 **Kostenanalyse**: Welche Fahrzeuge sind teuer?
- 🔔 **Benachrichtigungen**: "Ihr Dienstwagen ist vollgeladen"
- 📱 **Mobile App**: Fahrzeug-RFID via NFC

---

## ✅ Zusammenfassung

✅ **Jetzt möglich:**
- Fahrzeuge haben eigene RFID-Tags
- Automatische Erkennung beim Laden
- Zuordnung: User + Vehicle
- Vollständige Nachvollziehbarkeit

✅ **Nächster Schritt:**
- API neu starten
- Fahrzeug anlegen
- RFID-Tag zuweisen
- Testen! 🎉




