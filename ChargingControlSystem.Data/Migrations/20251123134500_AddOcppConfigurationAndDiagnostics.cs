using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChargingControlSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOcppConfigurationAndDiagnostics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConfigurationJson",
                table: "ChargingStations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirmwareStatus",
                table: "ChargingStations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastConfigurationUpdate",
                table: "ChargingStations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastFirmwareStatusUpdate",
                table: "ChargingStations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ChargingStationDiagnostics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChargingStationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DiagnosticsUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StopTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChargingStationDiagnostics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChargingStationDiagnostics_ChargingStations_ChargingStationId",
                        column: x => x.ChargingStationId,
                        principalTable: "ChargingStations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChargingStationFirmwareHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChargingStationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirmwareVersion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Info = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChargingStationFirmwareHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChargingStationFirmwareHistory_ChargingStations_ChargingStationId",
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
                value: new DateTime(2025, 10, 24, 13, 44, 59, 789, DateTimeKind.Utc).AddTicks(406));

            migrationBuilder.UpdateData(
                table: "ChargingParks",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 13, 44, 59, 789, DateTimeKind.Utc).AddTicks(113));

            migrationBuilder.UpdateData(
                table: "ChargingParks",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 8, 13, 44, 59, 789, DateTimeKind.Utc).AddTicks(119));

            migrationBuilder.UpdateData(
                table: "ChargingPoints",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 13, 44, 59, 789, DateTimeKind.Utc).AddTicks(281));

            migrationBuilder.UpdateData(
                table: "ChargingPoints",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 13, 44, 59, 789, DateTimeKind.Utc).AddTicks(284));

            migrationBuilder.UpdateData(
                table: "ChargingStations",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "ConfigurationJson", "CreatedAt", "FirmwareStatus", "LastConfigurationUpdate", "LastFirmwareStatusUpdate", "LastHeartbeat" },
                values: new object[] { null, new DateTime(2025, 11, 3, 13, 44, 59, 789, DateTimeKind.Utc).AddTicks(167), null, null, null, new DateTime(2025, 11, 23, 13, 39, 59, 789, DateTimeKind.Utc).AddTicks(167) });

            migrationBuilder.UpdateData(
                table: "ChargingStations",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "ConfigurationJson", "CreatedAt", "FirmwareStatus", "LastConfigurationUpdate", "LastFirmwareStatusUpdate", "LastHeartbeat" },
                values: new object[] { null, new DateTime(2025, 11, 5, 13, 44, 59, 789, DateTimeKind.Utc).AddTicks(238), null, null, null, new DateTime(2025, 11, 23, 13, 41, 59, 789, DateTimeKind.Utc).AddTicks(238) });

            migrationBuilder.UpdateData(
                table: "QrCodes",
                keyColumn: "Id",
                keyValue: new Guid("eeee2222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "ExpiresAt" },
                values: new object[] { new DateTime(2025, 11, 13, 13, 44, 59, 789, DateTimeKind.Utc).AddTicks(444), new DateTime(2026, 11, 23, 13, 44, 59, 789, DateTimeKind.Utc).AddTicks(442) });

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 24, 13, 44, 59, 788, DateTimeKind.Utc).AddTicks(9919));

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("aaaabbbb-1111-2222-3333-444444444444"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 29, 13, 44, 59, 788, DateTimeKind.Utc).AddTicks(9930));

            migrationBuilder.UpdateData(
                table: "UserGroupMemberships",
                keyColumn: "Id",
                keyValue: new Guid("ccccdddd-1111-2222-3333-444444444444"),
                column: "AssignedAt",
                value: new DateTime(2025, 10, 24, 13, 44, 59, 789, DateTimeKind.Utc).AddTicks(65));

            migrationBuilder.UpdateData(
                table: "UserGroupMemberships",
                keyColumn: "Id",
                keyValue: new Guid("ccccdddd-2222-3333-4444-555555555555"),
                column: "AssignedAt",
                value: new DateTime(2025, 10, 29, 13, 44, 59, 789, DateTimeKind.Utc).AddTicks(68));

            migrationBuilder.UpdateData(
                table: "UserGroups",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 24, 13, 44, 59, 788, DateTimeKind.Utc).AddTicks(9970));

            migrationBuilder.UpdateData(
                table: "UserGroups",
                keyColumn: "Id",
                keyValue: new Guid("aaaabbbb-3333-4444-5555-666666666666"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 29, 13, 44, 59, 788, DateTimeKind.Utc).AddTicks(9973));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "LastLoginAt" },
                values: new object[] { new DateTime(2025, 10, 24, 13, 44, 59, 789, DateTimeKind.Utc).AddTicks(19), new DateTime(2025, 11, 22, 13, 44, 59, 789, DateTimeKind.Utc).AddTicks(20) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("aaaabbbb-2222-3333-4444-555555555555"),
                columns: new[] { "CreatedAt", "LastLoginAt" },
                values: new object[] { new DateTime(2025, 10, 29, 13, 44, 59, 789, DateTimeKind.Utc).AddTicks(27), new DateTime(2025, 11, 21, 13, 44, 59, 789, DateTimeKind.Utc).AddTicks(27) });

            migrationBuilder.UpdateData(
                table: "VehicleAssignments",
                keyColumn: "Id",
                keyValue: new Guid("cccc0000-0000-0000-0000-000000000000"),
                column: "AssignedAt",
                value: new DateTime(2025, 11, 16, 13, 44, 59, 789, DateTimeKind.Utc).AddTicks(361));

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 29, 13, 44, 59, 789, DateTimeKind.Utc).AddTicks(325));

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 13, 44, 59, 789, DateTimeKind.Utc).AddTicks(328));

            migrationBuilder.CreateIndex(
                name: "IX_ChargingStationDiagnostics_ChargingStationId",
                table: "ChargingStationDiagnostics",
                column: "ChargingStationId");

            migrationBuilder.CreateIndex(
                name: "IX_ChargingStationFirmwareHistory_ChargingStationId",
                table: "ChargingStationFirmwareHistory",
                column: "ChargingStationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChargingStationDiagnostics");

            migrationBuilder.DropTable(
                name: "ChargingStationFirmwareHistory");

            migrationBuilder.DropColumn(
                name: "ConfigurationJson",
                table: "ChargingStations");

            migrationBuilder.DropColumn(
                name: "FirmwareStatus",
                table: "ChargingStations");

            migrationBuilder.DropColumn(
                name: "LastConfigurationUpdate",
                table: "ChargingStations");

            migrationBuilder.DropColumn(
                name: "LastFirmwareStatusUpdate",
                table: "ChargingStations");

            migrationBuilder.UpdateData(
                table: "BillingAccounts",
                keyColumn: "Id",
                keyValue: new Guid("dddd1111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 24, 13, 39, 46, 275, DateTimeKind.Utc).AddTicks(5119));

            migrationBuilder.UpdateData(
                table: "ChargingParks",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 13, 39, 46, 275, DateTimeKind.Utc).AddTicks(4902));

            migrationBuilder.UpdateData(
                table: "ChargingParks",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 8, 13, 39, 46, 275, DateTimeKind.Utc).AddTicks(4907));

            migrationBuilder.UpdateData(
                table: "ChargingPoints",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 13, 39, 46, 275, DateTimeKind.Utc).AddTicks(4991));

            migrationBuilder.UpdateData(
                table: "ChargingPoints",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 13, 39, 46, 275, DateTimeKind.Utc).AddTicks(4994));

            migrationBuilder.UpdateData(
                table: "ChargingStations",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "LastHeartbeat" },
                values: new object[] { new DateTime(2025, 11, 3, 13, 39, 46, 275, DateTimeKind.Utc).AddTicks(4946), new DateTime(2025, 11, 23, 13, 34, 46, 275, DateTimeKind.Utc).AddTicks(4947) });

            migrationBuilder.UpdateData(
                table: "ChargingStations",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "LastHeartbeat" },
                values: new object[] { new DateTime(2025, 11, 5, 13, 39, 46, 275, DateTimeKind.Utc).AddTicks(4952), new DateTime(2025, 11, 23, 13, 36, 46, 275, DateTimeKind.Utc).AddTicks(4953) });

            migrationBuilder.UpdateData(
                table: "QrCodes",
                keyColumn: "Id",
                keyValue: new Guid("eeee2222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "ExpiresAt" },
                values: new object[] { new DateTime(2025, 11, 13, 13, 39, 46, 275, DateTimeKind.Utc).AddTicks(5150), new DateTime(2026, 11, 23, 13, 39, 46, 275, DateTimeKind.Utc).AddTicks(5148) });

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 24, 13, 39, 46, 275, DateTimeKind.Utc).AddTicks(4730));

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("aaaabbbb-1111-2222-3333-444444444444"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 29, 13, 39, 46, 275, DateTimeKind.Utc).AddTicks(4740));

            migrationBuilder.UpdateData(
                table: "UserGroupMemberships",
                keyColumn: "Id",
                keyValue: new Guid("ccccdddd-1111-2222-3333-444444444444"),
                column: "AssignedAt",
                value: new DateTime(2025, 10, 24, 13, 39, 46, 275, DateTimeKind.Utc).AddTicks(4863));

            migrationBuilder.UpdateData(
                table: "UserGroupMemberships",
                keyColumn: "Id",
                keyValue: new Guid("ccccdddd-2222-3333-4444-555555555555"),
                column: "AssignedAt",
                value: new DateTime(2025, 10, 29, 13, 39, 46, 275, DateTimeKind.Utc).AddTicks(4866));

            migrationBuilder.UpdateData(
                table: "UserGroups",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 24, 13, 39, 46, 275, DateTimeKind.Utc).AddTicks(4784));

            migrationBuilder.UpdateData(
                table: "UserGroups",
                keyColumn: "Id",
                keyValue: new Guid("aaaabbbb-3333-4444-5555-666666666666"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 29, 13, 39, 46, 275, DateTimeKind.Utc).AddTicks(4786));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "LastLoginAt" },
                values: new object[] { new DateTime(2025, 10, 24, 13, 39, 46, 275, DateTimeKind.Utc).AddTicks(4822), new DateTime(2025, 11, 22, 13, 39, 46, 275, DateTimeKind.Utc).AddTicks(4822) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("aaaabbbb-2222-3333-4444-555555555555"),
                columns: new[] { "CreatedAt", "LastLoginAt" },
                values: new object[] { new DateTime(2025, 10, 29, 13, 39, 46, 275, DateTimeKind.Utc).AddTicks(4829), new DateTime(2025, 11, 21, 13, 39, 46, 275, DateTimeKind.Utc).AddTicks(4830) });

            migrationBuilder.UpdateData(
                table: "VehicleAssignments",
                keyColumn: "Id",
                keyValue: new Guid("cccc0000-0000-0000-0000-000000000000"),
                column: "AssignedAt",
                value: new DateTime(2025, 11, 16, 13, 39, 46, 275, DateTimeKind.Utc).AddTicks(5058));

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 29, 13, 39, 46, 275, DateTimeKind.Utc).AddTicks(5029));

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 13, 39, 46, 275, DateTimeKind.Utc).AddTicks(5033));
        }
    }
}
