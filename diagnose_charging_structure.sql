-- ========================================
-- DIAGNOSE: ChargingPoints und Connectors überprüfen
-- ========================================

PRINT '=== 1. Übersicht aller ChargingStations ===';
SELECT 
    cs.Id as StationId,
    cs.ChargeBoxId,
    cs.Name as StationName,
    cs.Status,
    cs.ChargingParkId,
    CASE WHEN cp.Id IS NOT NULL THEN 'Ja' ELSE 'NEIN - FEHLT!' END as HasChargingPark
FROM ChargingStations cs
LEFT JOIN ChargingParks cp ON cs.ChargingParkId = cp.Id
ORDER BY cs.Name;

PRINT '';
PRINT '=== 2. ChargingPoints pro Station ===';
SELECT 
    cs.ChargeBoxId,
    cs.Name as StationName,
    COUNT(cp.Id) as AnzahlChargingPoints,
    STRING_AGG(CAST(cp.EvseId AS VARCHAR), ', ') as EvseIds
FROM ChargingStations cs
LEFT JOIN ChargingPoints cp ON cs.Id = cp.ChargingStationId
GROUP BY cs.ChargeBoxId, cs.Name
ORDER BY cs.Name;

PRINT '';
PRINT '=== 3. Detaillierte ChargingPoints Struktur ===';
SELECT 
    cs.ChargeBoxId,
    cs.Name as StationName,
    cp.Id as ChargingPointId,
    cp.EvseId,
    cp.Status as ChargingPointStatus,
    COUNT(c.Id) as AnzahlConnectors
FROM ChargingStations cs
LEFT JOIN ChargingPoints cp ON cs.Id = cp.ChargingStationId
LEFT JOIN ChargingConnectors c ON cp.Id = c.ChargingPointId
GROUP BY cs.ChargeBoxId, cs.Name, cp.Id, cp.EvseId, cp.Status
ORDER BY cs.Name, cp.EvseId;

PRINT '';
PRINT '=== 4. Detaillierte Connectors ===';
SELECT 
    cs.ChargeBoxId,
    cs.Name as StationName,
    cp.EvseId,
    c.Id as ConnectorId,
    c.ConnectorType,
    c.Status as ConnectorStatus,
    c.MaxPower,
    c.ConnectorFormat
FROM ChargingStations cs
LEFT JOIN ChargingPoints cp ON cs.Id = cp.ChargingStationId
LEFT JOIN ChargingConnectors c ON cp.Id = c.ChargingPointId
ORDER BY cs.Name, cp.EvseId;

PRINT '';
PRINT '=== 5. Stationen OHNE ChargingPoints (PROBLEM!) ===';
SELECT 
    cs.Id as StationId,
    cs.ChargeBoxId,
    cs.Name as StationName,
    cs.Status,
    'FEHLT: ChargingPoints müssen angelegt werden!' as Problem
FROM ChargingStations cs
WHERE NOT EXISTS (
    SELECT 1 FROM ChargingPoints cp WHERE cp.ChargingStationId = cs.Id
)
ORDER BY cs.Name;

PRINT '';
PRINT '=== 6. ChargingPoints OHNE Connectors (PROBLEM!) ===';
SELECT 
    cs.ChargeBoxId,
    cs.Name as StationName,
    cp.Id as ChargingPointId,
    cp.EvseId,
    'FEHLT: Connectors müssen angelegt werden!' as Problem
FROM ChargingStations cs
JOIN ChargingPoints cp ON cs.Id = cp.ChargingStationId
WHERE NOT EXISTS (
    SELECT 1 FROM ChargingConnectors c WHERE c.ChargingPointId = cp.Id
)
ORDER BY cs.Name, cp.EvseId;

PRINT '';
PRINT '=== 7. Stationen OHNE ChargingPark (PROBLEM!) ===';
SELECT 
    cs.Id as StationId,
    cs.ChargeBoxId,
    cs.Name as StationName,
    cs.ChargingParkId,
    'FEHLT: ChargingPark muss verknüpft werden!' as Problem
FROM ChargingStations cs
WHERE cs.ChargingParkId IS NULL
    OR NOT EXISTS (SELECT 1 FROM ChargingParks cp WHERE cp.Id = cs.ChargingParkId)
ORDER BY cs.Name;

PRINT '';
PRINT '=== 8. User-Berechtigungen prüfen (RFID) ===';
SELECT 
    am.Identifier as RFID,
    u.Email as UserEmail,
    u.IsActive as UserActive,
    am.IsActive as RFIDActive,
    COUNT(DISTINCT ugm.UserGroupId) as AnzahlUserGroups
FROM AuthorizationMethods am
JOIN Users u ON am.UserId = u.Id
LEFT JOIN UserGroupMemberships ugm ON u.Id = ugm.UserId
GROUP BY am.Identifier, u.Email, u.IsActive, am.IsActive
ORDER BY am.Identifier;

PRINT '';
PRINT '=== DIAGNOSE ABGESCHLOSSEN ===';

