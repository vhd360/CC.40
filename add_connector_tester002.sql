-- ===================================================================
-- Connector 1 für Station Tester002 hinzufügen
-- ===================================================================

-- Connector 1 erstellen
INSERT INTO "ChargingConnectors" (
    "Id", 
    "ChargingStationId", 
    "ConnectorId", 
    "ConnectorType",
    "MaxPower", 
    "MaxCurrent", 
    "MaxVoltage", 
    "Status", 
    "CreatedAt"
)
VALUES (
    gen_random_uuid(),
    '0201587c-1664-454e-b19a-effbf18e00c6', -- Ihre Station-ID aus den Logs
    1,                                       -- ConnectorId = 1
    'Type2',                                 -- Standard Type 2 Stecker
    22,                                      -- 22 kW
    32,                                      -- 32 Ampere
    230,                                     -- 230 Volt
    0,                                       -- Status: Available
    NOW()
);

-- Überprüfung
SELECT 
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
WHERE s."Id" = '0201587c-1664-454e-b19a-effbf18e00c6';

SELECT '✅ Connector 1 erstellt!' AS "Status";

