-- ========================================
-- FIX: Fehlende ChargingPoints und Connectors anlegen
-- ========================================
-- Dieses Script legt für alle Stationen ohne ChargingPoints
-- automatisch ChargingPoints und Connectors an

BEGIN TRANSACTION;

PRINT '=== START: ChargingPoints und Connectors anlegen ===';
PRINT '';

-- ========================================
-- 1. AC Standardladesäule 1
-- ========================================
DECLARE @StationId1 UNIQUEIDENTIFIER = '55555555-5555-5555-5555-555555555555';
DECLARE @ChargingPointId1 UNIQUEIDENTIFIER = NEWID();
DECLARE @ConnectorId1 UNIQUEIDENTIFIER = NEWID();

IF NOT EXISTS (SELECT 1 FROM ChargingPoints WHERE ChargingStationId = @StationId1)
BEGIN
    PRINT '1. Lege ChargingPoint für AC Standardladesäule 1 an...';
    
    -- ChargingPoint anlegen
    INSERT INTO ChargingPoints (
        Id, ChargingStationId, EvseId, Name, Description,
        MaxPower, Status, SupportsSmartCharging, SupportsRemoteStartStop,
        SupportsReservation, IsActive, CreatedAt
    )
    VALUES (
        @ChargingPointId1,
        @StationId1,
        1, -- EvseId = 1 (OCPP ConnectorId)
        'EVSE 1',
        'AC Ladepunkt 1',
        22, -- 22 kW
        0, -- Available
        0, -- SupportsSmartCharging = false
        1, -- SupportsRemoteStartStop = true
        0, -- SupportsReservation = false
        1, -- IsActive = true
        GETUTCDATE()
    );
    
    -- Connector anlegen (Type2, 22kW AC)
    INSERT INTO ChargingConnectors (
        Id, ChargingPointId, ConnectorId, ConnectorType, ConnectorFormat,
        PowerType, MaxPower, MaxCurrent, MaxVoltage, Status,
        PhysicalReference, IsActive, CreatedAt
    )
    VALUES (
        @ConnectorId1,
        @ChargingPointId1,
        1, -- ConnectorId innerhalb des ChargingPoints
        'Type2',
        'SOCKET',
        'AC_3_PHASE',
        22, -- 22 kW
        32, -- 32 A
        400, -- 400 V
        0, -- Available
        'Connector 1',
        1, -- IsActive = true
        GETUTCDATE()
    );
    
    PRINT '   ✓ ChargingPoint und Connector für AC Standardladesäule 1 angelegt';
END
ELSE
BEGIN
    PRINT '   → AC Standardladesäule 1 hat bereits ChargingPoints';
END

PRINT '';

-- ========================================
-- 2. LS 1 (ccb7e94d-2864-482e-bee2-cb771f786140)
-- ========================================
DECLARE @StationId2 UNIQUEIDENTIFIER = '6722C35B-CED7-40F4-A4AD-6EB1D7FBCA7B';
DECLARE @ChargingPointId2 UNIQUEIDENTIFIER = NEWID();
DECLARE @ConnectorId2 UNIQUEIDENTIFIER = NEWID();

IF NOT EXISTS (SELECT 1 FROM ChargingPoints WHERE ChargingStationId = @StationId2)
BEGIN
    PRINT '2. Lege ChargingPoint für LS 1 (ccb7e94d) an...';
    
    -- ChargingPoint anlegen
    INSERT INTO ChargingPoints (
        Id, ChargingStationId, EvseId, Name, Description,
        MaxPower, Status, SupportsSmartCharging, SupportsRemoteStartStop,
        SupportsReservation, IsActive, CreatedAt
    )
    VALUES (
        @ChargingPointId2,
        @StationId2,
        1, -- EvseId = 1 (OCPP ConnectorId)
        'EVSE 1',
        'Ladepunkt 1',
        22, -- 22 kW
        0, -- Available
        0, -- SupportsSmartCharging = false
        1, -- SupportsRemoteStartStop = true
        0, -- SupportsReservation = false
        1, -- IsActive = true
        GETUTCDATE()
    );
    
    -- Connector anlegen (Type2, 22kW AC)
    INSERT INTO ChargingConnectors (
        Id, ChargingPointId, ConnectorId, ConnectorType, ConnectorFormat,
        PowerType, MaxPower, MaxCurrent, MaxVoltage, Status,
        PhysicalReference, IsActive, CreatedAt
    )
    VALUES (
        @ConnectorId2,
        @ChargingPointId2,
        1, -- ConnectorId innerhalb des ChargingPoints
        'Type2',
        'SOCKET',
        'AC_3_PHASE',
        22, -- 22 kW
        32, -- 32 A
        400, -- 400 V
        0, -- Available
        'Connector 1',
        1, -- IsActive = true
        GETUTCDATE()
    );
    
    PRINT '   ✓ ChargingPoint und Connector für LS 1 (ccb7e94d) angelegt';
END
ELSE
BEGIN
    PRINT '   → LS 1 (ccb7e94d) hat bereits ChargingPoints';
END

PRINT '';

-- ========================================
-- 3. LS 1 (Tester001)
-- ========================================
DECLARE @StationId3 UNIQUEIDENTIFIER = '0DF598BC-4270-4282-BC15-266FC6DC1EAB';
DECLARE @ChargingPointId3 UNIQUEIDENTIFIER = NEWID();
DECLARE @ConnectorId3 UNIQUEIDENTIFIER = NEWID();

IF NOT EXISTS (SELECT 1 FROM ChargingPoints WHERE ChargingStationId = @StationId3)
BEGIN
    PRINT '3. Lege ChargingPoint für LS 1 (Tester001) an...';
    
    -- ChargingPoint anlegen
    INSERT INTO ChargingPoints (
        Id, ChargingStationId, EvseId, Name, Description,
        MaxPower, Status, SupportsSmartCharging, SupportsRemoteStartStop,
        SupportsReservation, IsActive, CreatedAt
    )
    VALUES (
        @ChargingPointId3,
        @StationId3,
        1, -- EvseId = 1 (OCPP ConnectorId)
        'EVSE 1',
        'Ladepunkt 1',
        22, -- 22 kW
        0, -- Available
        0, -- SupportsSmartCharging = false
        1, -- SupportsRemoteStartStop = true
        0, -- SupportsReservation = false
        1, -- IsActive = true
        GETUTCDATE()
    );
    
    -- Connector anlegen (Type2, 22kW AC)
    INSERT INTO ChargingConnectors (
        Id, ChargingPointId, ConnectorId, ConnectorType, ConnectorFormat,
        PowerType, MaxPower, MaxCurrent, MaxVoltage, Status,
        PhysicalReference, IsActive, CreatedAt
    )
    VALUES (
        @ConnectorId3,
        @ChargingPointId3,
        1, -- ConnectorId innerhalb des ChargingPoints
        'Type2',
        'SOCKET',
        'AC_3_PHASE',
        22, -- 22 kW
        32, -- 32 A
        400, -- 400 V
        0, -- Available
        'Connector 1',
        1, -- IsActive = true
        GETUTCDATE()
    );
    
    PRINT '   ✓ ChargingPoint und Connector für LS 1 (Tester001) angelegt';
END
ELSE
BEGIN
    PRINT '   → LS 1 (Tester001) hat bereits ChargingPoints';
END

PRINT '';
PRINT '=== FIX ABGESCHLOSSEN ===';
PRINT '';
PRINT 'Überprüfe die Änderungen...';
PRINT '';

-- Zeige Zusammenfassung
SELECT 
    cs.ChargeBoxId,
    cs.Name as StationName,
    COUNT(cp.Id) as AnzahlChargingPoints,
    COUNT(c.Id) as AnzahlConnectors
FROM ChargingStations cs
LEFT JOIN ChargingPoints cp ON cs.Id = cp.ChargingStationId
LEFT JOIN ChargingConnectors c ON cp.Id = c.ChargingPointId
GROUP BY cs.ChargeBoxId, cs.Name
ORDER BY cs.Name;

PRINT '';
PRINT 'Committe Transaktion...';

COMMIT;

PRINT '✓ Änderungen erfolgreich gespeichert!';

