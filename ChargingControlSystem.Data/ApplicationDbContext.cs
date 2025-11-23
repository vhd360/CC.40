using ChargingControlSystem.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChargingControlSystem.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Tenants
    public DbSet<Tenant> Tenants { get; set; }

    // Users and Groups
    public DbSet<User> Users { get; set; }
    public DbSet<UserGroup> UserGroups { get; set; }
    public DbSet<UserGroupMembership> UserGroupMemberships { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<GroupPermission> GroupPermissions { get; set; }

    // Vehicles
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<VehicleAssignment> VehicleAssignments { get; set; }

    // Charging Infrastructure
    public DbSet<ChargingPark> ChargingParks { get; set; }
    public DbSet<ChargingStation> ChargingStations { get; set; }
    public DbSet<ChargingPoint> ChargingPoints { get; set; }
    public DbSet<ChargingStationGroup> ChargingStationGroups { get; set; }
    public DbSet<ChargingStationGroupMembership> ChargingStationGroupMemberships { get; set; }
    public DbSet<UserGroupChargingStationGroupPermission> UserGroupChargingStationGroupPermissions { get; set; }
    public DbSet<ChargingStationDiagnostics> ChargingStationDiagnostics { get; set; }
    public DbSet<ChargingStationFirmwareHistory> ChargingStationFirmwareHistory { get; set; }

    // Charging Sessions
    public DbSet<ChargingSession> ChargingSessions { get; set; }

    // QR Codes
    public DbSet<QrCode> QrCodes { get; set; }

    // Billing
    public DbSet<BillingAccount> BillingAccounts { get; set; }
    public DbSet<BillingTransaction> BillingTransactions { get; set; }

    // Authorization Methods
    public DbSet<AuthorizationMethod> AuthorizationMethods { get; set; }

    // Tariffs
    public DbSet<Tariff> Tariffs { get; set; }
    public DbSet<TariffComponent> TariffComponents { get; set; }
    public DbSet<UserGroupTariff> UserGroupTariffs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Tenant as multi-tenant root
        modelBuilder.Entity<Tenant>()
            .HasIndex(t => t.Subdomain)
            .IsUnique();

        // Configure Tenant hierarchical structure
        modelBuilder.Entity<Tenant>()
            .HasOne(t => t.ParentTenant)
            .WithMany(t => t.SubTenants)
            .HasForeignKey(t => t.ParentTenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure User
        modelBuilder.Entity<User>()
            .HasIndex(u => new { u.TenantId, u.Email })
            .IsUnique();

        // Configure UserGroupMembership (composite key)
        modelBuilder.Entity<UserGroupMembership>()
            .HasIndex(ugm => new { ugm.UserId, ugm.UserGroupId })
            .IsUnique();

        // Configure GroupPermission (composite key)
        modelBuilder.Entity<GroupPermission>()
            .HasIndex(gp => new { gp.UserGroupId, gp.PermissionId })
            .IsUnique();

        // Configure Vehicle
        modelBuilder.Entity<Vehicle>()
            .HasIndex(v => new { v.TenantId, v.LicensePlate })
            .IsUnique();


        // Configure ChargingStation
        modelBuilder.Entity<ChargingStation>()
            .HasIndex(cs => cs.StationId)
            .IsUnique();

        // Configure QrCode
        modelBuilder.Entity<QrCode>()
            .HasIndex(qr => qr.Code)
            .IsUnique();

        // Configure decimal precision for monetary values
        modelBuilder.Entity<ChargingSession>()
            .Property(cs => cs.EnergyDelivered)
            .HasPrecision(10, 3);

        modelBuilder.Entity<ChargingSession>()
            .Property(cs => cs.Cost)
            .HasPrecision(10, 2);

        modelBuilder.Entity<BillingTransaction>()
            .Property(bt => bt.Amount)
            .HasPrecision(10, 2);

        // UserGroupMemberships – keine Mehrfach-Cascades
        modelBuilder.Entity<UserGroupMembership>()
            .HasOne(m => m.User)
            .WithMany(u => u.UserGroupMemberships)
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserGroupMembership>()
            .HasOne(m => m.UserGroup)
            .WithMany(g => g.UserGroupMemberships)
            .HasForeignKey(m => m.UserGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        // GroupPermissions – ebenfalls Restrict
        modelBuilder.Entity<GroupPermission>()
            .HasOne(gp => gp.UserGroup)
            .WithMany(g => g.GroupPermissions)
            .HasForeignKey(gp => gp.UserGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<GroupPermission>()
            .HasOne(gp => gp.Permission)
            .WithMany(p => p.GroupPermissions)
            .HasForeignKey(gp => gp.PermissionId)
            .OnDelete(DeleteBehavior.Restrict);

        // VehicleAssignments – Restrict verhindert Kaskaden-Zyklen
        modelBuilder.Entity<VehicleAssignment>()
            .HasOne(va => va.Vehicle)
            .WithMany(v => v.VehicleAssignments)
            .HasForeignKey(va => va.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<VehicleAssignment>()
            .HasOne(va => va.User)
            .WithMany(u => u.VehicleAssignments)
            .HasForeignKey(va => va.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // ChargingSessions – optionale FKs auf Restrict
        modelBuilder.Entity<ChargingSession>()
            .HasOne(cs => cs.Tenant)
            .WithMany(t => t.ChargingSessions)
            .HasForeignKey(cs => cs.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ChargingSession>()
            .HasOne(cs => cs.ChargingPoint)
            .WithMany(cp => cp.ChargingSessions)
            .HasForeignKey(cs => cs.ChargingPointId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ChargingSession>()
            .HasOne(cs => cs.User)
            .WithMany(u => u.ChargingSessions)
            .HasForeignKey(cs => cs.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ChargingSession>()
            .HasOne(cs => cs.Vehicle)
            .WithMany(v => v.ChargingSessions)
            .HasForeignKey(cs => cs.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ChargingSession>()
            .HasOne(cs => cs.QrCode)
            .WithMany(qr => qr.ChargingSessions)
            .HasForeignKey(cs => cs.QrCodeId)
            .OnDelete(DeleteBehavior.Restrict);         

        modelBuilder.Entity<ChargingPark>()
            .Property(p => p.Latitude).HasPrecision(10, 8);
        modelBuilder.Entity<ChargingPark>()
            .Property(p => p.Longitude).HasPrecision(11, 8);

        modelBuilder.Entity<ChargingStation>()
            .Property(s => s.Latitude).HasPrecision(10, 8);
        modelBuilder.Entity<ChargingStation>()
            .Property(s => s.Longitude).HasPrecision(11, 8);

        // ChargingStationGroups
        modelBuilder.Entity<ChargingStationGroupMembership>()
            .HasOne(m => m.ChargingStationGroup)
            .WithMany(g => g.StationMemberships)
            .HasForeignKey(m => m.ChargingStationGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ChargingStationGroupMembership>()
            .HasOne(m => m.ChargingStation)
            .WithMany()
            .HasForeignKey(m => m.ChargingStationId)
            .OnDelete(DeleteBehavior.Restrict);

        // UserGroupChargingStationGroupPermissions
        modelBuilder.Entity<UserGroupChargingStationGroupPermission>()
            .HasOne(p => p.UserGroup)
            .WithMany(ug => ug.ChargingStationGroupPermissions)
            .HasForeignKey(p => p.UserGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserGroupChargingStationGroupPermission>()
            .HasOne(p => p.ChargingStationGroup)
            .WithMany(csg => csg.UserGroupPermissions)
            .HasForeignKey(p => p.ChargingStationGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        // AuthorizationMethods
        modelBuilder.Entity<AuthorizationMethod>()
            .HasOne(am => am.User)
            .WithMany(u => u.AuthorizationMethods)
            .HasForeignKey(am => am.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // ChargingSession - AuthorizationMethod
        modelBuilder.Entity<ChargingSession>()
            .HasOne(cs => cs.AuthorizationMethod)
            .WithMany(am => am.ChargingSessions)
            .HasForeignKey(cs => cs.AuthorizationMethodId)
            .OnDelete(DeleteBehavior.Restrict);

        // Tariffs - Configure relationships and indexes
        modelBuilder.Entity<Tariff>()
            .HasOne(t => t.Tenant)
            .WithMany()
            .HasForeignKey(t => t.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Tariff>()
            .HasIndex(t => new { t.TenantId, t.Name })
            .IsUnique();

        modelBuilder.Entity<TariffComponent>()
            .HasOne(tc => tc.Tariff)
            .WithMany(t => t.Components)
            .HasForeignKey(tc => tc.TariffId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserGroupTariff>()
            .HasOne(ugt => ugt.UserGroup)
            .WithMany()
            .HasForeignKey(ugt => ugt.UserGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserGroupTariff>()
            .HasOne(ugt => ugt.Tariff)
            .WithMany(t => t.UserGroupTariffs)
            .HasForeignKey(ugt => ugt.TariffId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserGroupTariff>()
            .HasIndex(ugt => new { ugt.UserGroupId, ugt.TariffId })
            .IsUnique();

        // Seed initial permissions
        SeedPermissions(modelBuilder);

        // Seed default tenant for development
        SeedDefaultTenant(modelBuilder);
    }

    private void SeedPermissions(ModelBuilder modelBuilder)
    {
        var permissions = new[]
        {
            // User management - Fixed GUIDs to prevent migration issues
            new Permission { Id = Guid.Parse("10000000-0000-0000-0000-000000000001"), Name = "users.view", Resource = "users", Action = "view", Description = "View users" },
            new Permission { Id = Guid.Parse("10000000-0000-0000-0000-000000000002"), Name = "users.create", Resource = "users", Action = "create", Description = "Create users" },
            new Permission { Id = Guid.Parse("10000000-0000-0000-0000-000000000003"), Name = "users.edit", Resource = "users", Action = "edit", Description = "Edit users" },
            new Permission { Id = Guid.Parse("10000000-0000-0000-0000-000000000004"), Name = "users.delete", Resource = "users", Action = "delete", Description = "Delete users" },

            // Vehicle management
            new Permission { Id = Guid.Parse("10000000-0000-0000-0000-000000000005"), Name = "vehicles.view", Resource = "vehicles", Action = "view", Description = "View vehicles" },
            new Permission { Id = Guid.Parse("10000000-0000-0000-0000-000000000006"), Name = "vehicles.create", Resource = "vehicles", Action = "create", Description = "Create vehicles" },
            new Permission { Id = Guid.Parse("10000000-0000-0000-0000-000000000007"), Name = "vehicles.edit", Resource = "vehicles", Action = "edit", Description = "Edit vehicles" },
            new Permission { Id = Guid.Parse("10000000-0000-0000-0000-000000000008"), Name = "vehicles.assign", Resource = "vehicles", Action = "assign", Description = "Assign vehicles" },

            // Charging infrastructure
            new Permission { Id = Guid.Parse("10000000-0000-0000-0000-000000000009"), Name = "charging.view", Resource = "charging", Action = "view", Description = "View charging infrastructure" },
            new Permission { Id = Guid.Parse("10000000-0000-0000-0000-00000000000A"), Name = "charging.manage", Resource = "charging", Action = "manage", Description = "Manage charging infrastructure" },

            // Billing
            new Permission { Id = Guid.Parse("10000000-0000-0000-0000-00000000000B"), Name = "billing.view", Resource = "billing", Action = "view", Description = "View billing information" },
            new Permission { Id = Guid.Parse("10000000-0000-0000-0000-00000000000C"), Name = "billing.manage", Resource = "billing", Action = "manage", Description = "Manage billing" },

            // QR Codes
            new Permission { Id = Guid.Parse("10000000-0000-0000-0000-00000000000D"), Name = "qrcodes.view", Resource = "qrcodes", Action = "view", Description = "View QR codes" },
            new Permission { Id = Guid.Parse("10000000-0000-0000-0000-00000000000E"), Name = "qrcodes.create", Resource = "qrcodes", Action = "create", Description = "Create QR codes" },
            new Permission { Id = Guid.Parse("10000000-0000-0000-0000-00000000000F"), Name = "qrcodes.manage", Resource = "qrcodes", Action = "manage", Description = "Manage QR codes" },

            // Tariffs
            new Permission { Id = Guid.Parse("10000000-0000-0000-0000-000000000010"), Name = "tariffs.view", Resource = "tariffs", Action = "view", Description = "View tariffs" },
            new Permission { Id = Guid.Parse("10000000-0000-0000-0000-000000000011"), Name = "tariffs.create", Resource = "tariffs", Action = "create", Description = "Create tariffs" },
            new Permission { Id = Guid.Parse("10000000-0000-0000-0000-000000000012"), Name = "tariffs.edit", Resource = "tariffs", Action = "edit", Description = "Edit tariffs" },
            new Permission { Id = Guid.Parse("10000000-0000-0000-0000-000000000013"), Name = "tariffs.delete", Resource = "tariffs", Action = "delete", Description = "Delete tariffs" }
        };

        modelBuilder.Entity<Permission>().HasData(permissions);
    }

    private void SeedDefaultTenant(ModelBuilder modelBuilder)
    {
        // ChargingControl (Root Tenant)
        var defaultTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var adminUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var adminGroupId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var station1Id = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var station2Id = Guid.Parse("55555555-5555-5555-5555-555555555555");
        var vehicle1Id = Guid.Parse("66666666-6666-6666-6666-666666666666");
        var vehicle2Id = Guid.Parse("77777777-7777-7777-7777-777777777777");

        // Acme (Sub-Tenant of ChargingControl)
        var acmeTenantId = Guid.Parse("aaaabbbb-1111-2222-3333-444444444444");
        var acmeAdminUserId = Guid.Parse("aaaabbbb-2222-3333-4444-555555555555");
        var acmeAdminGroupId = Guid.Parse("aaaabbbb-3333-4444-5555-666666666666");

        // Seed Tenants
        modelBuilder.Entity<Tenant>().HasData(
            // Root Tenant: ChargingControl
            new Tenant
            {
                Id = defaultTenantId,
                Name = "ChargingControl GmbH",
                Subdomain = "chargingcontrol",
                Description = "Hauptunternehmen für Lade-Management",
                ParentTenantId = null, // Root Tenant
                Address = "Musterstraße 123",
                PostalCode = "10115",
                City = "Berlin",
                Country = "Deutschland",
                Phone = "+49 30 12345678",
                Email = "info@chargingcontrol.de",
                Website = "https://www.chargingcontrol.de",
                TaxId = "DE123456789",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            // Sub-Tenant: Acme
            new Tenant
            {
                Id = acmeTenantId,
                Name = "Acme GmbH",
                Subdomain = "acme",
                Description = "Sub-Tenant für Acme Corporation",
                ParentTenantId = defaultTenantId, // Sub-Tenant von ChargingControl
                Address = "Acme Straße 456",
                PostalCode = "80331",
                City = "München",
                Country = "Deutschland",
                Phone = "+49 89 98765432",
                Email = "info@acme.de",
                Website = "https://www.acme.de",
                TaxId = "DE987654321",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-25)
            }
        );

        // Seed User Groups
        modelBuilder.Entity<UserGroup>().HasData(
            // ChargingControl Admin Group
            new UserGroup
            {
                Id = adminGroupId,
                TenantId = defaultTenantId,
                Name = "Administratoren",
                Description = "Vollständige Administrator-Rechte",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            // Acme Admin Group
            new UserGroup
            {
                Id = acmeAdminGroupId,
                TenantId = acmeTenantId,
                Name = "Acme Administratoren",
                Description = "Administrator-Rechte für Acme",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-25)
            }
        );

        // Seed Admin Users
        modelBuilder.Entity<User>().HasData(
            // ChargingControl Admin
            new User
            {
                Id = adminUserId,
                TenantId = defaultTenantId,
                FirstName = "Admin",
                LastName = "User",
                Email = "admin@chargingcontrol.com",
                PasswordHash = "$2a$11$QUxBZq94RgWvH09M.ER7EuF6Ju3mP45b9cEXAS99Iz09cAfKEZUeW", // Hash für "admin123"
                Role = Enums.UserRole.TenantAdmin,
                IsActive = true,
                IsEmailConfirmed = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                LastLoginAt = DateTime.UtcNow.AddDays(-1)
            },
            // Acme Admin
            new User
            {
                Id = acmeAdminUserId,
                TenantId = acmeTenantId,
                FirstName = "John",
                LastName = "Doe",
                Email = "admin@acme.com",
                PasswordHash = "$2a$11$QUxBZq94RgWvH09M.ER7EuF6Ju3mP45b9cEXAS99Iz09cAfKEZUeW", // Hash für "admin123"
                Role = Enums.UserRole.TenantAdmin,
                IsActive = true,
                IsEmailConfirmed = true,
                CreatedAt = DateTime.UtcNow.AddDays(-25),
                LastLoginAt = DateTime.UtcNow.AddDays(-2)
            }
        );

        // Seed User Group Memberships
        modelBuilder.Entity<UserGroupMembership>().HasData(
            // ChargingControl Admin Membership
            new UserGroupMembership
            {
                Id = Guid.Parse("ccccdddd-1111-2222-3333-444444444444"),
                UserId = adminUserId,
                UserGroupId = adminGroupId,
                AssignedAt = DateTime.UtcNow.AddDays(-30)
            },
            // Acme Admin Membership
            new UserGroupMembership
            {
                Id = Guid.Parse("ccccdddd-2222-3333-4444-555555555555"),
                UserId = acmeAdminUserId,
                UserGroupId = acmeAdminGroupId,
                AssignedAt = DateTime.UtcNow.AddDays(-25)
            }
        );

        // Seed Charging Parks
        modelBuilder.Entity<ChargingPark>().HasData(
            new ChargingPark
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                TenantId = defaultTenantId,
                Name = "Hauptgebäude Parkplatz",
                Description = "Zentraler Parkplatz am Hauptgebäude",
                Address = "Hauptstraße 123",
                PostalCode = "80331",
                City = "München",
                Country = "Deutschland",
                Latitude = 48.1351m,
                Longitude = 11.5820m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-20)
            },
            new ChargingPark
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                TenantId = defaultTenantId,
                Name = "Außenstelle Berlin",
                Description = "Ladepark an der Berliner Niederlassung",
                Address = "Friedrichstraße 45",
                PostalCode = "10117",
                City = "Berlin",
                Country = "Deutschland",
                Latitude = 52.5200m,
                Longitude = 13.4050m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            }
        );

        // Seed Charging Stations
        modelBuilder.Entity<ChargingStation>().HasData(
            new ChargingStation
            {
                Id = station1Id,
                ChargingParkId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                StationId = "CCS-001",
                Name = "CCS Schnellladesäule 1",
                Vendor = "Siemens",
                Model = "Sicharge CC",
                Type = Entities.ChargingStationType.DC,
                MaxPower = 150,
                NumberOfConnectors = 2,
                Status = Entities.ChargingStationStatus.Available,
                Latitude = 48.1351m,
                Longitude = 11.5820m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-20),
                LastHeartbeat = DateTime.UtcNow.AddMinutes(-5)
            },
            new ChargingStation
            {
                Id = station2Id,
                ChargingParkId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                StationId = "AC-001",
                Name = "AC Standardladesäule 1",
                Vendor = "ABB",
                Model = "Terra AC",
                Type = Entities.ChargingStationType.AC,
                MaxPower = 22,
                NumberOfConnectors = 2,
                Status = Entities.ChargingStationStatus.Available,
                Latitude = 48.1352m,
                Longitude = 11.5821m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-18),
                LastHeartbeat = DateTime.UtcNow.AddMinutes(-3)
            }
        );

        // Seed Charging Points
        var chargingPoint1Id = Guid.Parse("88888888-8888-8888-8888-888888888888");
        var chargingPoint2Id = Guid.Parse("99999999-9999-9999-9999-999999999999");

        modelBuilder.Entity<ChargingPoint>().HasData(
            new ChargingPoint
            {
                Id = chargingPoint1Id,
                ChargingStationId = station1Id,
                EvseId = 1,
                Name = "Ladepunkt 1",
                MaxPower = 150,
                ConnectorType = "CCS",
                ConnectorFormat = "SOCKET",
                PowerType = "DC",
                MaxCurrent = 200,
                MaxVoltage = 800,
                Status = ChargingPointStatus.Available,
                SupportsSmartCharging = true,
                SupportsRemoteStartStop = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-20)
            },
            new ChargingPoint
            {
                Id = chargingPoint2Id,
                ChargingStationId = station1Id,
                EvseId = 2,
                Name = "Ladepunkt 2",
                MaxPower = 150,
                ConnectorType = "CCS",
                ConnectorFormat = "SOCKET",
                PowerType = "DC",
                MaxCurrent = 200,
                MaxVoltage = 800,
                Status = ChargingPointStatus.Available,
                SupportsSmartCharging = true,
                SupportsRemoteStartStop = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-20)
            }
        );

        // ChargingPoints haben jetzt alle Connector-Eigenschaften direkt
        // Seed-Daten werden in ChargingPoint-Seeding aktualisiert

        // Seed Vehicles
        modelBuilder.Entity<Vehicle>().HasData(
            new Vehicle
            {
                Id = vehicle1Id,
                TenantId = defaultTenantId,
                LicensePlate = "M-CC 1234",
                Make = "Tesla",
                Model = "Model 3",
                Year = 2023,
                Type = Entities.VehicleType.PoolVehicle,
                Color = "Pearl White",
                Notes = "Poolfahrzeug für Geschäftsreisen",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-25)
            },
            new Vehicle
            {
                Id = vehicle2Id,
                TenantId = defaultTenantId,
                LicensePlate = "M-CC 5678",
                Make = "BMW",
                Model = "i3",
                Year = 2022,
                Type = Entities.VehicleType.CompanyVehicle,
                Color = "Mineral White",
                Notes = "Dienstwagen für den Vertrieb",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-20)
            }
        );

        // Seed Vehicle Assignment
        modelBuilder.Entity<VehicleAssignment>().HasData(
            new VehicleAssignment
            {
                Id = Guid.Parse("cccc0000-0000-0000-0000-000000000000"),
                VehicleId = vehicle1Id,
                UserId = adminUserId,
                AssignmentType = Entities.VehicleAssignmentType.Temporary,
                AssignedAt = DateTime.UtcNow.AddDays(-7)
            }
        );

        // Seed Billing Account
        modelBuilder.Entity<BillingAccount>().HasData(
            new BillingAccount
            {
                Id = Guid.Parse("dddd1111-1111-1111-1111-111111111111"),
                TenantId = defaultTenantId,
                AccountName = "Hauptkonto ChargingControl GmbH",
                Type = Entities.BillingAccountType.Company,
                Status = Entities.BillingAccountStatus.Active,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            }
        );

        // Seed QR Codes
        modelBuilder.Entity<QrCode>().HasData(
            new QrCode
            {
                Id = Guid.Parse("eeee2222-2222-2222-2222-222222222222"),
                TenantId = defaultTenantId,
                Code = "CC-PARK-001",
                Type = Entities.QrCodeType.ParkInvitation,
                ChargingParkId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Description = "Einladung zum Hauptparkplatz",
                ExpiresAt = DateTime.UtcNow.AddDays(365),
                MaxUses = 100,
                CurrentUses = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            }
        );
    }
}
