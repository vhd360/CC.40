-- ===================================================================
-- Berechtigungen für OCPP-Transaktion prüfen
-- ===================================================================

-- 1. Prüfen Sie die AuthorizationMethod (RFID-Karte)
SELECT 
    am."Id",
    am."Identifier" AS "IdTag",
    am."Type",
    am."IsActive" AS "AuthMethod_Active",
    am."FriendlyName",
    u."Id" AS "UserId",
    u."FirstName" || ' ' || u."LastName" AS "UserName",
    u."Email",
    u."IsActive" AS "User_Active"
FROM "AuthorizationMethods" am
JOIN "Users" u ON am."UserId" = u."Id"
WHERE am."Identifier" = '1234ABCD12';

-- 2. Prüfen Sie die User Group Membership
SELECT 
    u."Id" AS "UserId",
    u."FirstName" || ' ' || u."LastName" AS "UserName",
    ug."Id" AS "UserGroupId",
    ug."Name" AS "UserGroupName",
    ug."IsActive" AS "UserGroup_Active"
FROM "Users" u
LEFT JOIN "UserGroupMemberships" ugm ON u."Id" = ugm."UserId"
LEFT JOIN "UserGroups" ug ON ugm."UserGroupId" = ug."Id"
WHERE u."Id" IN (
    SELECT "UserId" FROM "AuthorizationMethods" WHERE "Identifier" = '1234ABCD12'
);

-- 3. Prüfen Sie die Charging Station Group Membership
SELECT 
    cs."Id" AS "StationId",
    cs."Name" AS "StationName",
    cs."ChargeBoxId",
    csg."Id" AS "StationGroupId",
    csg."Name" AS "StationGroupName"
FROM "ChargingStations" cs
LEFT JOIN "ChargingStationGroupMemberships" csgm ON cs."Id" = csgm."ChargingStationId"
LEFT JOIN "ChargingStationGroups" csg ON csgm."ChargingStationGroupId" = csg."Id"
WHERE cs."ChargeBoxId" = '48e2a994-64d8-4413-8048-bdec57a18094';

-- 4. Prüfen Sie die KOMPLETTE Berechtigungskette
-- (UserGroup -> ChargingStationGroup Permission)
SELECT 
    u."FirstName" || ' ' || u."LastName" AS "User",
    am."Identifier" AS "IdTag",
    ug."Name" AS "UserGroup",
    csg."Name" AS "StationGroup",
    cs."Name" AS "Station",
    cs."ChargeBoxId",
    CASE 
        WHEN p."Id" IS NOT NULL THEN '✅ BERECHTIGT'
        ELSE '❌ KEINE BERECHTIGUNG'
    END AS "Status"
FROM "AuthorizationMethods" am
JOIN "Users" u ON am."UserId" = u."Id"
LEFT JOIN "UserGroupMemberships" ugm ON u."Id" = ugm."UserId"
LEFT JOIN "UserGroups" ug ON ugm."UserGroupId" = ug."Id"
LEFT JOIN "UserGroupChargingStationGroupPermissions" p ON ug."Id" = p."UserGroupId"
LEFT JOIN "ChargingStationGroups" csg ON p."ChargingStationGroupId" = csg."Id"
LEFT JOIN "ChargingStationGroupMemberships" csgm ON csg."Id" = csgm."ChargingStationGroupId"
LEFT JOIN "ChargingStations" cs ON csgm."ChargingStationId" = cs."Id"
WHERE am."Identifier" = '1234ABCD12'
  AND cs."ChargeBoxId" = '48e2a994-64d8-4413-8048-bdec57a18094';

-- 5. Prüfen Sie die Connectors
SELECT 
    c."Id",
    c."ConnectorId",
    c."ConnectorType",
    c."MaxPower",
    c."Status",
    cs."Name" AS "StationName",
    cs."ChargeBoxId"
FROM "ChargingConnectors" c
JOIN "ChargingStations" cs ON c."ChargingStationId" = cs."Id"
WHERE cs."ChargeBoxId" = '48e2a994-64d8-4413-8048-bdec57a18094';

