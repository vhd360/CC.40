-- ========================================
-- FIX: Warum werden keine Ladepunkte angezeigt?
-- ========================================

-- 1. Prüfen, welche Stationen existieren
SELECT 
    cs.Id,
    cs.Name,
    cs.StationId,
    cs.IsActive AS StationIsActive,
    COUNT(cp.Id) AS AnzahlChargingPoints,
    SUM(CASE WHEN cp.IsActive = 1 THEN 1 ELSE 0 END) AS AktiveChargingPoints
FROM ChargingStations cs
LEFT JOIN ChargingPoints cp ON cs.Id = cp.ChargingStationId
GROUP BY cs.Id, cs.Name, cs.StationId, cs.IsActive
ORDER BY cs.Name;

-- 2. Prüfen, ob die Station "test" (C22E Gregor) ChargingPoints hat
SELECT 
    cs.Id AS StationId,
    cs.Name AS StationName,
    cs.StationId AS StationCode,
    cp.Id AS ChargingPointId,
    cp.EvseId,
    cp.Name AS PointName,
    cp.Status,
    cp.IsActive,
    cp.ConnectorType,
    cp.MaxPower,
    CASE 
        WHEN cp.Status = 0 AND cp.IsActive = 1 THEN '✅ Verfügbar'
        WHEN cp.Status = 1 THEN '⚠️ Belegt'
        WHEN cp.Status = 2 THEN '⚡ Lädt'
        WHEN cp.Status = 3 THEN '🔒 Reserviert'
        WHEN cp.Status = 4 THEN '❌ Defekt'
        WHEN cp.Status = 5 THEN '🚫 Nicht verfügbar'
        WHEN cp.IsActive = 0 THEN '❌ Inaktiv'
        ELSE '❓ Unbekannt'
    END AS StatusBeschreibung
FROM ChargingStations cs
LEFT JOIN ChargingPoints cp ON cs.Id = cp.ChargingStationId
WHERE cs.Name LIKE '%test%' OR cs.StationId LIKE '%C22E%'
ORDER BY cs.Name, cp.EvseId;

-- 3. Wenn keine ChargingPoints vorhanden sind, erstellen Sie einen:
-- (Führen Sie diesen Teil nur aus, wenn wirklich keine ChargingPoints vorhanden sind!)

-- Beispiel: ChargingPoint für Station "test" erstellen
-- Ersetzen Sie {STATION_ID} mit der tatsächlichen Station-ID aus Schritt 1

/*
INSERT INTO ChargingPoints (
    Id,
    ChargingStationId,
    EvseId,
    Name,
    Description,
    MaxPower,
    Status,
    ConnectorType,
    ConnectorFormat,
    PowerType,
    MaxCurrent,
    MaxVoltage,
    SupportsSmartCharging,
    SupportsRemoteStartStop,
    SupportsReservation,
    IsActive,
    CreatedAt
)
SELECT 
    NEWID() AS Id,
    cs.Id AS ChargingStationId,
    1 AS EvseId,
    'Ladepunkt 1' AS Name,
    NULL AS Description,
    22 AS MaxPower,
    0 AS Status, -- Available
    'Type2' AS ConnectorType,
    NULL AS ConnectorFormat,
    'AC' AS PowerType,
    32 AS MaxCurrent,
    230 AS MaxVoltage,
    0 AS SupportsSmartCharging,
    1 AS SupportsRemoteStartStop,
    0 AS SupportsReservation,
    1 AS IsActive,
    GETUTCDATE() AS CreatedAt
FROM ChargingStations cs
WHERE cs.Name LIKE '%test%' OR cs.StationId LIKE '%C22E%'
AND NOT EXISTS (
    SELECT 1 FROM ChargingPoints cp 
    WHERE cp.ChargingStationId = cs.Id
);
*/

