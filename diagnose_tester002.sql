-- ===================================================================
-- DIAGNOSE für Tester002 - Warum schlägt StartTransaction fehl?
-- ===================================================================

-- Test 1: ✅ Ladestation existiert?
SELECT 
    '1️⃣ Ladestation' AS "Test",
    CASE WHEN COUNT(*) > 0 THEN '✅ OK' ELSE '❌ FEHLT' END AS "Status",
    COUNT(*) AS "Anzahl"
FROM "ChargingStations"
WHERE "ChargeBoxId" = 'Tester002';

-- Details:
SELECT * FROM "ChargingStations" WHERE "ChargeBoxId" = 'Tester002';


-- Test 2: ✅ Connector(en) existieren?
SELECT 
    '2️⃣ Connectors' AS "Test",
    CASE WHEN COUNT(*) > 0 THEN '✅ OK' ELSE '❌ FEHLT' END AS "Status",
    COUNT(*) AS "Anzahl"
FROM "ChargingConnectors" c
JOIN "ChargingStations" s ON c."ChargingStationId" = s."Id"
WHERE s."ChargeBoxId" = 'Tester002';

-- Details:
SELECT 
    c."ConnectorId",
    c."ConnectorType",
    c."MaxPower",
    c."Status"
FROM "ChargingConnectors" c
JOIN "ChargingStations" s ON c."ChargingStationId" = s."Id"
WHERE s."ChargeBoxId" = 'Tester002';


-- Test 3: ✅ RFID-Karte existiert?
SELECT 
    '3️⃣ RFID-Karte' AS "Test",
    CASE WHEN COUNT(*) > 0 THEN '✅ OK' ELSE '❌ FEHLT' END AS "Status",
    COUNT(*) AS "Anzahl"
FROM "AuthorizationMethods"
WHERE "Identifier" = '1234ABCD12' AND "IsActive" = true;

-- Details:
SELECT 
    am."Identifier",
    am."Type",
    am."IsActive",
    u."FirstName" || ' ' || u."LastName" AS "User",
    u."Email",
    u."IsActive" AS "UserActive"
FROM "AuthorizationMethods" am
JOIN "Users" u ON am."UserId" = u."Id"
WHERE am."Identifier" = '1234ABCD12';


-- Test 4: ✅ User ist in einer UserGroup?
SELECT 
    '4️⃣ User in UserGroup' AS "Test",
    CASE WHEN COUNT(*) > 0 THEN '✅ OK' ELSE '❌ FEHLT' END AS "Status",
    COUNT(*) AS "Anzahl"
FROM "UserGroupMemberships" ugm
WHERE ugm."UserId" IN (
    SELECT "UserId" FROM "AuthorizationMethods" WHERE "Identifier" = '1234ABCD12'
);

-- Details:
SELECT 
    u."FirstName" || ' ' || u."LastName" AS "User",
    ug."Name" AS "UserGroup"
FROM "Users" u
JOIN "AuthorizationMethods" am ON u."Id" = am."UserId"
LEFT JOIN "UserGroupMemberships" ugm ON u."Id" = ugm."UserId"
LEFT JOIN "UserGroups" ug ON ugm."UserGroupId" = ug."Id"
WHERE am."Identifier" = '1234ABCD12';


-- Test 5: ✅ Ladestation ist in einer ChargingStationGroup?
SELECT 
    '5️⃣ Station in StationGroup' AS "Test",
    CASE WHEN COUNT(*) > 0 THEN '✅ OK' ELSE '❌ FEHLT' END AS "Status",
    COUNT(*) AS "Anzahl"
FROM "ChargingStationGroupMemberships" csgm
JOIN "ChargingStations" cs ON csgm."ChargingStationId" = cs."Id"
WHERE cs."ChargeBoxId" = 'Tester002';

-- Details:
SELECT 
    cs."Name" AS "Station",
    csg."Name" AS "StationGroup"
FROM "ChargingStations" cs
LEFT JOIN "ChargingStationGroupMemberships" csgm ON cs."Id" = csgm."ChargingStationId"
LEFT JOIN "ChargingStationGroups" csg ON csgm."ChargingStationGroupId" = csg."Id"
WHERE cs."ChargeBoxId" = 'Tester002';


-- Test 6: 🎯 UserGroup hat Berechtigung für ChargingStationGroup?
SELECT 
    '6️⃣ Permission UserGroup→StationGroup' AS "Test",
    CASE WHEN COUNT(*) > 0 THEN '✅ OK' ELSE '❌ FEHLT' END AS "Status",
    COUNT(*) AS "Anzahl"
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

-- Details:
SELECT 
    u."FirstName" || ' ' || u."LastName" AS "User",
    ug."Name" AS "UserGroup",
    csg."Name" AS "StationGroup",
    cs."Name" AS "Station"
FROM "AuthorizationMethods" am
JOIN "Users" u ON am."UserId" = u."Id"
JOIN "UserGroupMemberships" ugm ON u."Id" = ugm."UserId"
JOIN "UserGroups" ug ON ugm."UserGroupId" = ug."Id"
LEFT JOIN "UserGroupChargingStationGroupPermissions" p ON ug."Id" = p."UserGroupId"
LEFT JOIN "ChargingStationGroups" csg ON p."ChargingStationGroupId" = csg."Id"
LEFT JOIN "ChargingStationGroupMemberships" csgm ON csg."Id" = csgm."ChargingStationGroupId"
LEFT JOIN "ChargingStations" cs ON csgm."ChargingStationId" = cs."Id"
WHERE am."Identifier" = '1234ABCD12';


-- ===================================================================
-- 📊 ZUSAMMENFASSUNG
-- ===================================================================
SELECT 
    '📊 CHECKLISTE' AS "Info",
    (SELECT CASE WHEN COUNT(*) > 0 THEN '✅' ELSE '❌' END FROM "ChargingStations" WHERE "ChargeBoxId" = 'Tester002') AS "1_Station",
    (SELECT CASE WHEN COUNT(*) > 0 THEN '✅' ELSE '❌' END FROM "ChargingConnectors" c JOIN "ChargingStations" s ON c."ChargingStationId" = s."Id" WHERE s."ChargeBoxId" = 'Tester002') AS "2_Connectors",
    (SELECT CASE WHEN COUNT(*) > 0 THEN '✅' ELSE '❌' END FROM "AuthorizationMethods" WHERE "Identifier" = '1234ABCD12') AS "3_RFID",
    (SELECT CASE WHEN COUNT(*) > 0 THEN '✅' ELSE '❌' END FROM "UserGroupMemberships" WHERE "UserId" IN (SELECT "UserId" FROM "AuthorizationMethods" WHERE "Identifier" = '1234ABCD12')) AS "4_UserGroup",
    (SELECT CASE WHEN COUNT(*) > 0 THEN '✅' ELSE '❌' END FROM "ChargingStationGroupMemberships" csgm JOIN "ChargingStations" cs ON csgm."ChargingStationId" = cs."Id" WHERE cs."ChargeBoxId" = 'Tester002') AS "5_StationGroup",
    (SELECT CASE WHEN COUNT(*) > 0 THEN '✅' ELSE '❌' END 
     FROM "AuthorizationMethods" am
     JOIN "Users" u ON am."UserId" = u."Id"
     JOIN "UserGroupMemberships" ugm ON u."Id" = ugm."UserId"
     JOIN "UserGroupChargingStationGroupPermissions" p ON ugm."UserGroupId" = p."UserGroupId"
     JOIN "ChargingStationGroupMemberships" csgm ON p."ChargingStationGroupId" = csgm."ChargingStationGroupId"
     JOIN "ChargingStations" cs ON csgm."ChargingStationId" = cs."Id"
     WHERE am."Identifier" = '1234ABCD12' AND cs."ChargeBoxId" = 'Tester002') AS "6_Permission";

-- Wenn ALLE Spalten ✅ sind: System sollte funktionieren!
-- Wenn ❌ irgendwo: Dort liegt das Problem!

