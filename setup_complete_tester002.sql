-- ===================================================================
-- KOMPLETTES Setup für Ladestation Tester002 + RFID 1234ABCD12
-- ===================================================================
-- Dieses Skript erstellt ALLE notwendigen Daten, falls sie fehlen
-- ===================================================================

-- WICHTIG: Passen Sie die WHERE-Bedingung für Ihre Ladestation an!
-- Ersetzen Sie 'Name Ihrer Ladestation' mit dem tatsächlichen Namen!

-- Schritt 1: ChargeBoxId setzen
UPDATE "ChargingStations" 
SET "ChargeBoxId" = 'Tester002'
WHERE "Id" = (
    -- Automatisch die neueste Station nehmen
    SELECT "Id" FROM "ChargingStations" 
    ORDER BY "CreatedAt" DESC 
    LIMIT 1
);
-- ODER: Nutzen Sie eine spezifische WHERE-Bedingung wie:
-- WHERE "Name" = 'Ihre Ladestation';
-- WHERE "StationId" = 'CP001';


-- Schritt 2: Connector erstellen (falls nicht vorhanden)
INSERT INTO "ChargingConnectors" (
    "Id", "ChargingStationId", "ConnectorId", "ConnectorType", 
    "MaxPower", "MaxCurrent", "MaxVoltage", "Status", "CreatedAt"
)
SELECT 
    gen_random_uuid(),
    cs."Id",
    1,
    'Type2',
    22,
    32,
    230,
    0, -- Available
    NOW()
FROM "ChargingStations" cs
WHERE cs."ChargeBoxId" = 'Tester002'
  AND NOT EXISTS (
      SELECT 1 FROM "ChargingConnectors" c
      WHERE c."ChargingStationId" = cs."Id" AND c."ConnectorId" = 1
  );


-- Schritt 3: ChargingStationGroup erstellen (falls nicht vorhanden)
INSERT INTO "ChargingStationGroups" (
    "Id", "TenantId", "Name", "Description", "IsActive", "CreatedAt"
)
SELECT 
    gen_random_uuid(),
    cs."TenantId",
    'Alle Ladestationen',
    'Standard-Gruppe für alle Stationen',
    true,
    NOW()
FROM "ChargingStations" cs
WHERE cs."ChargeBoxId" = 'Tester002'
  AND NOT EXISTS (
      SELECT 1 FROM "ChargingStationGroups" csg
      WHERE csg."TenantId" = cs."TenantId" 
        AND csg."Name" = 'Alle Ladestationen'
  )
LIMIT 1;


-- Schritt 4: Station zur ChargingStationGroup hinzufügen
INSERT INTO "ChargingStationGroupMemberships" (
    "Id", "ChargingStationGroupId", "ChargingStationId", "AssignedAt"
)
SELECT 
    gen_random_uuid(),
    csg."Id",
    cs."Id",
    NOW()
FROM "ChargingStations" cs
JOIN "ChargingStationGroups" csg ON csg."TenantId" = cs."TenantId" AND csg."Name" = 'Alle Ladestationen'
WHERE cs."ChargeBoxId" = 'Tester002'
  AND NOT EXISTS (
      SELECT 1 FROM "ChargingStationGroupMemberships" csgm
      WHERE csgm."ChargingStationId" = cs."Id"
        AND csgm."ChargingStationGroupId" = csg."Id"
  );


-- Schritt 5: UserGroup erstellen (falls nicht vorhanden)
INSERT INTO "UserGroups" (
    "Id", "TenantId", "Name", "Description", "IsActive", "CreatedAt"
)
SELECT 
    gen_random_uuid(),
    u."TenantId",
    'Standard Benutzer',
    'Standard-Benutzergruppe mit Ladeberechtigung',
    true,
    NOW()
FROM "Users" u
WHERE u."Id" IN (
    SELECT "UserId" FROM "AuthorizationMethods" WHERE "Identifier" = '1234ABCD12'
)
  AND NOT EXISTS (
      SELECT 1 FROM "UserGroups" ug
      WHERE ug."TenantId" = u."TenantId" 
        AND ug."Name" = 'Standard Benutzer'
  )
LIMIT 1;


-- Schritt 6: User zur UserGroup hinzufügen
INSERT INTO "UserGroupMemberships" (
    "Id", "UserId", "UserGroupId", "AssignedAt"
)
SELECT 
    gen_random_uuid(),
    u."Id",
    ug."Id",
    NOW()
FROM "Users" u
JOIN "AuthorizationMethods" am ON u."Id" = am."UserId"
JOIN "UserGroups" ug ON ug."TenantId" = u."TenantId" AND ug."Name" = 'Standard Benutzer'
WHERE am."Identifier" = '1234ABCD12'
  AND NOT EXISTS (
      SELECT 1 FROM "UserGroupMemberships" ugm
      WHERE ugm."UserId" = u."Id"
        AND ugm."UserGroupId" = ug."Id"
  );


-- Schritt 7: Berechtigung UserGroup -> ChargingStationGroup erstellen
INSERT INTO "UserGroupChargingStationGroupPermissions" (
    "Id", "UserGroupId", "ChargingStationGroupId", "GrantedAt"
)
SELECT 
    gen_random_uuid(),
    ug."Id",
    csg."Id",
    NOW()
FROM "UserGroups" ug
CROSS JOIN "ChargingStationGroups" csg
WHERE ug."Name" = 'Standard Benutzer'
  AND csg."Name" = 'Alle Ladestationen'
  AND ug."TenantId" = csg."TenantId"
  AND NOT EXISTS (
      SELECT 1 FROM "UserGroupChargingStationGroupPermissions" p
      WHERE p."UserGroupId" = ug."Id"
        AND p."ChargingStationGroupId" = csg."Id"
  );


-- ===================================================================
-- ÜBERPRÜFUNG
-- ===================================================================

SELECT '✅ Setup abgeschlossen!' AS "Status";

-- Finale Berechtigungsprüfung
SELECT 
    u."FirstName" || ' ' || u."LastName" AS "User",
    am."Identifier" AS "RFID",
    ug."Name" AS "UserGroup",
    csg."Name" AS "StationGroup",
    cs."Name" AS "Station",
    cs."ChargeBoxId",
    '✅ BERECHTIGT' AS "Status"
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

-- Wenn hier eine Zeile erscheint: ✅ ALLES OK!
-- Wenn leer: ❌ Prüfen Sie die Logs oder führen Sie check_tester002_permissions.sql aus

