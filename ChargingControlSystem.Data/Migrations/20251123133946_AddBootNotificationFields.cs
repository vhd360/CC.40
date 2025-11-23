using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChargingControlSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBootNotificationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirmwareVersion",
                table: "ChargingStations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Iccid",
                table: "ChargingStations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Imsi",
                table: "ChargingStations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MeterSerialNumber",
                table: "ChargingStations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MeterType",
                table: "ChargingStations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SerialNumber",
                table: "ChargingStations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

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
                columns: new[] { "CreatedAt", "FirmwareVersion", "Iccid", "Imsi", "LastHeartbeat", "MeterSerialNumber", "MeterType", "SerialNumber" },
                values: new object[] { new DateTime(2025, 11, 3, 13, 39, 46, 275, DateTimeKind.Utc).AddTicks(4946), null, null, null, new DateTime(2025, 11, 23, 13, 34, 46, 275, DateTimeKind.Utc).AddTicks(4947), null, null, null });

            migrationBuilder.UpdateData(
                table: "ChargingStations",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "FirmwareVersion", "Iccid", "Imsi", "LastHeartbeat", "MeterSerialNumber", "MeterType", "SerialNumber" },
                values: new object[] { new DateTime(2025, 11, 5, 13, 39, 46, 275, DateTimeKind.Utc).AddTicks(4952), null, null, null, new DateTime(2025, 11, 23, 13, 36, 46, 275, DateTimeKind.Utc).AddTicks(4953), null, null, null });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirmwareVersion",
                table: "ChargingStations");

            migrationBuilder.DropColumn(
                name: "Iccid",
                table: "ChargingStations");

            migrationBuilder.DropColumn(
                name: "Imsi",
                table: "ChargingStations");

            migrationBuilder.DropColumn(
                name: "MeterSerialNumber",
                table: "ChargingStations");

            migrationBuilder.DropColumn(
                name: "MeterType",
                table: "ChargingStations");

            migrationBuilder.DropColumn(
                name: "SerialNumber",
                table: "ChargingStations");

            migrationBuilder.UpdateData(
                table: "BillingAccounts",
                keyColumn: "Id",
                keyValue: new Guid("dddd1111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 24, 8, 52, 28, 192, DateTimeKind.Utc).AddTicks(481));

            migrationBuilder.UpdateData(
                table: "ChargingParks",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 8, 52, 28, 192, DateTimeKind.Utc).AddTicks(311));

            migrationBuilder.UpdateData(
                table: "ChargingParks",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 8, 8, 52, 28, 192, DateTimeKind.Utc).AddTicks(315));

            migrationBuilder.UpdateData(
                table: "ChargingPoints",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 8, 52, 28, 192, DateTimeKind.Utc).AddTicks(386));

            migrationBuilder.UpdateData(
                table: "ChargingPoints",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 8, 52, 28, 192, DateTimeKind.Utc).AddTicks(389));

            migrationBuilder.UpdateData(
                table: "ChargingStations",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "LastHeartbeat" },
                values: new object[] { new DateTime(2025, 11, 3, 8, 52, 28, 192, DateTimeKind.Utc).AddTicks(346), new DateTime(2025, 11, 23, 8, 47, 28, 192, DateTimeKind.Utc).AddTicks(347) });

            migrationBuilder.UpdateData(
                table: "ChargingStations",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "LastHeartbeat" },
                values: new object[] { new DateTime(2025, 11, 5, 8, 52, 28, 192, DateTimeKind.Utc).AddTicks(353), new DateTime(2025, 11, 23, 8, 49, 28, 192, DateTimeKind.Utc).AddTicks(353) });

            migrationBuilder.UpdateData(
                table: "QrCodes",
                keyColumn: "Id",
                keyValue: new Guid("eeee2222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "ExpiresAt" },
                values: new object[] { new DateTime(2025, 11, 13, 8, 52, 28, 192, DateTimeKind.Utc).AddTicks(513), new DateTime(2026, 11, 23, 8, 52, 28, 192, DateTimeKind.Utc).AddTicks(512) });

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 24, 8, 52, 28, 192, DateTimeKind.Utc).AddTicks(119));

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("aaaabbbb-1111-2222-3333-444444444444"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 29, 8, 52, 28, 192, DateTimeKind.Utc).AddTicks(153));

            migrationBuilder.UpdateData(
                table: "UserGroupMemberships",
                keyColumn: "Id",
                keyValue: new Guid("ccccdddd-1111-2222-3333-444444444444"),
                column: "AssignedAt",
                value: new DateTime(2025, 10, 24, 8, 52, 28, 192, DateTimeKind.Utc).AddTicks(269));

            migrationBuilder.UpdateData(
                table: "UserGroupMemberships",
                keyColumn: "Id",
                keyValue: new Guid("ccccdddd-2222-3333-4444-555555555555"),
                column: "AssignedAt",
                value: new DateTime(2025, 10, 29, 8, 52, 28, 192, DateTimeKind.Utc).AddTicks(273));

            migrationBuilder.UpdateData(
                table: "UserGroups",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 24, 8, 52, 28, 192, DateTimeKind.Utc).AddTicks(188));

            migrationBuilder.UpdateData(
                table: "UserGroups",
                keyColumn: "Id",
                keyValue: new Guid("aaaabbbb-3333-4444-5555-666666666666"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 29, 8, 52, 28, 192, DateTimeKind.Utc).AddTicks(191));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "LastLoginAt" },
                values: new object[] { new DateTime(2025, 10, 24, 8, 52, 28, 192, DateTimeKind.Utc).AddTicks(232), new DateTime(2025, 11, 22, 8, 52, 28, 192, DateTimeKind.Utc).AddTicks(232) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("aaaabbbb-2222-3333-4444-555555555555"),
                columns: new[] { "CreatedAt", "LastLoginAt" },
                values: new object[] { new DateTime(2025, 10, 29, 8, 52, 28, 192, DateTimeKind.Utc).AddTicks(239), new DateTime(2025, 11, 21, 8, 52, 28, 192, DateTimeKind.Utc).AddTicks(239) });

            migrationBuilder.UpdateData(
                table: "VehicleAssignments",
                keyColumn: "Id",
                keyValue: new Guid("cccc0000-0000-0000-0000-000000000000"),
                column: "AssignedAt",
                value: new DateTime(2025, 11, 16, 8, 52, 28, 192, DateTimeKind.Utc).AddTicks(447));

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 29, 8, 52, 28, 192, DateTimeKind.Utc).AddTicks(419));

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 8, 52, 28, 192, DateTimeKind.Utc).AddTicks(423));
        }
    }
}
