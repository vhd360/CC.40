-- ========================================
-- DIAGNOSE: Warum werden keine Connectors angezeigt?
-- ========================================

PRINT '=== 1. Alle Ladestationen ===';
SELECT 
    Id,
    Name,
    StationId,
    ChargeBoxId,
    Status,
    IsActive,
    ChargingParkId
FROM ChargingStations
WHERE IsActive = 1
ORDER BY Name;

PRINT '';
PRINT '=== 2. ChargingPoints pro Station ===';
SELECT 
    cs.Name AS StationName,
    cs.Id AS StationId,
    cp.Id AS ChargingPointId,
    cp.EvseId,
    cp.Name AS PointName,
    cp.Status AS PointStatus,
    cp.IsActive AS PointIsActive,
    cp.MaxPower
FROM ChargingStations cs
LEFT JOIN ChargingPoints cp ON cs.Id = cp.ChargingStationId
WHERE cs.IsActive = 1
ORDER BY cs.Name, cp.EvseId;

PRINT '';
PRINT '=== 3. Connectors pro ChargingPoint ===';
SELECT 
    cs.Name AS StationName,
    cs.Id AS StationId,
    cp.EvseId,
    cp.Name AS PointName,
    cc.Id AS ConnectorId,
    cc.ConnectorId AS ConnectorNumber,
    cc.ConnectorType,
    cc.Status AS ConnectorStatus,
    cc.IsActive AS ConnectorIsActive,
    cc.MaxPower,
    CASE 
        WHEN cc.Status = 0 AND cc.IsActive = 1 AND cp.IsActive = 1 THEN 'JA - VERFÜGBAR'
        WHEN cc.Status != 0 THEN 'NEIN - Status: ' + CAST(cc.Status AS VARCHAR)
        WHEN cc.IsActive = 0 THEN 'NEIN - Connector deaktiviert'
        WHEN cp.IsActive = 0 THEN 'NEIN - ChargingPoint deaktiviert'
        ELSE 'NEIN - Unbekannt'
    END AS IstVerfuegbar
FROM ChargingStations cs
LEFT JOIN ChargingPoints cp ON cs.Id = cp.ChargingStationId
LEFT JOIN ChargingConnectors cc ON cp.Id = cc.ChargingPointId
WHERE cs.IsActive = 1
ORDER BY cs.Name, cp.EvseId, cc.ConnectorId;

PRINT '';
PRINT '=== 4. Stationen OHNE ChargingPoints (PROBLEM!) ===';
SELECT 
    cs.Id,
    cs.Name,
    cs.StationId,
    '❌ KEINE CHARGINGPOINTS VORHANDEN!' AS Problem,
    'Bitte ChargingPoints anlegen in der Ladestation-Detailansicht' AS Lösung
FROM ChargingStations cs
WHERE cs.IsActive = 1
  AND NOT EXISTS (SELECT 1 FROM ChargingPoints cp WHERE cp.ChargingStationId = cs.Id)
ORDER BY cs.Name;

PRINT '';
PRINT '=== 5. ChargingPoints OHNE Connectors (PROBLEM!) ===';
SELECT 
    cs.Name AS StationName,
    cp.Id AS ChargingPointId,
    cp.EvseId,
    cp.Name AS PointName,
    '❌ KEINE CONNECTORS VORHANDEN!' AS Problem,
    'Bitte Stecker zu diesem ChargingPoint hinzufügen' AS Lösung
FROM ChargingStations cs
JOIN ChargingPoints cp ON cs.Id = cp.ChargingStationId
WHERE cs.IsActive = 1
  AND cp.IsActive = 1
  AND NOT EXISTS (SELECT 1 FROM ChargingConnectors cc WHERE cc.ChargingPointId = cp.Id)
ORDER BY cs.Name, cp.EvseId;

PRINT '';
PRINT '=== 6. Deaktivierte Connectors ===';
SELECT 
    cs.Name AS StationName,
    cp.EvseId,
    cp.Name AS PointName,
    cc.ConnectorId,
    cc.ConnectorType,
    cc.IsActive AS ConnectorIsActive,
    cp.IsActive AS PointIsActive,
    '⚠️ DEAKTIVIERT - Muss reaktiviert werden!' AS Problem
FROM ChargingStations cs
JOIN ChargingPoints cp ON cs.Id = cp.ChargingStationId
JOIN ChargingConnectors cc ON cp.Id = cc.ChargingPointId
WHERE cs.IsActive = 1
  AND (cc.IsActive = 0 OR cp.IsActive = 0)
ORDER BY cs.Name, cp.EvseId, cc.ConnectorId;

PRINT '';
PRINT '=== 7. Connectors mit Status != Available ===';
SELECT 
    cs.Name AS StationName,
    cs.Id AS StationId,
    cp.EvseId,
    cc.ConnectorId,
    cc.ConnectorType,
    CASE cc.Status
        WHEN 0 THEN 'Available (OK)'
        WHEN 1 THEN 'Occupied (Belegt)'
        WHEN 2 THEN 'Faulted (Defekt)'
        WHEN 3 THEN 'Unavailable (Nicht verfügbar)'
        WHEN 4 THEN 'Reserved (Reserviert)'
        ELSE 'Unbekannt (' + CAST(cc.Status AS VARCHAR) + ')'
    END AS Status,
    '⚠️ Status muss auf Available (0) gesetzt werden!' AS Problem
FROM ChargingStations cs
JOIN ChargingPoints cp ON cs.Id = cp.ChargingStationId
JOIN ChargingConnectors cc ON cp.Id = cc.ChargingPointId
WHERE cs.IsActive = 1
  AND cc.IsActive = 1
  AND cp.IsActive = 1
  AND cc.Status != 0
ORDER BY cs.Name, cp.EvseId, cc.ConnectorId;

PRINT '';
PRINT '=== 8. Zusammenfassung ===';
SELECT 
    'Anzahl Stationen (aktiv)' AS Metrik,
    COUNT(*) AS Wert
FROM ChargingStations
WHERE IsActive = 1

UNION ALL

SELECT 
    'Anzahl ChargingPoints (aktiv)' AS Metrik,
    COUNT(*) AS Wert
FROM ChargingPoints cp
JOIN ChargingStations cs ON cp.ChargingStationId = cs.Id
WHERE cp.IsActive = 1 AND cs.IsActive = 1

UNION ALL

SELECT 
    'Anzahl Connectors (aktiv)' AS Metrik,
    COUNT(*) AS Wert
FROM ChargingConnectors cc
JOIN ChargingPoints cp ON cc.ChargingPointId = cp.Id
JOIN ChargingStations cs ON cp.ChargingStationId = cs.Id
WHERE cc.IsActive = 1 AND cp.IsActive = 1 AND cs.IsActive = 1

UNION ALL

SELECT 
    'Anzahl Connectors VERFÜGBAR (Status=0)' AS Metrik,
    COUNT(*) AS Wert
FROM ChargingConnectors cc
JOIN ChargingPoints cp ON cc.ChargingPointId = cp.Id
JOIN ChargingStations cs ON cp.ChargingStationId = cs.Id
WHERE cc.IsActive = 1 
  AND cp.IsActive = 1 
  AND cs.IsActive = 1
  AND cc.Status = 0;

PRINT '';
PRINT '=== DIAGNOSE ABGESCHLOSSEN ===';
PRINT 'Wenn "Anzahl Connectors VERFÜGBAR" = 0 ist, dann:';
PRINT '1. Legen Sie ChargingPoints an (Ladestation → Details → "Ladepunkt hinzufügen")';
PRINT '2. Fügen Sie Stecker hinzu (ChargingPoint → "Stecker hinzufügen")';
PRINT '3. Prüfen Sie, dass Status = Available (0) ist';
PRINT '4. Prüfen Sie, dass IsActive = 1 ist';


