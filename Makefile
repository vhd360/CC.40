.PHONY: help build up down restart logs clean migrate db-backup

# Default target
help:
	@echo "CUBOS.Charge - Docker Commands"
	@echo "==============================="
	@echo "make build          - Build all containers"
	@echo "make up             - Start all containers (production)"
	@echo "make down           - Stop and remove all containers"
	@echo "make restart        - Restart all containers"
	@echo "make logs           - Show logs (all containers)"
	@echo "make logs-api       - Show API logs"
	@echo "make logs-frontend  - Show Frontend logs"
	@echo "make logs-db        - Show Database logs"
	@echo "make clean          - Remove all containers, volumes, and images"
	@echo "make migrate        - Run database migrations"
	@echo "make db-backup      - Backup database"
	@echo "make dev            - Start development environment"
	@echo "make dev-down       - Stop development environment"
	@echo "make test           - Run tests"
	@echo "make health         - Check health status"

# Production Commands
build:
	docker-compose build

up:
	docker-compose up -d

down:
	docker-compose down

restart:
	docker-compose restart

logs:
	docker-compose logs -f

logs-api:
	docker-compose logs -f api

logs-frontend:
	docker-compose logs -f frontend

logs-db:
	docker-compose logs -f db

clean:
	docker-compose down -v --rmi all
	docker system prune -af

migrate:
	docker exec -it cubos-charge-api dotnet ef database update --project ChargingControlSystem.Data --startup-project ChargingControlSystem.Api

db-backup:
	@echo "Creating database backup..."
	docker exec cubos-charge-db /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'YourStrong!Password123' -Q "BACKUP DATABASE [ChargingControlSystem] TO DISK='/var/opt/mssql/backup/db_$(shell date +%Y%m%d_%H%M%S).bak'"
	docker cp cubos-charge-db:/var/opt/mssql/backup/. ./backups/
	@echo "Backup completed! Files in ./backups/"

# Development Commands
dev:
	docker-compose -f docker-compose.dev.yml up

dev-down:
	docker-compose -f docker-compose.dev.yml down

dev-build:
	docker-compose -f docker-compose.dev.yml up --build

# Testing
test:
	docker exec -it cubos-charge-api dotnet test

test-frontend:
	cd frontend && npm test -- --watchAll=false

test-frontend-docker:
	docker build -f frontend/Dockerfile.debug -t cubos-charge-frontend-debug ./frontend

# Health Check
health:
	@echo "=== Container Health Status ==="
	@docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
	@echo ""
	@echo "=== API Health Check ==="
	@curl -f http://localhost:5126/health && echo " - OK" || echo " - FAILED"
	@echo ""
	@echo "=== Frontend Health Check ==="
	@curl -f http://localhost:3000/health && echo " - OK" || echo " - FAILED"

# Installation
install:
	@echo "Creating necessary directories..."
	mkdir -p backups
	@echo "Done! Run 'make up' to start the application."


