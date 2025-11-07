using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ChargingControlSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddChargingPointEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChargingConnectors_ChargingStations_ChargingStationId",
                table: "ChargingConnectors");

            migrationBuilder.DropForeignKey(
                name: "FK_ChargingSessions_ChargingStations_ChargingStationId",
                table: "ChargingSessions");

            migrationBuilder.DropIndex(
                name: "IX_ChargingSessions_ChargingStationId",
                table: "ChargingSessions");

            migrationBuilder.DeleteData(
                table: "ChargingConnectors",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"));

            migrationBuilder.DeleteData(
                table: "ChargingConnectors",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("0509bad7-8c59-42a0-837f-e1987ef19644"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("1462baa9-f966-419b-9551-4c840d84b7a3"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("1eb710e7-bbf3-4bf1-8f32-249c4655304d"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("560b1513-3720-429a-bcfc-374298a2b263"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("5cd9d607-2900-465d-880f-0591d17dab70"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("61b6872f-8e41-45f1-b6ab-a7e0ae8263ea"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("7c161436-8436-472e-8630-b698f32c7f1b"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("7e14b3fa-fa5c-42e9-8e42-816cb5a7d6aa"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("84d5dc64-d276-4a58-b48a-f800860b6cf2"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("92e8640d-b0be-4823-af18-99c25a80eaaa"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("96b9b014-b140-41b7-8cfd-248780d5cec1"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("adee50c8-57ab-44b1-9547-da2ef4f650ac"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("c028445b-8bc8-4555-871b-618c98d6d69b"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d535ad43-8c6a-45f8-8ac2-4c95a2cdfba5"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("da09ab24-82de-4067-aaac-00e9371e6044"));

            migrationBuilder.DropColumn(
                name: "ChargingStationId",
                table: "ChargingSessions");

            migrationBuilder.RenameColumn(
                name: "ChargingStationId",
                table: "ChargingConnectors",
                newName: "ChargingPointId");

            migrationBuilder.RenameIndex(
                name: "IX_ChargingConnectors_ChargingStationId",
                table: "ChargingConnectors",
                newName: "IX_ChargingConnectors_ChargingPointId");

            migrationBuilder.AddColumn<string>(
                name: "ConnectorFormat",
                table: "ChargingConnectors",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ChargingConnectors",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastStatusChange",
                table: "ChargingConnectors",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "ChargingConnectors",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhysicalReference",
                table: "ChargingConnectors",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PowerType",
                table: "ChargingConnectors",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ChargingPoints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChargingStationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EvseId = table.Column<int>(type: "int", nullable: false),
                    EvseIdExternal = table.Column<string>(type: "nvarchar(48)", maxLength: 48, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MaxPower = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PublicKey = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CertificateChain = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: true),
                    SupportsSmartCharging = table.Column<bool>(type: "bit", nullable: false),
                    SupportsRemoteStartStop = table.Column<bool>(type: "bit", nullable: false),
                    SupportsReservation = table.Column<bool>(type: "bit", nullable: false),
                    TariffInfo = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastStatusChange = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChargingPoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChargingPoints_ChargingStations_ChargingStationId",
                        column: x => x.ChargingStationId,
                        principalTable: "ChargingStations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "BillingAccounts",
                keyColumn: "Id",
                keyValue: new Guid("dddd1111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2025, 9, 23, 15, 46, 44, 374, DateTimeKind.Utc).AddTicks(1334));

            migrationBuilder.UpdateData(
                table: "ChargingParks",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 3, 15, 46, 44, 374, DateTimeKind.Utc).AddTicks(1162));

            migrationBuilder.UpdateData(
                table: "ChargingParks",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 8, 15, 46, 44, 374, DateTimeKind.Utc).AddTicks(1164));

            migrationBuilder.InsertData(
                table: "ChargingPoints",
                columns: new[] { "Id", "CertificateChain", "ChargingStationId", "CreatedAt", "Description", "EvseId", "EvseIdExternal", "IsActive", "LastStatusChange", "MaxPower", "Name", "Notes", "PublicKey", "Status", "SupportsRemoteStartStop", "SupportsReservation", "SupportsSmartCharging", "TariffInfo" },
                values: new object[,]
                {
                    { new Guid("88888888-8888-8888-8888-888888888888"), null, new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2025, 10, 3, 15, 46, 44, 374, DateTimeKind.Utc).AddTicks(1220), null, 1, null, true, null, 150, "Ladepunkt 1", null, null, 0, true, false, true, null },
                    { new Guid("99999999-9999-9999-9999-999999999999"), null, new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2025, 10, 3, 15, 46, 44, 374, DateTimeKind.Utc).AddTicks(1222), null, 2, null, true, null, 150, "Ladepunkt 2", null, null, 0, true, false, true, null }
                });

            migrationBuilder.UpdateData(
                table: "ChargingStations",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "LastHeartbeat" },
                values: new object[] { new DateTime(2025, 10, 3, 15, 46, 44, 374, DateTimeKind.Utc).AddTicks(1191), new DateTime(2025, 10, 23, 15, 41, 44, 374, DateTimeKind.Utc).AddTicks(1191) });

            migrationBuilder.UpdateData(
                table: "ChargingStations",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "LastHeartbeat" },
                values: new object[] { new DateTime(2025, 10, 5, 15, 46, 44, 374, DateTimeKind.Utc).AddTicks(1195), new DateTime(2025, 10, 23, 15, 43, 44, 374, DateTimeKind.Utc).AddTicks(1197) });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Action", "Description", "Name", "Resource" },
                values: new object[,]
                {
                    { new Guid("21430b63-7975-444b-9361-0ec5d13283f2"), "delete", "Delete users", "users.delete", "users" },
                    { new Guid("2254ecb6-ca87-4fac-b669-cfc6660df267"), "edit", "Edit vehicles", "vehicles.edit", "vehicles" },
                    { new Guid("2b8d56cf-2e13-4c65-8f48-db6dbc573804"), "edit", "Edit users", "users.edit", "users" },
                    { new Guid("3093227c-cce4-422f-944b-9847186047c4"), "manage", "Manage billing", "billing.manage", "billing" },
                    { new Guid("3a546465-2b00-4b93-ad1c-410fe105af9b"), "manage", "Manage QR codes", "qrcodes.manage", "qrcodes" },
                    { new Guid("3d0526f5-fe92-4712-8033-0c1b585d3403"), "create", "Create QR codes", "qrcodes.create", "qrcodes" },
                    { new Guid("4d47d64b-eb0f-45bb-bc2e-b49ccc080474"), "create", "Create users", "users.create", "users" },
                    { new Guid("7003cd50-79cb-4ee0-9e2f-9c1310bd2f34"), "create", "Create vehicles", "vehicles.create", "vehicles" },
                    { new Guid("79a46c17-ea45-44aa-9e2b-c287d66ac991"), "view", "View charging infrastructure", "charging.view", "charging" },
                    { new Guid("8a540381-672f-4422-a50c-e8dd55188f99"), "view", "View billing information", "billing.view", "billing" },
                    { new Guid("ac94e6e3-0cd3-44f2-94f3-2d78041729fd"), "view", "View users", "users.view", "users" },
                    { new Guid("b004d1d3-9f21-4abf-87ac-b9d7c0a8fdc1"), "view", "View QR codes", "qrcodes.view", "qrcodes" },
                    { new Guid("ba333477-5719-4eea-aa99-0ceda3939728"), "assign", "Assign vehicles", "vehicles.assign", "vehicles" },
                    { new Guid("d41efa8d-7459-4960-ad3b-a2c94255a218"), "manage", "Manage charging infrastructure", "charging.manage", "charging" },
                    { new Guid("f5741eb8-9866-415a-ac48-b350e5f52f85"), "view", "View vehicles", "vehicles.view", "vehicles" }
                });

            migrationBuilder.UpdateData(
                table: "QrCodes",
                keyColumn: "Id",
                keyValue: new Guid("eeee2222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "ExpiresAt" },
                values: new object[] { new DateTime(2025, 10, 13, 15, 46, 44, 374, DateTimeKind.Utc).AddTicks(1354), new DateTime(2026, 10, 23, 15, 46, 44, 374, DateTimeKind.Utc).AddTicks(1352) });

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2025, 9, 23, 15, 46, 44, 374, DateTimeKind.Utc).AddTicks(1041));

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("aaaabbbb-1111-2222-3333-444444444444"),
                column: "CreatedAt",
                value: new DateTime(2025, 9, 28, 15, 46, 44, 374, DateTimeKind.Utc).AddTicks(1052));

            migrationBuilder.UpdateData(
                table: "UserGroupMemberships",
                keyColumn: "Id",
                keyValue: new Guid("ccccdddd-1111-2222-3333-444444444444"),
                column: "AssignedAt",
                value: new DateTime(2025, 9, 23, 15, 46, 44, 374, DateTimeKind.Utc).AddTicks(1130));

            migrationBuilder.UpdateData(
                table: "UserGroupMemberships",
                keyColumn: "Id",
                keyValue: new Guid("ccccdddd-2222-3333-4444-555555555555"),
                column: "AssignedAt",
                value: new DateTime(2025, 9, 28, 15, 46, 44, 374, DateTimeKind.Utc).AddTicks(1132));

            migrationBuilder.UpdateData(
                table: "UserGroups",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2025, 9, 23, 15, 46, 44, 374, DateTimeKind.Utc).AddTicks(1076));

            migrationBuilder.UpdateData(
                table: "UserGroups",
                keyColumn: "Id",
                keyValue: new Guid("aaaabbbb-3333-4444-5555-666666666666"),
                column: "CreatedAt",
                value: new DateTime(2025, 9, 28, 15, 46, 44, 374, DateTimeKind.Utc).AddTicks(1077));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "LastLoginAt" },
                values: new object[] { new DateTime(2025, 9, 23, 15, 46, 44, 374, DateTimeKind.Utc).AddTicks(1101), new DateTime(2025, 10, 22, 15, 46, 44, 374, DateTimeKind.Utc).AddTicks(1101) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("aaaabbbb-2222-3333-4444-555555555555"),
                columns: new[] { "CreatedAt", "LastLoginAt" },
                values: new object[] { new DateTime(2025, 9, 28, 15, 46, 44, 374, DateTimeKind.Utc).AddTicks(1108), new DateTime(2025, 10, 21, 15, 46, 44, 374, DateTimeKind.Utc).AddTicks(1109) });

            migrationBuilder.UpdateData(
                table: "VehicleAssignments",
                keyColumn: "Id",
                keyValue: new Guid("cccc0000-0000-0000-0000-000000000000"),
                column: "AssignedAt",
                value: new DateTime(2025, 10, 16, 15, 46, 44, 374, DateTimeKind.Utc).AddTicks(1310));

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                column: "CreatedAt",
                value: new DateTime(2025, 9, 28, 15, 46, 44, 374, DateTimeKind.Utc).AddTicks(1267));

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 3, 15, 46, 44, 374, DateTimeKind.Utc).AddTicks(1293));

            migrationBuilder.InsertData(
                table: "ChargingConnectors",
                columns: new[] { "Id", "ChargingPointId", "ConnectorFormat", "ConnectorId", "ConnectorType", "CreatedAt", "IsActive", "LastStatusChange", "MaxCurrent", "MaxPower", "MaxVoltage", "Notes", "PhysicalReference", "PowerType", "Status" },
                values: new object[,]
                {
                    { new Guid("aaaa8888-8888-8888-8888-888888888888"), new Guid("88888888-8888-8888-8888-888888888888"), null, 1, "CCS", new DateTime(2025, 10, 3, 15, 46, 44, 374, DateTimeKind.Utc).AddTicks(1244), true, null, 200, 150, 800, null, null, "DC", 0 },
                    { new Guid("aaaa9999-9999-9999-9999-999999999999"), new Guid("99999999-9999-9999-9999-999999999999"), null, 1, "CCS", new DateTime(2025, 10, 3, 15, 46, 44, 374, DateTimeKind.Utc).AddTicks(1247), true, null, 200, 150, 800, null, null, "DC", 0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChargingPoints_ChargingStationId",
                table: "ChargingPoints",
                column: "ChargingStationId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChargingConnectors_ChargingPoints_ChargingPointId",
                table: "ChargingConnectors",
                column: "ChargingPointId",
                principalTable: "ChargingPoints",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChargingConnectors_ChargingPoints_ChargingPointId",
                table: "ChargingConnectors");

            migrationBuilder.DropTable(
                name: "ChargingPoints");

            migrationBuilder.DeleteData(
                table: "ChargingConnectors",
                keyColumn: "Id",
                keyValue: new Guid("aaaa8888-8888-8888-8888-888888888888"));

            migrationBuilder.DeleteData(
                table: "ChargingConnectors",
                keyColumn: "Id",
                keyValue: new Guid("aaaa9999-9999-9999-9999-999999999999"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("21430b63-7975-444b-9361-0ec5d13283f2"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("2254ecb6-ca87-4fac-b669-cfc6660df267"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("2b8d56cf-2e13-4c65-8f48-db6dbc573804"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("3093227c-cce4-422f-944b-9847186047c4"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("3a546465-2b00-4b93-ad1c-410fe105af9b"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("3d0526f5-fe92-4712-8033-0c1b585d3403"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("4d47d64b-eb0f-45bb-bc2e-b49ccc080474"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("7003cd50-79cb-4ee0-9e2f-9c1310bd2f34"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("79a46c17-ea45-44aa-9e2b-c287d66ac991"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("8a540381-672f-4422-a50c-e8dd55188f99"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ac94e6e3-0cd3-44f2-94f3-2d78041729fd"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("b004d1d3-9f21-4abf-87ac-b9d7c0a8fdc1"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ba333477-5719-4eea-aa99-0ceda3939728"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d41efa8d-7459-4960-ad3b-a2c94255a218"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("f5741eb8-9866-415a-ac48-b350e5f52f85"));

            migrationBuilder.DropColumn(
                name: "ConnectorFormat",
                table: "ChargingConnectors");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ChargingConnectors");

            migrationBuilder.DropColumn(
                name: "LastStatusChange",
                table: "ChargingConnectors");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "ChargingConnectors");

            migrationBuilder.DropColumn(
                name: "PhysicalReference",
                table: "ChargingConnectors");

            migrationBuilder.DropColumn(
                name: "PowerType",
                table: "ChargingConnectors");

            migrationBuilder.RenameColumn(
                name: "ChargingPointId",
                table: "ChargingConnectors",
                newName: "ChargingStationId");

            migrationBuilder.RenameIndex(
                name: "IX_ChargingConnectors_ChargingPointId",
                table: "ChargingConnectors",
                newName: "IX_ChargingConnectors_ChargingStationId");

            migrationBuilder.AddColumn<Guid>(
                name: "ChargingStationId",
                table: "ChargingSessions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "BillingAccounts",
                keyColumn: "Id",
                keyValue: new Guid("dddd1111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2025, 9, 23, 14, 56, 25, 351, DateTimeKind.Utc).AddTicks(8515));

            migrationBuilder.InsertData(
                table: "ChargingConnectors",
                columns: new[] { "Id", "ChargingStationId", "ConnectorId", "ConnectorType", "CreatedAt", "MaxCurrent", "MaxPower", "MaxVoltage", "Status" },
                values: new object[,]
                {
                    { new Guid("88888888-8888-8888-8888-888888888888"), new Guid("44444444-4444-4444-4444-444444444444"), 1, "CCS", new DateTime(2025, 10, 3, 14, 56, 25, 351, DateTimeKind.Utc).AddTicks(8408), 200, 0, 800, 0 },
                    { new Guid("99999999-9999-9999-9999-999999999999"), new Guid("44444444-4444-4444-4444-444444444444"), 2, "CCS", new DateTime(2025, 10, 3, 14, 56, 25, 351, DateTimeKind.Utc).AddTicks(8411), 200, 0, 800, 0 }
                });

            migrationBuilder.UpdateData(
                table: "ChargingParks",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 3, 14, 56, 25, 351, DateTimeKind.Utc).AddTicks(8343));

            migrationBuilder.UpdateData(
                table: "ChargingParks",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 8, 14, 56, 25, 351, DateTimeKind.Utc).AddTicks(8346));

            migrationBuilder.UpdateData(
                table: "ChargingStations",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "LastHeartbeat" },
                values: new object[] { new DateTime(2025, 10, 3, 14, 56, 25, 351, DateTimeKind.Utc).AddTicks(8375), new DateTime(2025, 10, 23, 14, 51, 25, 351, DateTimeKind.Utc).AddTicks(8375) });

            migrationBuilder.UpdateData(
                table: "ChargingStations",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "LastHeartbeat" },
                values: new object[] { new DateTime(2025, 10, 5, 14, 56, 25, 351, DateTimeKind.Utc).AddTicks(8380), new DateTime(2025, 10, 23, 14, 53, 25, 351, DateTimeKind.Utc).AddTicks(8380) });

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

            migrationBuilder.UpdateData(
                table: "QrCodes",
                keyColumn: "Id",
                keyValue: new Guid("eeee2222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "ExpiresAt" },
                values: new object[] { new DateTime(2025, 10, 13, 14, 56, 25, 351, DateTimeKind.Utc).AddTicks(8544), new DateTime(2026, 10, 23, 14, 56, 25, 351, DateTimeKind.Utc).AddTicks(8543) });

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2025, 9, 23, 14, 56, 25, 351, DateTimeKind.Utc).AddTicks(8200));

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("aaaabbbb-1111-2222-3333-444444444444"),
                column: "CreatedAt",
                value: new DateTime(2025, 9, 28, 14, 56, 25, 351, DateTimeKind.Utc).AddTicks(8209));

            migrationBuilder.UpdateData(
                table: "UserGroupMemberships",
                keyColumn: "Id",
                keyValue: new Guid("ccccdddd-1111-2222-3333-444444444444"),
                column: "AssignedAt",
                value: new DateTime(2025, 9, 23, 14, 56, 25, 351, DateTimeKind.Utc).AddTicks(8306));

            migrationBuilder.UpdateData(
                table: "UserGroupMemberships",
                keyColumn: "Id",
                keyValue: new Guid("ccccdddd-2222-3333-4444-555555555555"),
                column: "AssignedAt",
                value: new DateTime(2025, 9, 28, 14, 56, 25, 351, DateTimeKind.Utc).AddTicks(8309));

            migrationBuilder.UpdateData(
                table: "UserGroups",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2025, 9, 23, 14, 56, 25, 351, DateTimeKind.Utc).AddTicks(8240));

            migrationBuilder.UpdateData(
                table: "UserGroups",
                keyColumn: "Id",
                keyValue: new Guid("aaaabbbb-3333-4444-5555-666666666666"),
                column: "CreatedAt",
                value: new DateTime(2025, 9, 28, 14, 56, 25, 351, DateTimeKind.Utc).AddTicks(8241));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "LastLoginAt" },
                values: new object[] { new DateTime(2025, 9, 23, 14, 56, 25, 351, DateTimeKind.Utc).AddTicks(8272), new DateTime(2025, 10, 22, 14, 56, 25, 351, DateTimeKind.Utc).AddTicks(8272) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("aaaabbbb-2222-3333-4444-555555555555"),
                columns: new[] { "CreatedAt", "LastLoginAt" },
                values: new object[] { new DateTime(2025, 9, 28, 14, 56, 25, 351, DateTimeKind.Utc).AddTicks(8278), new DateTime(2025, 10, 21, 14, 56, 25, 351, DateTimeKind.Utc).AddTicks(8278) });

            migrationBuilder.UpdateData(
                table: "VehicleAssignments",
                keyColumn: "Id",
                keyValue: new Guid("cccc0000-0000-0000-0000-000000000000"),
                column: "AssignedAt",
                value: new DateTime(2025, 10, 16, 14, 56, 25, 351, DateTimeKind.Utc).AddTicks(8477));

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                column: "CreatedAt",
                value: new DateTime(2025, 9, 28, 14, 56, 25, 351, DateTimeKind.Utc).AddTicks(8443));

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 3, 14, 56, 25, 351, DateTimeKind.Utc).AddTicks(8444));

            migrationBuilder.CreateIndex(
                name: "IX_ChargingSessions_ChargingStationId",
                table: "ChargingSessions",
                column: "ChargingStationId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChargingConnectors_ChargingStations_ChargingStationId",
                table: "ChargingConnectors",
                column: "ChargingStationId",
                principalTable: "ChargingStations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChargingSessions_ChargingStations_ChargingStationId",
                table: "ChargingSessions",
                column: "ChargingStationId",
                principalTable: "ChargingStations",
                principalColumn: "Id");
        }
    }
}
