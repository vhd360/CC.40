-- ===================================================================
-- OCPP Test-Daten für Charging Control System
-- ===================================================================
-- Hinweis: Tenant 11111111-1111-1111-1111-111111111111 existiert bereits
-- durch Seed-Daten in ApplicationDbContext.cs
-- ===================================================================

-- 1. Ladepark erstellen (nutzt existierenden Tenant)
INSERT INTO "ChargingParks" (
    "Id", "TenantId", "Name", "Description", "Address", "PostalCode", 
    "City", "Country", "Latitude", "Longitude", "IsActive", "CreatedAt"
)
VALUES (
    '22222222-2222-2222-2222-222222222222', 
    '11111111-1111-1111-1111-111111111111', 
    'OCPP Test Ladepark', 
    'Testpark für OCPP-Simulator',
    'Teststraße 10', 
    '12345',
    'Teststadt', 
    'Deutschland',
    51.5074, 
    -0.1278, 
    true, 
    NOW()
);

-- 2. Ladestation mit der ChargeBoxId vom OCPP-Simulator erstellen
INSERT INTO "ChargingStations" (
    "Id", "ChargingParkId", "StationId", "Name", "Vendor", "Model", 
    "Type", "MaxPower", "NumberOfConnectors", "Status", 
    "ChargeBoxId", "OcppProtocol", "IsActive", "CreatedAt"
)
VALUES (
    '33333333-3333-3333-3333-333333333333', 
    '22222222-2222-2222-2222-222222222222', 
    'SIM001', 
    'CubosSim Test Station', 
    'CubosSim', 
    'SW1', 
    0, -- AC
    22, 
    1, 
    0, -- Available
    '48e2a994-64d8-4413-8048-bdec57a18094', 
    'OCPP16', 
    true, 
    NOW()
);

-- 3. Connector 1 für die Ladestation erstellen
INSERT INTO "ChargingConnectors" (
    "Id", "ChargingStationId", "ConnectorId", "ConnectorType", 
    "MaxPower", "MaxCurrent", "MaxVoltage", "Status", "CreatedAt"
)
VALUES (
    '44444444-4444-4444-4444-444444444444', 
    '33333333-3333-3333-3333-333333333333', 
    1, 
    'Type2', 
    22, 
    32, 
    230, 
    0, -- Available
    NOW()
);

-- 4. Test-User erstellen
INSERT INTO "Users" (
    "Id", "TenantId", "Email", "FirstName", "LastName", 
    "PasswordHash", "Role", "IsActive", "IsEmailConfirmed", "CreatedAt"
)
VALUES (
    '55555555-5555-5555-5555-555555555555', 
    '11111111-1111-1111-1111-111111111111', 
    'testuser@chargingcontrol.com', 
    'Max', 
    'Mustermann', 
    '$2a$11$QUxBZq94RgWvH09M.ER7EuF6Ju3mP45b9cEXAS99Iz09cAfKEZUeW', -- admin123
    1, -- User
    true, 
    true,
    NOW()
);

-- 5. RFID Authorization Method erstellen (IdTag: 1234ABCD12)
INSERT INTO "AuthorizationMethods" (
    "Id", "UserId", "Type", "Identifier", "FriendlyName", 
    "IsActive", "CreatedAt"
)
VALUES (
    '66666666-6666-6666-6666-666666666666', 
    '55555555-5555-5555-5555-555555555555', 
    0, -- RFID
    '1234ABCD12', 
    'Test RFID Karte',
    true, 
    NOW()
);

-- 6. Charging Station Group erstellen
INSERT INTO "ChargingStationGroups" (
    "Id", "TenantId", "Name", "Description", "IsActive", "CreatedAt"
)
VALUES (
    '77777777-7777-7777-7777-777777777777', 
    '11111111-1111-1111-1111-111111111111', 
    'OCPP Test Gruppe', 
    'Gruppe für alle Test-Ladestationen',
    true,
    NOW()
);

-- 7. Ladestation zur Gruppe hinzufügen
INSERT INTO "ChargingStationGroupMemberships" (
    "Id", "ChargingStationGroupId", "ChargingStationId", "AssignedAt"
)
VALUES (
    'ffffffff-7777-7777-7777-777777777777',
    '77777777-7777-7777-7777-777777777777', 
    '33333333-3333-3333-3333-333333333333', 
    NOW()
);

-- 8. User Group erstellen
INSERT INTO "UserGroups" (
    "Id", "TenantId", "Name", "Description", "IsActive", "CreatedAt"
)
VALUES (
    '88888888-8888-8888-8888-888888888888', 
    '11111111-1111-1111-1111-111111111111', 
    'OCPP Test Benutzer', 
    'Test-Benutzergruppe mit Ladeberechtigung',
    true,
    NOW()
);

-- 9. User zur User Group hinzufügen
INSERT INTO "UserGroupMemberships" (
    "Id", "UserId", "UserGroupId", "AssignedAt"
)
VALUES (
    'ffffffff-8888-8888-8888-888888888888',
    '55555555-5555-5555-5555-555555555555', 
    '88888888-8888-8888-8888-888888888888', 
    NOW()
);

-- 10. Berechtigung: UserGroup -> ChargingStationGroup
INSERT INTO "UserGroupChargingStationGroupPermissions" (
    "Id", "UserGroupId", "ChargingStationGroupId", "GrantedAt"
)
VALUES (
    '99999999-9999-9999-9999-999999999999', 
    '88888888-8888-8888-8888-888888888888', 
    '77777777-7777-7777-7777-777777777777', 
    NOW()
);

-- ===================================================================
-- Fertig! Test-Daten erstellt
-- ===================================================================

-- Übersicht der erstellten Daten:
SELECT '=== OCPP Test-Konfiguration ===' AS "Info";
SELECT 'ChargeBoxId: 48e2a994-64d8-4413-8048-bdec57a18094' AS "Ladestation";
SELECT 'IdTag (RFID): 1234ABCD12' AS "Authorization";
SELECT 'User: Max Mustermann (testuser@chargingcontrol.com)' AS "Benutzer";
SELECT 'Password: admin123' AS "Passwort";

