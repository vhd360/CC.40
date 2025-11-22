# 🚀 Quick Start mit Docker

Starten Sie CUBOS.Charge in weniger als 5 Minuten!

---

## ⚡ Option 1: Docker Compose (Empfohlen)

### Schritt 1: Voraussetzungen prüfen
```bash
docker --version  # Mindestens 20.10
docker-compose --version  # Mindestens 2.0
```

### Schritt 2: Starten
```bash
docker-compose up -d
```

### Schritt 3: Datenbank initialisieren
```bash
docker exec -it cubos-charge-api dotnet ef database update
```

### Schritt 4: Fertig! 🎉
- **Frontend:** http://localhost:3000
- **API:** http://localhost:5126
- **Swagger:** http://localhost:5126/swagger
- **OCPP Server:** ws://localhost:8080

---

## 🛠️ Option 2: Make Commands (Noch einfacher!)

```bash
# Installation
make install

# Starten
make up

# Logs anzeigen
make logs

# Migrations ausführen
make migrate

# Stoppen
make down

# Hilfe anzeigen
make help
```

---

## 🏗️ Entwicklungs-Modus mit Hot Reload

```bash
# Development starten
docker-compose -f docker-compose.dev.yml up

# Oder mit Make
make dev
```

**Vorteile:**
- ✅ Code-Änderungen werden sofort übernommen
- ✅ Keine Neukompilierung nötig
- ✅ Live-Reload für Frontend und Backend

---

## 🔍 Status prüfen

```bash
# Container-Status
docker ps

# Logs live anschauen
docker-compose logs -f

# Health Check
make health
```

---

## 🐛 Troubleshooting

### Problem: Container startet nicht
```bash
docker-compose logs api
docker-compose up -d --build
```

### Problem: Frontend-Build fehlgeschlagen
```bash
# Debug-Build testen
make test-frontend-docker

# Lokalen Build testen
cd frontend && npm ci --legacy-peer-deps

# Package-lock.json neu generieren
cd frontend && rm package-lock.json && npm install
```

### Problem: Port belegt
```bash
# Port 5126 freigeben (Windows PowerShell)
Get-Process -Id (Get-NetTCPConnection -LocalPort 5126).OwningProcess | Stop-Process

# Port 5126 freigeben (Linux/Mac)
lsof -ti:5126 | xargs kill -9
```

### Problem: Datenbank-Fehler
```bash
# Datenbank zurücksetzen
docker-compose down -v
docker-compose up -d
make migrate
```

---

## 🧹 Aufräumen

```bash
# Container stoppen und entfernen
docker-compose down

# Alles entfernen (inkl. Volumes)
docker-compose down -v

# Komplett aufräumen
make clean
```

---

## 📚 Weitere Informationen

Ausführliche Dokumentation: [DOCKER.md](DOCKER.md)

---

## 💡 Tipps

1. **Erste Anmeldung:** Erstellen Sie einen Tenant unter http://localhost:3000/register
2. **Admin-Login:** Nach Tenant-Erstellung können Sie sich anmelden
3. **OCPP-Station:** Verbinden Sie Ihre Ladestation zu `ws://localhost:8080`

---

**Viel Erfolg mit CUBOS.Charge! 🎉**


