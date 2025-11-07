-- ===================================================================
-- Datenbank-Update für ChargingPoint-Struktur (MS SQL Server)
-- ===================================================================
-- ACHTUNG: Dieses Skript muss NACH der EF-Migration ausgeführt werden!
-- 1. Führen Sie erst aus: dotnet ef database update --project ChargingControlSystem.Data --startup-project ChargingControlSystem.Api
-- 2. Dann führen Sie dieses Skript aus
-- ===================================================================

-- ChargingPoint für Ladestation "Tester002" erstellen
DECLARE @station_id UNIQUEIDENTIFIER;
DECLARE @charging_point_id UNIQUEIDENTIFIER;

-- Station-ID ermitteln
SELECT TOP 1 @station_id = Id
FROM ChargingStations
WHERE ChargeBoxId = 'Tester002';

IF @station_id IS NOT NULL
BEGIN
    -- Neue GUIDs generieren
    SET @charging_point_id = NEWID();
    
    -- ChargingPoint erstellen
    INSERT INTO ChargingPoints (
        Id, ChargingStationId, EvseId, Name, 
        MaxPower, Status, SupportsSmartCharging, 
        SupportsRemoteStartStop, SupportsReservation,
        IsActive, CreatedAt
    ) VALUES (
        @charging_point_id,
        @station_id,
        1, -- OCPP ConnectorId = 1
        'Ladepunkt 1',
        22, -- 22 kW
        0, -- Available
        0, -- false
        1, -- true
        0, -- false
        1, -- true
        GETUTCDATE()
    );
    
    -- Connector für den ChargingPoint erstellen
    INSERT INTO ChargingConnectors (
        Id, ChargingPointId, ConnectorId, ConnectorType,
        PowerType, MaxPower, MaxCurrent, MaxVoltage,
        Status, IsActive, CreatedAt
    ) VALUES (
        NEWID(),
        @charging_point_id,
        1,
        'Type2',
        'AC_3_PHASE',
        22,
        32,
        230,
        0, -- Available
        1, -- true
        GETUTCDATE()
    );
    
    PRINT '✅ ChargingPoint und Connector für Tester002 erstellt!';
END
ELSE
BEGIN
    PRINT '❌ Station "Tester002" nicht gefunden!';
END

GO

-- Überprüfung
SELECT 
    cs.Name AS Station,
    cs.ChargeBoxId,
    cp.EvseId,
    cp.Name AS ChargingPoint,
    cp.MaxPower AS CP_MaxPower,
    c.ConnectorId,
    c.ConnectorType,
    c.Status
FROM ChargingStations cs
JOIN ChargingPoints cp ON cp.ChargingStationId = cs.Id
JOIN ChargingConnectors c ON c.ChargingPointId = cp.Id
WHERE cs.ChargeBoxId = 'Tester002';

SELECT '🎉 Setup abgeschlossen!' AS Status;

