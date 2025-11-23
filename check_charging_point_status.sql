-- ========================================
-- DIAGNOSE: Warum werden keine Ladepunkte angezeigt?
-- ========================================

-- 1. Alle ChargingPoints mit Status prüfen
SELECT 
    cp.Id,
    cp.ChargingStationId,
    cs.Name AS StationName,
    cs.StationId,
    cp.EvseId,
    cp.Name AS PointName,
    cp.Status,
    cp.IsActive,
    cp.ConnectorType,
    cp.MaxPower,
    CASE 
        WHEN cp.Status = 0 AND cp.IsActive = 1 THEN '✅ Verfügbar (Available)'
        WHEN cp.Status = 1 THEN '⚠️ Belegt (Occupied)'
        WHEN cp.Status = 2 THEN '⚡ Lädt (Charging)'
        WHEN cp.Status = 3 THEN '🔒 Reserviert (Reserved)'
        WHEN cp.Status = 4 THEN '❌ Defekt (Faulted)'
        WHEN cp.Status = 5 THEN '🚫 Nicht verfügbar (Unavailable)'
        WHEN cp.Status = 6 THEN '⏳ Vorbereitung (Preparing)'
        WHEN cp.Status = 7 THEN '🏁 Abschluss (Finishing)'
        WHEN cp.IsActive = 0 THEN '❌ Inaktiv'
        ELSE '❓ Unbekannt'
    END AS StatusBeschreibung
FROM ChargingPoints cp
JOIN ChargingStations cs ON cp.ChargingStationId = cs.Id
ORDER BY cs.Name, cp.EvseId;

-- 2. Prüfen, welche Stationen ChargingPoints haben
SELECT 
    cs.Id AS StationId,
    cs.Name AS StationName,
    cs.StationId AS StationCode,
    COUNT(cp.Id) AS AnzahlLadepunkte,
    SUM(CASE WHEN cp.IsActive = 1 THEN 1 ELSE 0 END) AS AktiveLadepunkte,
    SUM(CASE WHEN cp.Status = 0 AND cp.IsActive = 1 THEN 1 ELSE 0 END) AS VerfuegbareLadepunkte
FROM ChargingStations cs
LEFT JOIN ChargingPoints cp ON cs.Id = cp.ChargingStationId
GROUP BY cs.Id, cs.Name, cs.StationId
ORDER BY cs.Name;

-- 3. Prüfen, ob ChargingConnectors noch existieren (sollten nach Migration gelöscht sein)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ChargingConnectors')
BEGIN
    PRINT '⚠️ WARNUNG: ChargingConnectors-Tabelle existiert noch! Migration wurde nicht ausgeführt.';
    SELECT COUNT(*) AS AnzahlConnectors FROM ChargingConnectors;
END
ELSE
BEGIN
    PRINT '✅ OK: ChargingConnectors-Tabelle wurde entfernt (Migration erfolgreich).';
END

-- 4. Prüfen, ob ChargingSessions noch auf ChargingConnectorId verweisen
IF EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'ChargingSessions' 
    AND COLUMN_NAME = 'ChargingConnectorId'
)
BEGIN
    PRINT '⚠️ WARNUNG: ChargingSessions hat noch ChargingConnectorId! Migration wurde nicht ausgeführt.';
END
ELSE IF EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'ChargingSessions' 
    AND COLUMN_NAME = 'ChargingPointId'
)
BEGIN
    PRINT '✅ OK: ChargingSessions verwendet ChargingPointId (Migration erfolgreich).';
END
ELSE
BEGIN
    PRINT '❌ FEHLER: ChargingSessions hat weder ChargingConnectorId noch ChargingPointId!';
END

