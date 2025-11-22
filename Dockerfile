# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["ChargingControlSystem.Api/ChargingControlSystem.Api.csproj", "ChargingControlSystem.Api/"]
COPY ["ChargingControlSystem.Data/ChargingControlSystem.Data.csproj", "ChargingControlSystem.Data/"]
COPY ["ChargingControlSystem.OCPP/ChargingControlSystem.OCPP.csproj", "ChargingControlSystem.OCPP/"]
RUN dotnet restore "ChargingControlSystem.Api/ChargingControlSystem.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/ChargingControlSystem.Api"
RUN dotnet build "ChargingControlSystem.Api.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "ChargingControlSystem.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl for healthchecks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

EXPOSE 80
EXPOSE 443
EXPOSE 8080

COPY --from=publish /app/publish .

# Environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Healthcheck
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:80/health || exit 1

ENTRYPOINT ["dotnet", "ChargingControlSystem.Api.dll"]




