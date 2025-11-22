-- Migration: Add Private Charging Stations Support
-- Fügt die Spalten IsPrivate und OwnerId zur ChargingStations-Tabelle hinzu

USE ChargingControlSystem;
GO

-- Prüfe, ob die Spalten bereits existieren
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ChargingStations]') AND name = 'IsPrivate')
BEGIN
    PRINT 'Füge Spalte IsPrivate hinzu...'
    
    -- Füge IsPrivate Spalte hinzu
    ALTER TABLE [dbo].[ChargingStations]
    ADD [IsPrivate] BIT NOT NULL DEFAULT 0;
    
    PRINT '✓ Spalte IsPrivate hinzugefügt'
END
ELSE
BEGIN
    PRINT 'Spalte IsPrivate existiert bereits'
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ChargingStations]') AND name = 'OwnerId')
BEGIN
    PRINT 'Füge Spalte OwnerId hinzu...'
    
    -- Füge OwnerId Spalte hinzu
    ALTER TABLE [dbo].[ChargingStations]
    ADD [OwnerId] UNIQUEIDENTIFIER NULL;
    
    PRINT '✓ Spalte OwnerId hinzugefügt'
END
ELSE
BEGIN
    PRINT 'Spalte OwnerId existiert bereits'
END
GO

-- Erstelle Index für OwnerId
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ChargingStations_OwnerId')
BEGIN
    PRINT 'Erstelle Index für OwnerId...'
    
    CREATE NONCLUSTERED INDEX [IX_ChargingStations_OwnerId]
    ON [dbo].[ChargingStations] ([OwnerId]);
    
    PRINT '✓ Index IX_ChargingStations_OwnerId erstellt'
END
ELSE
BEGIN
    PRINT 'Index IX_ChargingStations_OwnerId existiert bereits'
END
GO

-- Erstelle Foreign Key für OwnerId
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ChargingStations_Users_OwnerId')
BEGIN
    PRINT 'Erstelle Foreign Key für OwnerId...'
    
    ALTER TABLE [dbo].[ChargingStations]
    ADD CONSTRAINT [FK_ChargingStations_Users_OwnerId]
    FOREIGN KEY ([OwnerId])
    REFERENCES [dbo].[Users] ([Id])
    ON DELETE NO ACTION;
    
    PRINT '✓ Foreign Key FK_ChargingStations_Users_OwnerId erstellt'
END
ELSE
BEGIN
    PRINT 'Foreign Key FK_ChargingStations_Users_OwnerId existiert bereits'
END
GO

-- Mache ChargingParkId nullable
IF EXISTS (
    SELECT * 
    FROM sys.columns c
    JOIN sys.tables t ON c.object_id = t.object_id
    WHERE t.name = 'ChargingStations' 
    AND c.name = 'ChargingParkId' 
    AND c.is_nullable = 0
)
BEGIN
    PRINT 'Ändere ChargingParkId zu nullable...'
    
    -- Entferne Foreign Key constraint temporär
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ChargingStations_ChargingParks_ChargingParkId')
    BEGIN
        ALTER TABLE [dbo].[ChargingStations]
        DROP CONSTRAINT [FK_ChargingStations_ChargingParks_ChargingParkId];
    END
    
    -- Ändere Spalte zu nullable
    ALTER TABLE [dbo].[ChargingStations]
    ALTER COLUMN [ChargingParkId] UNIQUEIDENTIFIER NULL;
    
    -- Stelle Foreign Key wieder her
    ALTER TABLE [dbo].[ChargingStations]
    ADD CONSTRAINT [FK_ChargingStations_ChargingParks_ChargingParkId]
    FOREIGN KEY ([ChargingParkId])
    REFERENCES [dbo].[ChargingParks] ([Id])
    ON DELETE CASCADE;
    
    PRINT '✓ ChargingParkId ist jetzt nullable'
END
ELSE
BEGIN
    PRINT 'ChargingParkId ist bereits nullable'
END
GO

-- Füge Eintrag zur __EFMigrationsHistory Tabelle hinzu
IF NOT EXISTS (SELECT * FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = '20251122143000_AddPrivateChargingStations')
BEGIN
    PRINT 'Registriere Migration in __EFMigrationsHistory...'
    
    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20251122143000_AddPrivateChargingStations', '8.0.10');
    
    PRINT '✓ Migration registriert'
END
ELSE
BEGIN
    PRINT 'Migration ist bereits registriert'
END
GO

PRINT ''
PRINT '=========================================='
PRINT '  Migration erfolgreich abgeschlossen!'
PRINT '=========================================='
PRINT ''
PRINT 'Die folgenden Änderungen wurden vorgenommen:'
PRINT '  ✓ Spalte IsPrivate (BIT, NOT NULL, DEFAULT 0)'
PRINT '  ✓ Spalte OwnerId (UNIQUEIDENTIFIER, NULL)'
PRINT '  ✓ Index IX_ChargingStations_OwnerId'
PRINT '  ✓ Foreign Key FK_ChargingStations_Users_OwnerId'
PRINT '  ✓ ChargingParkId ist jetzt nullable'
PRINT ''
PRINT 'Sie können jetzt die Anwendung neu starten!'
GO

