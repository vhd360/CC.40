-- ========================================
-- PRÜFUNG: Warum gibt GetStationConnectors leeres Array zurück?
-- ========================================

DECLARE @StationId UNIQUEIDENTIFIER = 'c80e8787-5a4e-418c-8f2d-59ec46ebe700';
DECLARE @ChargingPointId UNIQUEIDENTIFIER = '8453dcc9-c76d-41c5-aff2-cfe4d15a92c6';

PRINT '=== 1. Station-Informationen ===';
SELECT 
    Id,
    Name,
    StationId,
    ChargeBoxId,
    IsActive
FROM ChargingStations
WHERE Id = @StationId;

PRINT '';
PRINT '=== 2. ChargingPoint-Informationen ===';
SELECT 
    cp.Id AS ChargingPointId,
    cp.ChargingStationId,
    cp.EvseId,
    cp.Name AS PointName,
    cp.IsActive AS PointIsActive,
    cs.Id AS StationIdAusStation,
    cs.Name AS StationName,
    CASE 
        WHEN cp.ChargingStationId = @StationId THEN '✅ RICHTIG ZUGEORDNET'
        ELSE '❌ FALSCH ZUGEORDNET! Erwartet: ' + CAST(@StationId AS VARCHAR(50)) + 
             ' Tatsächlich: ' + CAST(cp.ChargingStationId AS VARCHAR(50))
    END AS Zuordnung
FROM ChargingPoints cp
LEFT JOIN ChargingStations cs ON cp.ChargingStationId = cs.Id
WHERE cp.Id = @ChargingPointId;

PRINT '';
PRINT '=== 3. Connector-Informationen ===';
SELECT 
    cc.Id AS ConnectorId,
    cc.ChargingPointId,
    cc.ConnectorId AS ConnectorNumber,
    cc.ConnectorType,
    cc.Status,
    cc.IsActive AS ConnectorIsActive,
    cp.ChargingStationId,
    CASE 
        WHEN cp.ChargingStationId = @StationId THEN '✅ GEHÖRT ZUR STATION'
        ELSE '❌ GEHÖRT NICHT ZUR STATION!'
    END AS GehoertZurStation
FROM ChargingConnectors cc
JOIN ChargingPoints cp ON cc.ChargingPointId = cp.Id
WHERE cc.ChargingPointId = @ChargingPointId;

PRINT '';
PRINT '=== 4. Was würde GetStationConnectors zurückgeben? ===';
SELECT 
    cc.Id AS ConnectorId,
    cc.ConnectorId AS ConnectorNumber,
    cp.EvseId,
    cp.Name AS PointName,
    cc.ConnectorType AS Type,
    cc.Status,
    cc.IsActive AS ConnectorIsActive,
    cp.IsActive AS PointIsActive,
    cp.ChargingStationId,
    CASE 
        WHEN cp.ChargingStationId = @StationId AND cc.IsActive = 1 AND cp.IsActive = 1 
        THEN '✅ WÜRDE ZURÜCKGEgeben'
        ELSE '❌ WÜRDE NICHT ZURÜCKGEgeben'
    END AS WuerdeZurueckgegeben
FROM ChargingConnectors cc
JOIN ChargingPoints cp ON cc.ChargingPointId = cp.Id
WHERE cp.ChargingStationId = @StationId;

PRINT '';
PRINT '=== 5. Alle ChargingPoints dieser Station ===';
SELECT 
    cp.Id AS ChargingPointId,
    cp.ChargingStationId,
    cp.EvseId,
    cp.Name,
    cp.IsActive,
    COUNT(cc.Id) AS AnzahlConnectors
FROM ChargingPoints cp
LEFT JOIN ChargingConnectors cc ON cp.Id = cc.ChargingPointId
WHERE cp.ChargingStationId = @StationId
GROUP BY cp.Id, cp.ChargingStationId, cp.EvseId, cp.Name, cp.IsActive;

PRINT '';
PRINT '=== DIAGNOSE ABGESCHLOSSEN ===';


