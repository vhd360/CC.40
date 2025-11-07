using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ChargingControlSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Resource = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Subdomain = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ParentTenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TaxId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Theme = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tenants_Tenants_ParentTenantId",
                        column: x => x.ParentTenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BillingAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StripeCustomerId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeactivatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillingAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillingAccounts_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChargingParks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(10,8)", precision: 10, scale: 8, nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(11,8)", precision: 11, scale: 8, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChargingParks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChargingParks_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Role = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsEmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LicensePlate = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Make = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeactivatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vehicles_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChargingStationGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChargingParkId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChargingStationGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChargingStationGroups_ChargingParks_ChargingParkId",
                        column: x => x.ChargingParkId,
                        principalTable: "ChargingParks",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ChargingStationGroups_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChargingStations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChargingParkId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StationId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Vendor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    MaxPower = table.Column<int>(type: "int", nullable: false),
                    NumberOfConnectors = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(10,8)", precision: 10, scale: 8, nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(11,8)", precision: 11, scale: 8, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ChargeBoxId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OcppPassword = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    OcppProtocol = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OcppEndpoint = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastHeartbeat = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChargingStations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChargingStations_ChargingParks_ChargingParkId",
                        column: x => x.ChargingParkId,
                        principalTable: "ChargingParks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuthorizationMethods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Identifier = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FriendlyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ValidUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorizationMethods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuthorizationMethods_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QrCodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    ChargingParkId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MaxUses = table.Column<int>(type: "int", nullable: true),
                    CurrentUses = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QrCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QrCodes_ChargingParks_ChargingParkId",
                        column: x => x.ChargingParkId,
                        principalTable: "ChargingParks",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_QrCodes_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QrCodes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InviteToken = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    InviteTokenExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserGroups_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserGroups_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VehicleAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignmentType = table.Column<int>(type: "int", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReturnedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleAssignments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VehicleAssignments_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChargingConnectors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChargingStationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConnectorId = table.Column<int>(type: "int", nullable: false),
                    ConnectorType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MaxPower = table.Column<int>(type: "int", nullable: false),
                    MaxCurrent = table.Column<int>(type: "int", nullable: false),
                    MaxVoltage = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChargingConnectors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChargingConnectors_ChargingStations_ChargingStationId",
                        column: x => x.ChargingStationId,
                        principalTable: "ChargingStations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChargingStationGroupMemberships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChargingStationGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChargingStationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChargingStationGroupMemberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChargingStationGroupMemberships_ChargingStationGroups_ChargingStationGroupId",
                        column: x => x.ChargingStationGroupId,
                        principalTable: "ChargingStationGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChargingStationGroupMemberships_ChargingStations_ChargingStationId",
                        column: x => x.ChargingStationId,
                        principalTable: "ChargingStations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GroupPermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupPermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupPermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroupPermissions_UserGroups_UserGroupId",
                        column: x => x.UserGroupId,
                        principalTable: "UserGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserGroupChargingStationGroupPermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChargingStationGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GrantedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGroupChargingStationGroupPermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserGroupChargingStationGroupPermissions_ChargingStationGroups_ChargingStationGroupId",
                        column: x => x.ChargingStationGroupId,
                        principalTable: "ChargingStationGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserGroupChargingStationGroupPermissions_UserGroups_UserGroupId",
                        column: x => x.UserGroupId,
                        principalTable: "UserGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserGroupMemberships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGroupMemberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserGroupMemberships_UserGroups_UserGroupId",
                        column: x => x.UserGroupId,
                        principalTable: "UserGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserGroupMemberships_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChargingSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChargingConnectorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VehicleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    QrCodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AuthorizationMethodId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SessionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OcppTransactionId = table.Column<int>(type: "int", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EnergyDelivered = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ChargingStationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChargingSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChargingSessions_AuthorizationMethods_AuthorizationMethodId",
                        column: x => x.AuthorizationMethodId,
                        principalTable: "AuthorizationMethods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChargingSessions_ChargingConnectors_ChargingConnectorId",
                        column: x => x.ChargingConnectorId,
                        principalTable: "ChargingConnectors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChargingSessions_ChargingStations_ChargingStationId",
                        column: x => x.ChargingStationId,
                        principalTable: "ChargingStations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ChargingSessions_QrCodes_QrCodeId",
                        column: x => x.QrCodeId,
                        principalTable: "QrCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChargingSessions_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChargingSessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChargingSessions_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BillingTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BillingAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChargingSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TransactionType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    StripePaymentIntentId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillingTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillingTransactions_BillingAccounts_BillingAccountId",
                        column: x => x.BillingAccountId,
                        principalTable: "BillingAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BillingTransactions_ChargingSessions_ChargingSessionId",
                        column: x => x.ChargingSessionId,
                        principalTable: "ChargingSessions",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Action", "Description", "Name", "Resource" },
                values: new object[,]
                {
                    { new Guid("0509bad7-8c59-42a0-837f-e1987ef19644"), "create", "Create QR codes", "qrcodes.create", "qrcodes" },
                    { new Guid("1462baa9-f966-419b-9551-4c840d84b7a3"), "manage", "Manage charging infrastructure", "charging.manage", "charging" },
                    { new Guid("1eb710e7-bbf3-4bf1-8f32-249c4655304d"), "view", "View charging infrastructure", "charging.view", "charging" },
                    { new Guid("560b1513-3720-429a-bcfc-374298a2b263"), "assign", "Assign vehicles", "vehicles.assign", "vehicles" },
                    { new Guid("5cd9d607-2900-465d-880f-0591d17dab70"), "view", "View users", "users.view", "users" },
                    { new Guid("61b6872f-8e41-45f1-b6ab-a7e0ae8263ea"), "view", "View billing information", "billing.view", "billing" },
                    { new Guid("7c161436-8436-472e-8630-b698f32c7f1b"), "create", "Create users", "users.create", "users" },
                    { new Guid("7e14b3fa-fa5c-42e9-8e42-816cb5a7d6aa"), "edit", "Edit vehicles", "vehicles.edit", "vehicles" },
                    { new Guid("84d5dc64-d276-4a58-b48a-f800860b6cf2"), "view", "View vehicles", "vehicles.view", "vehicles" },
                    { new Guid("92e8640d-b0be-4823-af18-99c25a80eaaa"), "manage", "Manage QR codes", "qrcodes.manage", "qrcodes" },
                    { new Guid("96b9b014-b140-41b7-8cfd-248780d5cec1"), "edit", "Edit users", "users.edit", "users" },
                    { new Guid("adee50c8-57ab-44b1-9547-da2ef4f650ac"), "view", "View QR codes", "qrcodes.view", "qrcodes" },
                    { new Guid("c028445b-8bc8-4555-871b-618c98d6d69b"), "create", "Create vehicles", "vehicles.create", "vehicles" },
                    { new Guid("d535ad43-8c6a-45f8-8ac2-4c95a2cdfba5"), "delete", "Delete users", "users.delete", "users" },
                    { new Guid("da09ab24-82de-4067-aaac-00e9371e6044"), "manage", "Manage billing", "billing.manage", "billing" }
                });

            migrationBuilder.InsertData(
                table: "Tenants",
                columns: new[] { "Id", "Address", "City", "Country", "CreatedAt", "Description", "Email", "IsActive", "LogoUrl", "Name", "ParentTenantId", "Phone", "PostalCode", "Subdomain", "TaxId", "Theme", "UpdatedAt", "Website" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), "Musterstraße 123", "Berlin", "Deutschland", new DateTime(2025, 9, 23, 14, 56, 25, 351, DateTimeKind.Utc).AddTicks(8200), "Hauptunternehmen für Lade-Management", "info@chargingcontrol.de", true, null, "ChargingControl GmbH", null, "+49 30 12345678", "10115", "chargingcontrol", "DE123456789", 0, null, "https://www.chargingcontrol.de" });

            migrationBuilder.InsertData(
                table: "BillingAccounts",
                columns: new[] { "Id", "AccountName", "CreatedAt", "DeactivatedAt", "Status", "StripeCustomerId", "TenantId", "Type" },
                values: new object[] { new Guid("dddd1111-1111-1111-1111-111111111111"), "Hauptkonto ChargingControl GmbH", new DateTime(2025, 9, 23, 14, 56, 25, 351, DateTimeKind.Utc).AddTicks(8515), null, 0, null, new Guid("11111111-1111-1111-1111-111111111111"), 1 });

            migrationBuilder.InsertData(
                table: "ChargingParks",
                columns: new[] { "Id", "Address", "City", "Country", "CreatedAt", "Description", "IsActive", "Latitude", "Longitude", "Name", "PostalCode", "TenantId" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Hauptstraße 123", "München", "Deutschland", new DateTime(2025, 10, 3, 14, 56, 25, 351, DateTimeKind.Utc).AddTicks(8343), "Zentraler Parkplatz am Hauptgebäude", true, 48.1351m, 11.5820m, "Hauptgebäude Parkplatz", "80331", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "Friedrichstraße 45", "Berlin", "Deutschland", new DateTime(2025, 10, 8, 14, 56, 25, 351, DateTimeKind.Utc).AddTicks(8346), "Ladepark an der Berliner Niederlassung", true, 52.5200m, 13.4050m, "Außenstelle Berlin", "10117", new Guid("11111111-1111-1111-1111-111111111111") }
                });

            migrationBuilder.InsertData(
                table: "Tenants",
                columns: new[] { "Id", "Address", "City", "Country", "CreatedAt", "Description", "Email", "IsActive", "LogoUrl", "Name", "ParentTenantId", "Phone", "PostalCode", "Subdomain", "TaxId", "Theme", "UpdatedAt", "Website" },
                values: new object[] { new Guid("aaaabbbb-1111-2222-3333-444444444444"), "Acme Straße 456", "München", "Deutschland", new DateTime(2025, 9, 28, 14, 56, 25, 351, DateTimeKind.Utc).AddTicks(8209), "Sub-Tenant für Acme Corporation", "info@acme.de", true, null, "Acme GmbH", new Guid("11111111-1111-1111-1111-111111111111"), "+49 89 98765432", "80331", "acme", "DE987654321", 0, null, "https://www.acme.de" });

            migrationBuilder.InsertData(
                table: "UserGroups",
                columns: new[] { "Id", "CreatedAt", "Description", "InviteToken", "InviteTokenExpiresAt", "IsActive", "Name", "TenantId", "UserId" },
                values: new object[] { new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2025, 9, 23, 14, 56, 25, 351, DateTimeKind.Utc).AddTicks(8240), "Vollständige Administrator-Rechte", null, null, true, "Administratoren", new Guid("11111111-1111-1111-1111-111111111111"), null });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FirstName", "IsActive", "IsEmailConfirmed", "LastLoginAt", "LastName", "PasswordHash", "PhoneNumber", "Role", "TenantId" },
                values: new object[] { new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2025, 9, 23, 14, 56, 25, 351, DateTimeKind.Utc).AddTicks(8272), "admin@chargingcontrol.com", "Admin", true, true, new DateTime(2025, 10, 22, 14, 56, 25, 351, DateTimeKind.Utc).AddTicks(8272), "User", "$2a$11$QUxBZq94RgWvH09M.ER7EuF6Ju3mP45b9cEXAS99Iz09cAfKEZUeW", null, 1, new Guid("11111111-1111-1111-1111-111111111111") });

            migrationBuilder.InsertData(
                table: "Vehicles",
                columns: new[] { "Id", "Color", "CreatedAt", "DeactivatedAt", "IsActive", "LicensePlate", "Make", "Model", "Notes", "TenantId", "Type", "Year" },
                values: new object[,]
                {
                    { new Guid("66666666-6666-6666-6666-666666666666"), "Pearl White", new DateTime(2025, 9, 28, 14, 56, 25, 351, DateTimeKind.Utc).AddTicks(8443), null, true, "M-CC 1234", "Tesla", "Model 3", "Poolfahrzeug für Geschäftsreisen", new Guid("11111111-1111-1111-1111-111111111111"), 0, 2023 },
                    { new Guid("77777777-7777-7777-7777-777777777777"), "Mineral White", new DateTime(2025, 10, 3, 14, 56, 25, 351, DateTimeKind.Utc).AddTicks(8444), null, true, "M-CC 5678", "BMW", "i3", "Dienstwagen für den Vertrieb", new Guid("11111111-1111-1111-1111-111111111111"), 1, 2022 }
                });

            migrationBuilder.InsertData(
                table: "ChargingStations",
                columns: new[] { "Id", "ChargeBoxId", "ChargingParkId", "CreatedAt", "IsActive", "LastHeartbeat", "Latitude", "Longitude", "MaxPower", "Model", "Name", "Notes", "NumberOfConnectors", "OcppEndpoint", "OcppPassword", "OcppProtocol", "StationId", "Status", "Type", "Vendor" },
                values: new object[,]
                {
                    { new Guid("44444444-4444-4444-4444-444444444444"), null, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2025, 10, 3, 14, 56, 25, 351, DateTimeKind.Utc).AddTicks(8375), true, new DateTime(2025, 10, 23, 14, 51, 25, 351, DateTimeKind.Utc).AddTicks(8375), 48.1351m, 11.5820m, 150, "Sicharge CC", "CCS Schnellladesäule 1", null, 2, null, null, null, "CCS-001", 0, 1, "Siemens" },
                    { new Guid("55555555-5555-5555-5555-555555555555"), null, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2025, 10, 5, 14, 56, 25, 351, DateTimeKind.Utc).AddTicks(8380), true, new DateTime(2025, 10, 23, 14, 53, 25, 351, DateTimeKind.Utc).AddTicks(8380), 48.1352m, 11.5821m, 22, "Terra AC", "AC Standardladesäule 1", null, 2, null, null, null, "AC-001", 0, 0, "ABB" }
                });

            migrationBuilder.InsertData(
                table: "QrCodes",
                columns: new[] { "Id", "ChargingParkId", "Code", "CreatedAt", "CurrentUses", "Description", "ExpiresAt", "IsActive", "LastUsedAt", "MaxUses", "TenantId", "Type", "UserId" },
                values: new object[] { new Guid("eeee2222-2222-2222-2222-222222222222"), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "CC-PARK-001", new DateTime(2025, 10, 13, 14, 56, 25, 351, DateTimeKind.Utc).AddTicks(8544), 0, "Einladung zum Hauptparkplatz", new DateTime(2026, 10, 23, 14, 56, 25, 351, DateTimeKind.Utc).AddTicks(8543), true, null, 100, new Guid("11111111-1111-1111-1111-111111111111"), 0, null });

            migrationBuilder.InsertData(
                table: "UserGroupMemberships",
                columns: new[] { "Id", "AssignedAt", "UserGroupId", "UserId" },
                values: new object[] { new Guid("ccccdddd-1111-2222-3333-444444444444"), new DateTime(2025, 9, 23, 14, 56, 25, 351, DateTimeKind.Utc).AddTicks(8306), new Guid("33333333-3333-3333-3333-333333333333"), new Guid("22222222-2222-2222-2222-222222222222") });

            migrationBuilder.InsertData(
                table: "UserGroups",
                columns: new[] { "Id", "CreatedAt", "Description", "InviteToken", "InviteTokenExpiresAt", "IsActive", "Name", "TenantId", "UserId" },
                values: new object[] { new Guid("aaaabbbb-3333-4444-5555-666666666666"), new DateTime(2025, 9, 28, 14, 56, 25, 351, DateTimeKind.Utc).AddTicks(8241), "Administrator-Rechte für Acme", null, null, true, "Acme Administratoren", new Guid("aaaabbbb-1111-2222-3333-444444444444"), null });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FirstName", "IsActive", "IsEmailConfirmed", "LastLoginAt", "LastName", "PasswordHash", "PhoneNumber", "Role", "TenantId" },
                values: new object[] { new Guid("aaaabbbb-2222-3333-4444-555555555555"), new DateTime(2025, 9, 28, 14, 56, 25, 351, DateTimeKind.Utc).AddTicks(8278), "admin@acme.com", "John", true, true, new DateTime(2025, 10, 21, 14, 56, 25, 351, DateTimeKind.Utc).AddTicks(8278), "Doe", "$2a$11$QUxBZq94RgWvH09M.ER7EuF6Ju3mP45b9cEXAS99Iz09cAfKEZUeW", null, 1, new Guid("aaaabbbb-1111-2222-3333-444444444444") });

            migrationBuilder.InsertData(
                table: "VehicleAssignments",
                columns: new[] { "Id", "AssignedAt", "AssignmentType", "Notes", "ReturnedAt", "UserId", "VehicleId" },
                values: new object[] { new Guid("cccc0000-0000-0000-0000-000000000000"), new DateTime(2025, 10, 16, 14, 56, 25, 351, DateTimeKind.Utc).AddTicks(8477), 1, null, null, new Guid("22222222-2222-2222-2222-222222222222"), new Guid("66666666-6666-6666-6666-666666666666") });

            migrationBuilder.InsertData(
                table: "ChargingConnectors",
                columns: new[] { "Id", "ChargingStationId", "ConnectorId", "ConnectorType", "CreatedAt", "MaxCurrent", "MaxPower", "MaxVoltage", "Status" },
                values: new object[,]
                {
                    { new Guid("88888888-8888-8888-8888-888888888888"), new Guid("44444444-4444-4444-4444-444444444444"), 1, "CCS", new DateTime(2025, 10, 3, 14, 56, 25, 351, DateTimeKind.Utc).AddTicks(8408), 200, 0, 800, 0 },
                    { new Guid("99999999-9999-9999-9999-999999999999"), new Guid("44444444-4444-4444-4444-444444444444"), 2, "CCS", new DateTime(2025, 10, 3, 14, 56, 25, 351, DateTimeKind.Utc).AddTicks(8411), 200, 0, 800, 0 }
                });

            migrationBuilder.InsertData(
                table: "UserGroupMemberships",
                columns: new[] { "Id", "AssignedAt", "UserGroupId", "UserId" },
                values: new object[] { new Guid("ccccdddd-2222-3333-4444-555555555555"), new DateTime(2025, 9, 28, 14, 56, 25, 351, DateTimeKind.Utc).AddTicks(8309), new Guid("aaaabbbb-3333-4444-5555-666666666666"), new Guid("aaaabbbb-2222-3333-4444-555555555555") });

            migrationBuilder.CreateIndex(
                name: "IX_AuthorizationMethods_UserId",
                table: "AuthorizationMethods",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BillingAccounts_TenantId",
                table: "BillingAccounts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_BillingTransactions_BillingAccountId",
                table: "BillingTransactions",
                column: "BillingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_BillingTransactions_ChargingSessionId",
                table: "BillingTransactions",
                column: "ChargingSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ChargingConnectors_ChargingStationId",
                table: "ChargingConnectors",
                column: "ChargingStationId");

            migrationBuilder.CreateIndex(
                name: "IX_ChargingParks_TenantId",
                table: "ChargingParks",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ChargingSessions_AuthorizationMethodId",
                table: "ChargingSessions",
                column: "AuthorizationMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_ChargingSessions_ChargingConnectorId",
                table: "ChargingSessions",
                column: "ChargingConnectorId");

            migrationBuilder.CreateIndex(
                name: "IX_ChargingSessions_ChargingStationId",
                table: "ChargingSessions",
                column: "ChargingStationId");

            migrationBuilder.CreateIndex(
                name: "IX_ChargingSessions_QrCodeId",
                table: "ChargingSessions",
                column: "QrCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_ChargingSessions_TenantId",
                table: "ChargingSessions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ChargingSessions_UserId",
                table: "ChargingSessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChargingSessions_VehicleId",
                table: "ChargingSessions",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_ChargingStationGroupMemberships_ChargingStationGroupId",
                table: "ChargingStationGroupMemberships",
                column: "ChargingStationGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ChargingStationGroupMemberships_ChargingStationId",
                table: "ChargingStationGroupMemberships",
                column: "ChargingStationId");

            migrationBuilder.CreateIndex(
                name: "IX_ChargingStationGroups_ChargingParkId",
                table: "ChargingStationGroups",
                column: "ChargingParkId");

            migrationBuilder.CreateIndex(
                name: "IX_ChargingStationGroups_TenantId",
                table: "ChargingStationGroups",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ChargingStations_ChargingParkId",
                table: "ChargingStations",
                column: "ChargingParkId");

            migrationBuilder.CreateIndex(
                name: "IX_ChargingStations_StationId",
                table: "ChargingStations",
                column: "StationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupPermissions_PermissionId",
                table: "GroupPermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupPermissions_UserGroupId_PermissionId",
                table: "GroupPermissions",
                columns: new[] { "UserGroupId", "PermissionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QrCodes_ChargingParkId",
                table: "QrCodes",
                column: "ChargingParkId");

            migrationBuilder.CreateIndex(
                name: "IX_QrCodes_Code",
                table: "QrCodes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QrCodes_TenantId",
                table: "QrCodes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_QrCodes_UserId",
                table: "QrCodes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_ParentTenantId",
                table: "Tenants",
                column: "ParentTenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Subdomain",
                table: "Tenants",
                column: "Subdomain",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserGroupChargingStationGroupPermissions_ChargingStationGroupId",
                table: "UserGroupChargingStationGroupPermissions",
                column: "ChargingStationGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroupChargingStationGroupPermissions_UserGroupId",
                table: "UserGroupChargingStationGroupPermissions",
                column: "UserGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroupMemberships_UserGroupId",
                table: "UserGroupMemberships",
                column: "UserGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroupMemberships_UserId_UserGroupId",
                table: "UserGroupMemberships",
                columns: new[] { "UserId", "UserGroupId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserGroups_TenantId",
                table: "UserGroups",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroups_UserId",
                table: "UserGroups",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId_Email",
                table: "Users",
                columns: new[] { "TenantId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleAssignments_UserId",
                table: "VehicleAssignments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleAssignments_VehicleId",
                table: "VehicleAssignments",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_TenantId_LicensePlate",
                table: "Vehicles",
                columns: new[] { "TenantId", "LicensePlate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BillingTransactions");

            migrationBuilder.DropTable(
                name: "ChargingStationGroupMemberships");

            migrationBuilder.DropTable(
                name: "GroupPermissions");

            migrationBuilder.DropTable(
                name: "UserGroupChargingStationGroupPermissions");

            migrationBuilder.DropTable(
                name: "UserGroupMemberships");

            migrationBuilder.DropTable(
                name: "VehicleAssignments");

            migrationBuilder.DropTable(
                name: "BillingAccounts");

            migrationBuilder.DropTable(
                name: "ChargingSessions");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "ChargingStationGroups");

            migrationBuilder.DropTable(
                name: "UserGroups");

            migrationBuilder.DropTable(
                name: "AuthorizationMethods");

            migrationBuilder.DropTable(
                name: "ChargingConnectors");

            migrationBuilder.DropTable(
                name: "QrCodes");

            migrationBuilder.DropTable(
                name: "Vehicles");

            migrationBuilder.DropTable(
                name: "ChargingStations");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "ChargingParks");

            migrationBuilder.DropTable(
                name: "Tenants");
        }
    }
}
