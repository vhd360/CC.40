-- ===================================================================
-- ChargeBoxId für Ihre Ladestation korrigieren
-- ===================================================================
-- ANLEITUNG:
-- 1. Führen Sie zuerst "check_chargebox_id.sql" aus, um die aktuelle ID zu sehen
-- 2. Ersetzen Sie unten 'IHRE_LADESTATION_ID' mit der echten Station-ID
-- 3. Die ChargeBoxId wird auf die ID gesetzt, die Ihre Ladestation sendet
-- ===================================================================

-- Option 1: Wenn Sie die Datenbank-ID Ihrer Station kennen
UPDATE "ChargingStations" 
SET "ChargeBoxId" = '48e2a994-64d8-4413-8048-bdec57a18094'
WHERE "Id" = 'IHRE_LADESTATION_ID';  -- <-- ERSETZEN SIE DIES!

-- Option 2: Wenn Sie nach Name suchen
UPDATE "ChargingStations" 
SET "ChargeBoxId" = '48e2a994-64d8-4413-8048-bdec57a18094'
WHERE "Name" = 'Name Ihrer Ladestation';  -- <-- ERSETZEN SIE DIES!

-- Option 3: Wenn Sie nach StationId suchen
UPDATE "ChargingStations" 
SET "ChargeBoxId" = '48e2a994-64d8-4413-8048-bdec57a18094'
WHERE "StationId" = 'CP001';  -- <-- ERSETZEN SIE DIES!

-- Überprüfung
SELECT "Id", "Name", "ChargeBoxId", "IsActive" 
FROM "ChargingStations" 
WHERE "ChargeBoxId" = '48e2a994-64d8-4413-8048-bdec57a18094';

