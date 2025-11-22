# 🐳 Docker Deployment Guide - CUBOS.Charge

Vollständige Anleitung für die Containerisierung und Bereitstellung des ChargingControlSystem.

---

## 📋 Übersicht

Das Projekt besteht aus drei Hauptkomponenten:

1. **Backend API** (.NET 8) - Port 5126
2. **OCPP Server** (WebSocket) - Port 8080
3. **Frontend** (React) - Port 3000
4. **Datenbank** (SQL Server) - Port 1433

---

## 🚀 Quick Start (Production)

### Voraussetzungen:
- Docker 20.10+
- Docker Compose 2.0+

### Schritt 1: Projekt klonen
```bash
git clone <repository-url>
cd CC.40
```

### Schritt 2: Umgebungsvariablen konfigurieren (Optional)
Erstellen Sie eine `.env` Datei im Hauptverzeichnis:

```env
# Datenbank
DB_SA_PASSWORD=YourStrong!Password123

# JWT
JWT_SECRET=YourSuperSecretKeyThatIsAtLeast32CharactersLong!
JWT_ISSUER=ChargingControlSystem
JWT_AUDIENCE=ChargingControlSystemUsers
JWT_EXPIRATION_MINUTES=43200

# Ports
API_PORT=5126
FRONTEND_PORT=3000
OCPP_PORT=8080
DB_PORT=1433
```

### Schritt 3: Container starten
```bash
docker-compose up -d
```

### Schritt 4: Datenbank-Migration ausführen
```bash
docker exec -it cubos-charge-api dotnet ef database update
```

### Schritt 5: Zugriff auf die Anwendung
- **Frontend:** http://localhost:3000
- **API:** http://localhost:5126
- **OCPP Server:** ws://localhost:8080
- **API Swagger:** http://localhost:5126/swagger

---

## 🛠️ Entwicklungs-Umgebung

Für die Entwicklung mit Hot Reload:

```bash
docker-compose -f docker-compose.dev.yml up
```

**Vorteile:**
- ✅ Automatische Code-Neukompilierung (Hot Reload)
- ✅ Volume-Mounts für lokale Änderungen
- ✅ Entwickler-freundliche Logs
- ✅ Schnellere Iterationen

---

## 🏗️ Build-Prozess

### Backend (Multi-Stage Build)

```dockerfile
Stage 1: Build     → SDK Container, kompiliert den Code
Stage 2: Publish   → Veröffentlicht die Anwendung
Stage 3: Runtime   → Schlankes Runtime-Image (~200MB)
```

**Vorteile:**
- Kleines finales Image
- Keine Build-Tools im Production-Image
- Sichere Laufzeitumgebung

### Frontend (Multi-Stage Build)

```dockerfile
Stage 1: Build       → Node Container, baut React App
Stage 2: Production  → Nginx Alpine, serviert statische Dateien
```

**Vorteile:**
- Extrem kleines Image (~25MB)
- Nginx für optimale Performance
- Gzip-Kompression aktiviert
- Caching für statische Assets

---

## 📦 Container-Details

### 1. Backend API Container

**Image:** `cubos-charge-api`
**Base:** `mcr.microsoft.com/dotnet/aspnet:8.0`

**Exposed Ports:**
- `80` - HTTP API
- `8080` - OCPP WebSocket Server

**Environment Variables:**
```env
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=...
Jwt__Secret=...
OCPP__ServerPort=8080
```

**Health Check:**
```bash
curl -f http://localhost:80/health || exit 1
```

---

### 2. Frontend Container

**Image:** `cubos-charge-frontend`
**Base:** `nginx:alpine`

**Exposed Ports:**
- `80` - HTTP (gemappt auf 3000)

**Features:**
- React Router Support (try_files)
- Gzip Compression
- Static Asset Caching (1 Jahr)
- Security Headers

**Health Check:**
```bash
wget --quiet --tries=1 --spider http://localhost/
```

---

### 3. Database Container

**Image:** `mcr.microsoft.com/mssql/server:2022-latest`

**Exposed Ports:**
- `1433` - SQL Server

**Environment Variables:**
```env
ACCEPT_EULA=Y
SA_PASSWORD=YourStrong!Password123
MSSQL_PID=Developer
```

**Persistent Volume:**
- `sqlserver-data:/var/opt/mssql`

---

## 🔧 Nützliche Docker-Befehle

### Container-Management

```bash
# Alle Container starten
docker-compose up -d

# Container stoppen
docker-compose stop

# Container stoppen und entfernen
docker-compose down

# Container neu bauen und starten
docker-compose up -d --build

# Logs anzeigen
docker-compose logs -f

# Logs für einen Service
docker-compose logs -f api

# Container-Status prüfen
docker-compose ps

# In Container einsteigen
docker exec -it cubos-charge-api bash
```

### Datenbank-Operationen

```bash
# Migration anwenden
docker exec -it cubos-charge-api dotnet ef database update

# Neue Migration erstellen
docker exec -it cubos-charge-api dotnet ef migrations add MigrationName

# Datenbank-Backup
docker exec cubos-charge-db /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P 'YourStrong!Password123' \
  -Q "BACKUP DATABASE [ChargingControlSystem] TO DISK='/var/opt/mssql/backup/db.bak'"

# Backup aus Container kopieren
docker cp cubos-charge-db:/var/opt/mssql/backup/db.bak ./backup.bak
```

### Debugging

```bash
# Container-Logs live anzeigen
docker-compose logs -f --tail=100

# Container-Ressourcen anzeigen
docker stats

# Netzwerk inspizieren
docker network inspect cubos-network

# Container-Details anzeigen
docker inspect cubos-charge-api
```

---

## 🌐 Netzwerk-Konfiguration

### Internes Netzwerk: `cubos-network`

```
frontend (port 3000)
    ↓
api (port 5126, 8080)
    ↓
db (port 1433)
```

**Service Discovery:**
- Container können sich gegenseitig über Service-Namen erreichen
- Beispiel: `api` → `db:1433`

---

## 🔒 Sicherheits-Best-Practices

### 1. Secrets Management

**NICHT committen:**
```env
JWT_SECRET=...
DB_PASSWORD=...
```

**Verwenden Sie:**
- Docker Secrets
- Externe Secret-Management-Tools (Azure Key Vault, AWS Secrets Manager)
- `.env` Dateien (nicht in Git)

### 2. Non-Root User

```dockerfile
# Im Dockerfile
RUN adduser --disabled-password --gecos '' appuser
USER appuser
```

### 3. Security Headers

Nginx konfiguriert:
```nginx
add_header X-Frame-Options "SAMEORIGIN";
add_header X-Content-Type-Options "nosniff";
add_header X-XSS-Protection "1; mode=block";
```

---

## 📊 Monitoring & Health Checks

### Health Check Endpoints

**API:**
```bash
curl http://localhost:5126/health
```

**Frontend:**
```bash
curl http://localhost:3000/health
```

**Datenbank:**
```bash
docker exec cubos-charge-db /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P 'YourStrong!Password123' -Q "SELECT 1"
```

### Docker Health Status

```bash
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
```

---

## 🚢 Production Deployment

### Option 1: Docker Compose (Single Server)

```bash
# Production starten
docker-compose -f docker-compose.yml up -d

# Mit custom .env
docker-compose --env-file .env.production up -d
```

### Option 2: Kubernetes (Empfohlen für Skalierung)

```bash
# Images bauen
docker build -t cubos-charge-api:latest .
docker build -t cubos-charge-frontend:latest ./frontend

# Images pushen (zu Container Registry)
docker tag cubos-charge-api:latest your-registry/cubos-charge-api:latest
docker push your-registry/cubos-charge-api:latest

# Kubernetes Deployment
kubectl apply -f k8s/
```

### Option 3: Cloud Services

#### Azure Container Apps
```bash
az containerapp up \
  --name cubos-charge \
  --resource-group rg-cubos \
  --location westeurope \
  --source .
```

#### AWS ECS
```bash
ecs-cli compose up --cluster cubos-cluster
```

---

## 🔄 CI/CD Pipeline

### Beispiel: GitHub Actions

```yaml
name: Build and Deploy

on:
  push:
    branches: [main]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Build Backend
        run: docker build -t cubos-charge-api .
      
      - name: Build Frontend
        run: docker build -t cubos-charge-frontend ./frontend
      
      - name: Push to Registry
        run: |
          docker tag cubos-charge-api ${{ secrets.REGISTRY }}/cubos-charge-api:latest
          docker push ${{ secrets.REGISTRY }}/cubos-charge-api:latest
```

---

## 🐛 Troubleshooting

### Problem: Container startet nicht

```bash
# Logs prüfen
docker-compose logs api

# Container-Status prüfen
docker ps -a

# Neu bauen
docker-compose up -d --build --force-recreate
```

### Problem: Datenbank-Verbindung fehlgeschlagen

```bash
# Prüfen, ob DB Container läuft
docker ps | grep db

# DB Health Check
docker exec cubos-charge-db /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'YourStrong!Password123' -Q "SELECT 1"

# Connection String prüfen
docker exec cubos-charge-api env | grep ConnectionStrings
```

### Problem: Frontend kann API nicht erreichen

```bash
# Netzwerk prüfen
docker network inspect cubos-network

# API Erreichbarkeit testen
docker exec cubos-charge-frontend wget -O- http://api:80/health
```

### Problem: Frontend-Build schlägt fehl (npm ci Fehler)

```bash
# Debug-Build testen
make test-frontend-docker

# Lokalen Build testen
cd frontend && npm ci --legacy-peer-deps

# Package-lock.json neu generieren
cd frontend && rm package-lock.json && npm install

# Node-Module Cache löschen
cd frontend && rm -rf node_modules && npm cache clean --force

# Mit alternativer Node-Version testen
docker run --rm -it -v $(pwd)/frontend:/app node:20-alpine sh -c "cd /app && npm ci --legacy-peer-deps"
```

### Problem: React 19 Kompatibilitätsprobleme

```bash
# Legacy Peer Deps verwenden
npm ci --legacy-peer-deps

# Oder package.json anpassen (temporär)
"overrides": {
  "react": "^18.0.0",
  "react-dom": "^18.0.0"
}
```

### Problem: Port bereits belegt

```bash
# Port freigeben
lsof -ti:5126 | xargs kill -9

# Alternative Ports verwenden
docker-compose -f docker-compose.yml up -d -e API_PORT=5127
```

---

## 📈 Performance-Optimierung

### 1. Multi-Stage Build
✅ Bereits implementiert

### 2. Layer Caching
```dockerfile
# Dependencies zuerst kopieren (cacht besser)
COPY package*.json ./
RUN npm ci

# Dann Source Code
COPY . .
```

### 3. Nginx Tuning
```nginx
worker_processes auto;
gzip on;
gzip_comp_level 6;
```

### 4. Resource Limits

```yaml
services:
  api:
    deploy:
      resources:
        limits:
          cpus: '2'
          memory: 2G
        reservations:
          cpus: '1'
          memory: 1G
```

---

## 🧪 Testing

### Container Tests

```bash
# Health Checks testen
docker-compose ps

# API Tests
docker exec cubos-charge-api dotnet test

# Integration Tests
docker-compose -f docker-compose.test.yml up --abort-on-container-exit
```

---

## 📝 Checkliste für Production

- [ ] Secrets aus Environment Variables
- [ ] HTTPS aktiviert (Reverse Proxy)
- [ ] Health Checks konfiguriert
- [ ] Logging aktiviert (ELK Stack, Grafana)
- [ ] Backups automatisiert
- [ ] Resource Limits gesetzt
- [ ] Monitoring aktiviert
- [ ] SSL-Zertifikate installiert
- [ ] Firewall konfiguriert
- [ ] Non-root User verwendet

---

## 🆘 Support

Bei Fragen oder Problemen:
1. Logs prüfen: `docker-compose logs -f`
2. Health Status: `docker ps`
3. Container neu starten: `docker-compose restart api`

---

## 📚 Weiterführende Links

- [Docker Dokumentation](https://docs.docker.com/)
- [.NET Docker Guide](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/)
- [Nginx Docker](https://hub.docker.com/_/nginx)
- [SQL Server Container](https://hub.docker.com/_/microsoft-mssql-server)

---

**Version:** 1.0  
**Letzte Aktualisierung:** 2025-11-07  
**Projekt:** CUBOS.Charge ChargingControlSystem


