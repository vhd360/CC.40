-- ===================================================================
-- Fehlende Berechtigungen für Tester002 automatisch hinzufügen
-- ===================================================================
-- Dieses Skript ergänzt ALLE fehlenden Komponenten
-- ChargeBoxId bleibt unverändert!
-- ===================================================================

-- 1. Connector hinzufügen (falls nicht vorhanden)
DO $$
DECLARE
    v_station_id UUID;
    v_connector_count INT;
BEGIN
    -- Station-ID holen
    SELECT "Id" INTO v_station_id
    FROM "ChargingStations"
    WHERE "ChargeBoxId" = 'Tester002'
    LIMIT 1;
    
    IF v_station_id IS NOT NULL THEN
        -- Prüfen ob Connector 1 existiert
        SELECT COUNT(*) INTO v_connector_count
        FROM "ChargingConnectors"
        WHERE "ChargingStationId" = v_station_id AND "ConnectorId" = 1;
        
        IF v_connector_count = 0 THEN
            INSERT INTO "ChargingConnectors" (
                "Id", "ChargingStationId", "ConnectorId", "ConnectorType",
                "MaxPower", "MaxCurrent", "MaxVoltage", "Status", "CreatedAt"
            ) VALUES (
                gen_random_uuid(),
                v_station_id,
                1,
                'Type2',
                22,
                32,
                230,
                0, -- Available
                NOW()
            );
            RAISE NOTICE '✅ Connector 1 erstellt';
        ELSE
            RAISE NOTICE '✅ Connector 1 existiert bereits';
        END IF;
    END IF;
END $$;


-- 2. ChargingStationGroup erstellen (falls nicht vorhanden)
DO $$
DECLARE
    v_tenant_id UUID;
    v_station_group_id UUID;
BEGIN
    -- TenantId der Station holen
    SELECT "TenantId" INTO v_tenant_id
    FROM "ChargingStations"
    WHERE "ChargeBoxId" = 'Tester002'
    LIMIT 1;
    
    IF v_tenant_id IS NOT NULL THEN
        -- Prüfen ob Gruppe existiert
        SELECT "Id" INTO v_station_group_id
        FROM "ChargingStationGroups"
        WHERE "TenantId" = v_tenant_id AND "Name" = 'Alle Ladestationen'
        LIMIT 1;
        
        IF v_station_group_id IS NULL THEN
            INSERT INTO "ChargingStationGroups" (
                "Id", "TenantId", "Name", "Description", "IsActive", "CreatedAt"
            ) VALUES (
                gen_random_uuid(),
                v_tenant_id,
                'Alle Ladestationen',
                'Standard-Gruppe für alle Stationen',
                true,
                NOW()
            );
            RAISE NOTICE '✅ ChargingStationGroup erstellt';
        ELSE
            RAISE NOTICE '✅ ChargingStationGroup existiert bereits';
        END IF;
    END IF;
END $$;


-- 3. Station zur ChargingStationGroup hinzufügen
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


-- 4. UserGroup erstellen (falls nicht vorhanden)
DO $$
DECLARE
    v_tenant_id UUID;
    v_user_group_id UUID;
BEGIN
    -- TenantId des Users holen (über RFID-Karte)
    SELECT u."TenantId" INTO v_tenant_id
    FROM "Users" u
    JOIN "AuthorizationMethods" am ON u."Id" = am."UserId"
    WHERE am."Identifier" = '1234ABCD12'
    LIMIT 1;
    
    IF v_tenant_id IS NOT NULL THEN
        -- Prüfen ob UserGroup existiert
        SELECT "Id" INTO v_user_group_id
        FROM "UserGroups"
        WHERE "TenantId" = v_tenant_id AND "Name" = 'Standard Benutzer'
        LIMIT 1;
        
        IF v_user_group_id IS NULL THEN
            INSERT INTO "UserGroups" (
                "Id", "TenantId", "Name", "Description", "IsActive", "CreatedAt"
            ) VALUES (
                gen_random_uuid(),
                v_tenant_id,
                'Standard Benutzer',
                'Standard-Benutzergruppe',
                true,
                NOW()
            );
            RAISE NOTICE '✅ UserGroup erstellt';
        ELSE
            RAISE NOTICE '✅ UserGroup existiert bereits';
        END IF;
    END IF;
END $$;


-- 5. User zur UserGroup hinzufügen
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


-- 6. Berechtigung UserGroup -> ChargingStationGroup erstellen
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
-- ✅ FERTIG - ÜBERPRÜFUNG
-- ===================================================================

SELECT '🎉 Setup abgeschlossen!' AS "Status";

-- Finale Prüfung
SELECT 
    '✅ BERECHTIGUNG KOMPLETT' AS "Status",
    u."FirstName" || ' ' || u."LastName" AS "User",
    am."Identifier" AS "RFID",
    ug."Name" AS "UserGroup",
    csg."Name" AS "StationGroup",
    cs."Name" AS "Station",
    cs."ChargeBoxId"
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

