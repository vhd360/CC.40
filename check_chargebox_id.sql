-- Prüfen Sie, welche ChargeBoxId in der Datenbank eingetragen ist
SELECT 
    "Id",
    "Name",
    "StationId",
    "ChargeBoxId",
    "Vendor",
    "Model",
    "IsActive"
FROM "ChargingStations"
ORDER BY "CreatedAt" DESC;

-- Prüfen Sie auch die Connectors
SELECT 
    c."Id",
    c."ConnectorId",
    c."ConnectorType",
    c."Status",
    s."Name" AS "StationName",
    s."ChargeBoxId"
FROM "ChargingConnectors" c
JOIN "ChargingStations" s ON c."ChargingStationId" = s."Id"
ORDER BY s."Name", c."ConnectorId";

