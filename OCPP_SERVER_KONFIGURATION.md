# OCPP Server Konfiguration

## Standard-Konfiguration (localhost)

Der OCPP-Server läuft standardmäßig auf `http://localhost:9000/ocpp/`. Diese Konfiguration benötigt keine speziellen Berechtigungen.

```json
{
  "Ocpp": {
    "ServerUrl": "http://localhost:9000/ocpp/"
  }
}
```

## Auf externer IP-Adresse lauschen

Wenn Sie möchten, dass der OCPP-Server auf einer bestimmten IP-Adresse lauscht (z.B. `192.168.178.121`), gibt es zwei Möglichkeiten:

### Option 1: Mit Administrator-Rechten starten (einfachste Lösung)

Starten Sie Visual Studio oder das Terminal als Administrator:

```powershell
# Terminal als Administrator öffnen, dann:
cd D:\CC.40\ChargingControlSystem.Api
dotnet run
```

### Option 2: URL-Reservation erstellen (empfohlen für Produktion)

Diese Methode erlaubt es, die Anwendung ohne Admin-Rechte zu starten:

1. **PowerShell als Administrator öffnen**

2. **URL-Reservation erstellen:**

```powershell
# Für Ihre spezifische IP-Adresse:
netsh http add urlacl url=http://192.168.178.121:9000/ user=JEDER

# Oder für alle IP-Adressen:
netsh http add urlacl url=http://+:9000/ user=JEDER

# Für localhost (normalerweise nicht nötig):
netsh http add urlacl url=http://localhost:9000/ user=JEDER
```

3. **appsettings.json anpassen:**

```json
{
  "Ocpp": {
    "ServerUrl": "http://192.168.178.121:9000/ocpp/"
  }
}
```

4. **Anwendung normal starten (ohne Admin-Rechte)**

### Reservation überprüfen

```powershell
# Alle Reservationen anzeigen:
netsh http show urlacl

# Bestimmte Reservation suchen:
netsh http show urlacl | findstr "9000"
```

### Reservation entfernen

```powershell
# Falls Sie die Reservation wieder entfernen möchten:
netsh http delete urlacl url=http://192.168.178.121:9000/
```

## Firewall-Konfiguration

Wenn Sie von außerhalb des lokalen Computers auf den OCPP-Server zugreifen möchten, müssen Sie den Port in der Windows-Firewall freigeben:

```powershell
# Als Administrator:
New-NetFirewallRule -DisplayName "OCPP Server" -Direction Inbound -Protocol TCP -LocalPort 9000 -Action Allow
```

## Umgebungsspezifische Konfiguration

### appsettings.Development.json

Für die Entwicklung können Sie eine separate Konfiguration erstellen:

```json
{
  "Ocpp": {
    "ServerUrl": "http://localhost:9000/ocpp/"
  }
}
```

### appsettings.Production.json

Für die Produktion mit externer IP:

```json
{
  "Ocpp": {
    "ServerUrl": "http://192.168.178.121:9000/ocpp/"
  }
}
```

## Testen der Verbindung

Sie können die Verbindung mit einem WebSocket-Client testen:

```javascript
// Im Browser-Console oder mit einem WebSocket-Tool:
const ws = new WebSocket('ws://localhost:9000/ocpp/TEST-STATION-01');
ws.onopen = () => console.log('Verbunden!');
ws.onmessage = (event) => console.log('Nachricht:', event.data);
```

## Troubleshooting

### Fehler: "Zugriff verweigert" / "Access Denied"
- **Lösung 1**: Als Administrator starten
- **Lösung 2**: URL-Reservation erstellen (siehe oben)

### Fehler: "Address already in use"
- Port 9000 wird bereits verwendet
- **Lösung**: Anderen Port verwenden oder den blockierenden Prozess beenden

```powershell
# Port-Nutzung prüfen:
netstat -ano | findstr :9000

# Prozess beenden (mit der PID aus dem vorherigen Befehl):
taskkill /PID <PID> /F
```

### Keine Verbindung von externen Geräten
- Firewall-Regel überprüfen
- Netzwerk-Konfiguration prüfen (Router, etc.)
- IP-Adresse in der Konfiguration überprüfen

## Produktions-Deployment

Für Produktionsumgebungen empfehlen wir:

1. **Windows Service** mit URL-Reservation
2. **Reverse Proxy** (IIS oder Nginx) vor dem OCPP-Server
3. **TLS/SSL** für sichere Verbindungen (`wss://` statt `ws://`)
4. **Monitoring** und Logging aktivieren

