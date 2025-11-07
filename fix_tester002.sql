-- ===================================================================
-- ChargeBoxId auf "Tester002" setzen
-- ===================================================================

-- 1. Zuerst prüfen: Welche Ladestation haben Sie angelegt?
SELECT 
    "Id",
    "Name",
    "StationId",
    "ChargeBoxId",
    "Vendor",
    "Model",
    "IsActive",
    "CreatedAt"
FROM "ChargingStations"
ORDER BY "CreatedAt" DESC
LIMIT 5;

-- 2. ChargeBoxId auf "Tester002" setzen
-- ANPASSEN: Ersetzen Sie die WHERE-Bedingung mit Ihrer Ladestation!

-- Option A: Nach Name
-- UPDATE "ChargingStations" 
-- SET "ChargeBoxId" = 'Tester002'
-- WHERE "Name" = 'Name Ihrer Ladestation';

-- Option B: Nach StationId
-- UPDATE "ChargingStations" 
-- SET "ChargeBoxId" = 'Tester002'
-- WHERE "StationId" = 'CP001';  -- Ihre StationId

-- Option C: Nach Datenbank-ID (am sichersten!)
-- UPDATE "ChargingStations" 
-- SET "ChargeBoxId" = 'Tester002'
-- WHERE "Id" = 'ihre-guid-hier';

-- Option D: Wenn Sie nur eine Station haben (automatisch die neueste)
UPDATE "ChargingStations" 
SET "ChargeBoxId" = 'Tester002'
WHERE "Id" = (
    SELECT "Id" FROM "ChargingStations" 
    ORDER BY "CreatedAt" DESC 
    LIMIT 1
);

-- 3. Überprüfung
SELECT 
    "Id",
    "Name",
    "StationId",
    "ChargeBoxId",
    "Vendor",
    "Model",
    "Status",
    "IsActive"
FROM "ChargingStations"
WHERE "ChargeBoxId" = 'Tester002';

-- 4. Connector prüfen
SELECT 
    c."Id",
    c."ConnectorId",
    c."ConnectorType",
    c."MaxPower",
    c."MaxCurrent",
    c."MaxVoltage",
    c."Status",
    s."Name" AS "StationName",
    s."ChargeBoxId"
FROM "ChargingConnectors" c
JOIN "ChargingStations" s ON c."ChargingStationId" = s."Id"
WHERE s."ChargeBoxId" = 'Tester002';

-- Falls KEIN Connector existiert, fügen Sie einen hinzu:
-- INSERT INTO "ChargingConnectors" (
--     "Id", "ChargingStationId", "ConnectorId", "ConnectorType", 
--     "MaxPower", "MaxCurrent", "MaxVoltage", "Status", "CreatedAt"
-- )
-- SELECT 
--     gen_random_uuid(),
--     "Id",
--     1,
--     'Type2',
--     22,
--     32,
--     230,
--     0,
--     NOW()
-- FROM "ChargingStations"
-- WHERE "ChargeBoxId" = 'Tester002'
--   AND NOT EXISTS (
--       SELECT 1 FROM "ChargingConnectors" 
--       WHERE "ChargingStationId" = "ChargingStations"."Id"
--   );

