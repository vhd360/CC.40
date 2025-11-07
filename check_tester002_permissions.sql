-- ===================================================================
-- Vollständige Berechtigungsprüfung für Tester002 + RFID 1234ABCD12
-- ===================================================================

-- 1. ✅ AuthorizationMethod (RFID-Karte) prüfen
SELECT 
    '1️⃣ RFID-Karte' AS "Check",
    am."Id",
    am."Identifier" AS "IdTag",
    am."Type",
    am."IsActive" AS "Active",
    am."FriendlyName",
    u."FirstName" || ' ' || u."LastName" AS "User",
    u."Email",
    u."IsActive" AS "UserActive"
FROM "AuthorizationMethods" am
JOIN "Users" u ON am."UserId" = u."Id"
WHERE am."Identifier" = '1234ABCD12';

-- Wenn leer: RFID-Karte existiert NICHT!
-- Fix: Legen Sie die RFID-Karte im Frontend an


-- 2. ✅ Ladestation prüfen
SELECT 
    '2️⃣ Ladestation' AS "Check",
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

-- Wenn leer: ChargeBoxId ist FALSCH oder fehlt!
-- Fix: Führen Sie fix_tester002.sql aus


-- 3. ✅ Connectors prüfen
SELECT 
    '3️⃣ Connectors' AS "Check",
    c."ConnectorId",
    c."ConnectorType",
    c."MaxPower",
    c."Status",
    s."Name" AS "Station"
FROM "ChargingConnectors" c
JOIN "ChargingStations" s ON c."ChargingStationId" = s."Id"
WHERE s."ChargeBoxId" = 'Tester002'
ORDER BY c."ConnectorId";

-- Wenn leer: Kein Connector vorhanden!
-- Fix: Siehe fix_tester002.sql (Connector INSERT)


-- 4. ✅ User Group Membership prüfen
SELECT 
    '4️⃣ User Groups' AS "Check",
    u."FirstName" || ' ' || u."LastName" AS "User",
    ug."Name" AS "UserGroup",
    ug."IsActive" AS "GroupActive",
    ugm."AssignedAt"
FROM "Users" u
JOIN "UserGroupMemberships" ugm ON u."Id" = ugm."UserId"
JOIN "UserGroups" ug ON ugm."UserGroupId" = ug."Id"
WHERE u."Id" IN (
    SELECT "UserId" FROM "AuthorizationMethods" WHERE "Identifier" = '1234ABCD12'
);

-- Wenn leer: User ist in KEINER UserGroup!
-- Fix: Weisen Sie den User einer UserGroup zu


-- 5. ✅ Charging Station Group Membership prüfen
SELECT 
    '5️⃣ Station Groups' AS "Check",
    csg."Name" AS "StationGroup",
    cs."Name" AS "Station",
    cs."ChargeBoxId",
    csgm."AssignedAt"
FROM "ChargingStations" cs
JOIN "ChargingStationGroupMemberships" csgm ON cs."Id" = csgm."ChargingStationId"
JOIN "ChargingStationGroups" csg ON csgm."ChargingStationGroupId" = csg."Id"
WHERE cs."ChargeBoxId" = 'Tester002';

-- Wenn leer: Ladestation ist in KEINER StationGroup!
-- Fix: Weisen Sie die Station einer ChargingStationGroup zu


-- 6. 🎯 HAUPTCHECK: Komplette Berechtigungskette
SELECT 
    '🎯 BERECHTIGUNG' AS "Check",
    u."FirstName" || ' ' || u."LastName" AS "User",
    u."Email",
    am."Identifier" AS "RFID_IdTag",
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
JOIN "UserGroupMemberships" ugm ON u."Id" = ugm."UserId"
JOIN "UserGroups" ug ON ugm."UserGroupId" = ug."Id"
JOIN "UserGroupChargingStationGroupPermissions" p ON ug."Id" = p."UserGroupId"
JOIN "ChargingStationGroups" csg ON p."ChargingStationGroupId" = csg."Id"
JOIN "ChargingStationGroupMemberships" csgm ON csg."Id" = csgm."ChargingStationGroupId"
JOIN "ChargingStations" cs ON csgm."ChargingStationId" = cs."Id"
WHERE am."Identifier" = '1234ABCD12'
  AND cs."ChargeBoxId" = 'Tester002';

-- ✅ Wenn hier eine Zeile mit "✅ BERECHTIGT" kommt: ALLES OK!
-- ❌ Wenn leer: Irgendwo fehlt eine Verknüpfung!


-- 7. 📊 Zusammenfassung
SELECT 
    '📊 ZUSAMMENFASSUNG' AS "Info",
    (SELECT COUNT(*) FROM "AuthorizationMethods" WHERE "Identifier" = '1234ABCD12') AS "RFID_Exists",
    (SELECT COUNT(*) FROM "ChargingStations" WHERE "ChargeBoxId" = 'Tester002') AS "Station_Exists",
    (SELECT COUNT(*) FROM "ChargingConnectors" c 
     JOIN "ChargingStations" s ON c."ChargingStationId" = s."Id" 
     WHERE s."ChargeBoxId" = 'Tester002') AS "Connectors_Count",
    (SELECT COUNT(*) FROM "Users" u
     JOIN "AuthorizationMethods" am ON u."Id" = am."UserId"
     JOIN "UserGroupMemberships" ugm ON u."Id" = ugm."UserId"
     WHERE am."Identifier" = '1234ABCD12') AS "User_In_Groups",
    (SELECT COUNT(*) FROM "ChargingStations" cs
     JOIN "ChargingStationGroupMemberships" csgm ON cs."Id" = csgm."ChargingStationId"
     WHERE cs."ChargeBoxId" = 'Tester002') AS "Station_In_Groups";

