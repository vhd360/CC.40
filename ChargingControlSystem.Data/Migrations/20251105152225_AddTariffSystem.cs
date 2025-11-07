using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ChargingControlSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTariffSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // No need to delete existing permissions - they were already updated with fixed GUIDs

            migrationBuilder.CreateTable(
                name: "Tariffs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ValidUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tariffs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tariffs_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TariffComponents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TariffId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,4)", nullable: false),
                    StepSize = table.Column<int>(type: "int", nullable: true),
                    TimeStart = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    TimeEnd = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    DaysOfWeek = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MinimumCharge = table.Column<decimal>(type: "decimal(10,4)", nullable: true),
                    MaximumCharge = table.Column<decimal>(type: "decimal(10,4)", nullable: true),
                    GracePeriodMinutes = table.Column<int>(type: "int", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TariffComponents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TariffComponents_Tariffs_TariffId",
                        column: x => x.TariffId,
                        principalTable: "Tariffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserGroupTariffs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TariffId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGroupTariffs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserGroupTariffs_Tariffs_TariffId",
                        column: x => x.TariffId,
                        principalTable: "Tariffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserGroupTariffs_UserGroups_UserGroupId",
                        column: x => x.UserGroupId,
                        principalTable: "UserGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "BillingAccounts",
                keyColumn: "Id",
                keyValue: new Guid("dddd1111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 6, 15, 22, 25, 497, DateTimeKind.Utc).AddTicks(823));

            migrationBuilder.UpdateData(
                table: "ChargingConnectors",
                keyColumn: "Id",
                keyValue: new Guid("aaaa8888-8888-8888-8888-888888888888"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 16, 15, 22, 25, 497, DateTimeKind.Utc).AddTicks(708));

            migrationBuilder.UpdateData(
                table: "ChargingConnectors",
                keyColumn: "Id",
                keyValue: new Guid("aaaa9999-9999-9999-9999-999999999999"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 16, 15, 22, 25, 497, DateTimeKind.Utc).AddTicks(711));

            migrationBuilder.UpdateData(
                table: "ChargingParks",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 16, 15, 22, 25, 497, DateTimeKind.Utc).AddTicks(589));

            migrationBuilder.UpdateData(
                table: "ChargingParks",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 21, 15, 22, 25, 497, DateTimeKind.Utc).AddTicks(594));

            migrationBuilder.UpdateData(
                table: "ChargingPoints",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 16, 15, 22, 25, 497, DateTimeKind.Utc).AddTicks(672));

            migrationBuilder.UpdateData(
                table: "ChargingPoints",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 16, 15, 22, 25, 497, DateTimeKind.Utc).AddTicks(675));

            migrationBuilder.UpdateData(
                table: "ChargingStations",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "LastHeartbeat" },
                values: new object[] { new DateTime(2025, 10, 16, 15, 22, 25, 497, DateTimeKind.Utc).AddTicks(631), new DateTime(2025, 11, 5, 15, 17, 25, 497, DateTimeKind.Utc).AddTicks(632) });

            migrationBuilder.UpdateData(
                table: "ChargingStations",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "LastHeartbeat" },
                values: new object[] { new DateTime(2025, 10, 18, 15, 22, 25, 497, DateTimeKind.Utc).AddTicks(639), new DateTime(2025, 11, 5, 15, 19, 25, 497, DateTimeKind.Utc).AddTicks(639) });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Action", "Description", "Name", "Resource" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), "view", "View users", "users.view", "users" },
                    { new Guid("10000000-0000-0000-0000-000000000002"), "create", "Create users", "users.create", "users" },
                    { new Guid("10000000-0000-0000-0000-000000000003"), "edit", "Edit users", "users.edit", "users" },
                    { new Guid("10000000-0000-0000-0000-000000000004"), "delete", "Delete users", "users.delete", "users" },
                    { new Guid("10000000-0000-0000-0000-000000000005"), "view", "View vehicles", "vehicles.view", "vehicles" },
                    { new Guid("10000000-0000-0000-0000-000000000006"), "create", "Create vehicles", "vehicles.create", "vehicles" },
                    { new Guid("10000000-0000-0000-0000-000000000007"), "edit", "Edit vehicles", "vehicles.edit", "vehicles" },
                    { new Guid("10000000-0000-0000-0000-000000000008"), "assign", "Assign vehicles", "vehicles.assign", "vehicles" },
                    { new Guid("10000000-0000-0000-0000-000000000009"), "view", "View charging infrastructure", "charging.view", "charging" },
                    { new Guid("10000000-0000-0000-0000-00000000000a"), "manage", "Manage charging infrastructure", "charging.manage", "charging" },
                    { new Guid("10000000-0000-0000-0000-00000000000b"), "view", "View billing information", "billing.view", "billing" },
                    { new Guid("10000000-0000-0000-0000-00000000000c"), "manage", "Manage billing", "billing.manage", "billing" },
                    { new Guid("10000000-0000-0000-0000-00000000000d"), "view", "View QR codes", "qrcodes.view", "qrcodes" },
                    { new Guid("10000000-0000-0000-0000-00000000000e"), "create", "Create QR codes", "qrcodes.create", "qrcodes" },
                    { new Guid("10000000-0000-0000-0000-00000000000f"), "manage", "Manage QR codes", "qrcodes.manage", "qrcodes" },
                    { new Guid("10000000-0000-0000-0000-000000000010"), "view", "View tariffs", "tariffs.view", "tariffs" },
                    { new Guid("10000000-0000-0000-0000-000000000011"), "create", "Create tariffs", "tariffs.create", "tariffs" },
                    { new Guid("10000000-0000-0000-0000-000000000012"), "edit", "Edit tariffs", "tariffs.edit", "tariffs" },
                    { new Guid("10000000-0000-0000-0000-000000000013"), "delete", "Delete tariffs", "tariffs.delete", "tariffs" }
                });

            migrationBuilder.UpdateData(
                table: "QrCodes",
                keyColumn: "Id",
                keyValue: new Guid("eeee2222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "ExpiresAt" },
                values: new object[] { new DateTime(2025, 10, 26, 15, 22, 25, 497, DateTimeKind.Utc).AddTicks(856), new DateTime(2026, 11, 5, 15, 22, 25, 497, DateTimeKind.Utc).AddTicks(851) });

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 6, 15, 22, 25, 497, DateTimeKind.Utc).AddTicks(443));

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("aaaabbbb-1111-2222-3333-444444444444"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 11, 15, 22, 25, 497, DateTimeKind.Utc).AddTicks(453));

            migrationBuilder.UpdateData(
                table: "UserGroupMemberships",
                keyColumn: "Id",
                keyValue: new Guid("ccccdddd-1111-2222-3333-444444444444"),
                column: "AssignedAt",
                value: new DateTime(2025, 10, 6, 15, 22, 25, 497, DateTimeKind.Utc).AddTicks(549));

            migrationBuilder.UpdateData(
                table: "UserGroupMemberships",
                keyColumn: "Id",
                keyValue: new Guid("ccccdddd-2222-3333-4444-555555555555"),
                column: "AssignedAt",
                value: new DateTime(2025, 10, 11, 15, 22, 25, 497, DateTimeKind.Utc).AddTicks(553));

            migrationBuilder.UpdateData(
                table: "UserGroups",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 6, 15, 22, 25, 497, DateTimeKind.Utc).AddTicks(482));

            migrationBuilder.UpdateData(
                table: "UserGroups",
                keyColumn: "Id",
                keyValue: new Guid("aaaabbbb-3333-4444-5555-666666666666"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 11, 15, 22, 25, 497, DateTimeKind.Utc).AddTicks(486));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "LastLoginAt" },
                values: new object[] { new DateTime(2025, 10, 6, 15, 22, 25, 497, DateTimeKind.Utc).AddTicks(516), new DateTime(2025, 11, 4, 15, 22, 25, 497, DateTimeKind.Utc).AddTicks(517) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("aaaabbbb-2222-3333-4444-555555555555"),
                columns: new[] { "CreatedAt", "LastLoginAt" },
                values: new object[] { new DateTime(2025, 10, 11, 15, 22, 25, 497, DateTimeKind.Utc).AddTicks(526), new DateTime(2025, 11, 3, 15, 22, 25, 497, DateTimeKind.Utc).AddTicks(526) });

            migrationBuilder.UpdateData(
                table: "VehicleAssignments",
                keyColumn: "Id",
                keyValue: new Guid("cccc0000-0000-0000-0000-000000000000"),
                column: "AssignedAt",
                value: new DateTime(2025, 10, 29, 15, 22, 25, 497, DateTimeKind.Utc).AddTicks(764));

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 11, 15, 22, 25, 497, DateTimeKind.Utc).AddTicks(741));

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 16, 15, 22, 25, 497, DateTimeKind.Utc).AddTicks(743));

            migrationBuilder.CreateIndex(
                name: "IX_TariffComponents_TariffId",
                table: "TariffComponents",
                column: "TariffId");

            migrationBuilder.CreateIndex(
                name: "IX_Tariffs_TenantId_Name",
                table: "Tariffs",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserGroupTariffs_TariffId",
                table: "UserGroupTariffs",
                column: "TariffId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroupTariffs_UserGroupId_TariffId",
                table: "UserGroupTariffs",
                columns: new[] { "UserGroupId", "TariffId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TariffComponents");

            migrationBuilder.DropTable(
                name: "UserGroupTariffs");

            migrationBuilder.DropTable(
                name: "Tariffs");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-00000000000a"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-00000000000b"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-00000000000c"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-00000000000d"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-00000000000e"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-00000000000f"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000011"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000012"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000013"));

            migrationBuilder.UpdateData(
                table: "BillingAccounts",
                keyColumn: "Id",
                keyValue: new Guid("dddd1111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2025, 9, 23, 15, 46, 44, 374, DateTimeKind.Utc).AddTicks(1334));

            migrationBuilder.UpdateData(
                table: "ChargingConnectors",
                keyColumn: "Id",
                keyValue: new Guid("aaaa8888-8888-8888-8888-888888888888"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 3, 15, 46, 44, 374, DateTimeKind.Utc).AddTicks(1244));

            migrationBuilder.UpdateData(
                table: "ChargingConnectors",
                keyColumn: "Id",
                keyValue: new Guid("aaaa9999-9999-9999-9999-999999999999"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 3, 15, 46, 44, 374, DateTimeKind.Utc).AddTicks(1247));

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

            migrationBuilder.UpdateData(
                table: "ChargingPoints",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 3, 15, 46, 44, 374, DateTimeKind.Utc).AddTicks(1220));

            migrationBuilder.UpdateData(
                table: "ChargingPoints",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 3, 15, 46, 44, 374, DateTimeKind.Utc).AddTicks(1222));

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

            // Insert only NEW tariff permissions
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Action", "Description", "Name", "Resource" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000010"), "view", "View tariffs", "tariffs.view", "tariffs" },
                    { new Guid("10000000-0000-0000-0000-000000000011"), "create", "Create tariffs", "tariffs.create", "tariffs" },
                    { new Guid("10000000-0000-0000-0000-000000000012"), "edit", "Edit tariffs", "tariffs.edit", "tariffs" },
                    { new Guid("10000000-0000-0000-0000-000000000013"), "delete", "Delete tariffs", "tariffs.delete", "tariffs" }
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
        }
    }
}
