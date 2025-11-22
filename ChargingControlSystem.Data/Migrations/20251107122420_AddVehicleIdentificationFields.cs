using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChargingControlSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleIdentificationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "QrCode",
                table: "Vehicles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RfidTag",
                table: "Vehicles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "BillingAccounts",
                keyColumn: "Id",
                keyValue: new Guid("dddd1111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 8, 12, 24, 19, 817, DateTimeKind.Utc).AddTicks(9421));

            migrationBuilder.UpdateData(
                table: "ChargingConnectors",
                keyColumn: "Id",
                keyValue: new Guid("aaaa8888-8888-8888-8888-888888888888"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 18, 12, 24, 19, 817, DateTimeKind.Utc).AddTicks(9309));

            migrationBuilder.UpdateData(
                table: "ChargingConnectors",
                keyColumn: "Id",
                keyValue: new Guid("aaaa9999-9999-9999-9999-999999999999"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 18, 12, 24, 19, 817, DateTimeKind.Utc).AddTicks(9311));

            migrationBuilder.UpdateData(
                table: "ChargingParks",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 18, 12, 24, 19, 817, DateTimeKind.Utc).AddTicks(9208));

            migrationBuilder.UpdateData(
                table: "ChargingParks",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 23, 12, 24, 19, 817, DateTimeKind.Utc).AddTicks(9212));

            migrationBuilder.UpdateData(
                table: "ChargingPoints",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 18, 12, 24, 19, 817, DateTimeKind.Utc).AddTicks(9283));

            migrationBuilder.UpdateData(
                table: "ChargingPoints",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 18, 12, 24, 19, 817, DateTimeKind.Utc).AddTicks(9284));

            migrationBuilder.UpdateData(
                table: "ChargingStations",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "LastHeartbeat" },
                values: new object[] { new DateTime(2025, 10, 18, 12, 24, 19, 817, DateTimeKind.Utc).AddTicks(9244), new DateTime(2025, 11, 7, 12, 19, 19, 817, DateTimeKind.Utc).AddTicks(9245) });

            migrationBuilder.UpdateData(
                table: "ChargingStations",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "LastHeartbeat" },
                values: new object[] { new DateTime(2025, 10, 20, 12, 24, 19, 817, DateTimeKind.Utc).AddTicks(9249), new DateTime(2025, 11, 7, 12, 21, 19, 817, DateTimeKind.Utc).AddTicks(9250) });

            migrationBuilder.UpdateData(
                table: "QrCodes",
                keyColumn: "Id",
                keyValue: new Guid("eeee2222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "ExpiresAt" },
                values: new object[] { new DateTime(2025, 10, 28, 12, 24, 19, 817, DateTimeKind.Utc).AddTicks(9448), new DateTime(2026, 11, 7, 12, 24, 19, 817, DateTimeKind.Utc).AddTicks(9446) });

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 8, 12, 24, 19, 817, DateTimeKind.Utc).AddTicks(9067));

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("aaaabbbb-1111-2222-3333-444444444444"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 13, 12, 24, 19, 817, DateTimeKind.Utc).AddTicks(9077));

            migrationBuilder.UpdateData(
                table: "UserGroupMemberships",
                keyColumn: "Id",
                keyValue: new Guid("ccccdddd-1111-2222-3333-444444444444"),
                column: "AssignedAt",
                value: new DateTime(2025, 10, 8, 12, 24, 19, 817, DateTimeKind.Utc).AddTicks(9167));

            migrationBuilder.UpdateData(
                table: "UserGroupMemberships",
                keyColumn: "Id",
                keyValue: new Guid("ccccdddd-2222-3333-4444-555555555555"),
                column: "AssignedAt",
                value: new DateTime(2025, 10, 13, 12, 24, 19, 817, DateTimeKind.Utc).AddTicks(9171));

            migrationBuilder.UpdateData(
                table: "UserGroups",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 8, 12, 24, 19, 817, DateTimeKind.Utc).AddTicks(9103));

            migrationBuilder.UpdateData(
                table: "UserGroups",
                keyColumn: "Id",
                keyValue: new Guid("aaaabbbb-3333-4444-5555-666666666666"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 13, 12, 24, 19, 817, DateTimeKind.Utc).AddTicks(9104));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "LastLoginAt" },
                values: new object[] { new DateTime(2025, 10, 8, 12, 24, 19, 817, DateTimeKind.Utc).AddTicks(9134), new DateTime(2025, 11, 6, 12, 24, 19, 817, DateTimeKind.Utc).AddTicks(9134) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("aaaabbbb-2222-3333-4444-555555555555"),
                columns: new[] { "CreatedAt", "LastLoginAt" },
                values: new object[] { new DateTime(2025, 10, 13, 12, 24, 19, 817, DateTimeKind.Utc).AddTicks(9142), new DateTime(2025, 11, 5, 12, 24, 19, 817, DateTimeKind.Utc).AddTicks(9142) });

            migrationBuilder.UpdateData(
                table: "VehicleAssignments",
                keyColumn: "Id",
                keyValue: new Guid("cccc0000-0000-0000-0000-000000000000"),
                column: "AssignedAt",
                value: new DateTime(2025, 10, 31, 12, 24, 19, 817, DateTimeKind.Utc).AddTicks(9393));

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "QrCode", "RfidTag" },
                values: new object[] { new DateTime(2025, 10, 13, 12, 24, 19, 817, DateTimeKind.Utc).AddTicks(9370), null, null });

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "QrCode", "RfidTag" },
                values: new object[] { new DateTime(2025, 10, 18, 12, 24, 19, 817, DateTimeKind.Utc).AddTicks(9372), null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QrCode",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "RfidTag",
                table: "Vehicles");

            migrationBuilder.UpdateData(
                table: "BillingAccounts",
                keyColumn: "Id",
                keyValue: new Guid("dddd1111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 7, 11, 40, 48, 443, DateTimeKind.Utc).AddTicks(6524));

            migrationBuilder.UpdateData(
                table: "ChargingConnectors",
                keyColumn: "Id",
                keyValue: new Guid("aaaa8888-8888-8888-8888-888888888888"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 17, 11, 40, 48, 443, DateTimeKind.Utc).AddTicks(6394));

            migrationBuilder.UpdateData(
                table: "ChargingConnectors",
                keyColumn: "Id",
                keyValue: new Guid("aaaa9999-9999-9999-9999-999999999999"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 17, 11, 40, 48, 443, DateTimeKind.Utc).AddTicks(6398));

            migrationBuilder.UpdateData(
                table: "ChargingParks",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 17, 11, 40, 48, 443, DateTimeKind.Utc).AddTicks(6208));

            migrationBuilder.UpdateData(
                table: "ChargingParks",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 22, 11, 40, 48, 443, DateTimeKind.Utc).AddTicks(6216));

            migrationBuilder.UpdateData(
                table: "ChargingPoints",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 17, 11, 40, 48, 443, DateTimeKind.Utc).AddTicks(6342));

            migrationBuilder.UpdateData(
                table: "ChargingPoints",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 17, 11, 40, 48, 443, DateTimeKind.Utc).AddTicks(6348));

            migrationBuilder.UpdateData(
                table: "ChargingStations",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "LastHeartbeat" },
                values: new object[] { new DateTime(2025, 10, 17, 11, 40, 48, 443, DateTimeKind.Utc).AddTicks(6272), new DateTime(2025, 11, 6, 11, 35, 48, 443, DateTimeKind.Utc).AddTicks(6274) });

            migrationBuilder.UpdateData(
                table: "ChargingStations",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "LastHeartbeat" },
                values: new object[] { new DateTime(2025, 10, 19, 11, 40, 48, 443, DateTimeKind.Utc).AddTicks(6283), new DateTime(2025, 11, 6, 11, 37, 48, 443, DateTimeKind.Utc).AddTicks(6284) });

            migrationBuilder.UpdateData(
                table: "QrCodes",
                keyColumn: "Id",
                keyValue: new Guid("eeee2222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "ExpiresAt" },
                values: new object[] { new DateTime(2025, 10, 27, 11, 40, 48, 443, DateTimeKind.Utc).AddTicks(6573), new DateTime(2026, 11, 6, 11, 40, 48, 443, DateTimeKind.Utc).AddTicks(6565) });

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 7, 11, 40, 48, 443, DateTimeKind.Utc).AddTicks(5881));

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("aaaabbbb-1111-2222-3333-444444444444"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 12, 11, 40, 48, 443, DateTimeKind.Utc).AddTicks(5897));

            migrationBuilder.UpdateData(
                table: "UserGroupMemberships",
                keyColumn: "Id",
                keyValue: new Guid("ccccdddd-1111-2222-3333-444444444444"),
                column: "AssignedAt",
                value: new DateTime(2025, 10, 7, 11, 40, 48, 443, DateTimeKind.Utc).AddTicks(6143));

            migrationBuilder.UpdateData(
                table: "UserGroupMemberships",
                keyColumn: "Id",
                keyValue: new Guid("ccccdddd-2222-3333-4444-555555555555"),
                column: "AssignedAt",
                value: new DateTime(2025, 10, 12, 11, 40, 48, 443, DateTimeKind.Utc).AddTicks(6150));

            migrationBuilder.UpdateData(
                table: "UserGroups",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 7, 11, 40, 48, 443, DateTimeKind.Utc).AddTicks(5940));

            migrationBuilder.UpdateData(
                table: "UserGroups",
                keyColumn: "Id",
                keyValue: new Guid("aaaabbbb-3333-4444-5555-666666666666"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 12, 11, 40, 48, 443, DateTimeKind.Utc).AddTicks(5946));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "LastLoginAt" },
                values: new object[] { new DateTime(2025, 10, 7, 11, 40, 48, 443, DateTimeKind.Utc).AddTicks(5989), new DateTime(2025, 11, 5, 11, 40, 48, 443, DateTimeKind.Utc).AddTicks(5990) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("aaaabbbb-2222-3333-4444-555555555555"),
                columns: new[] { "CreatedAt", "LastLoginAt" },
                values: new object[] { new DateTime(2025, 10, 12, 11, 40, 48, 443, DateTimeKind.Utc).AddTicks(6110), new DateTime(2025, 11, 4, 11, 40, 48, 443, DateTimeKind.Utc).AddTicks(6110) });

            migrationBuilder.UpdateData(
                table: "VehicleAssignments",
                keyColumn: "Id",
                keyValue: new Guid("cccc0000-0000-0000-0000-000000000000"),
                column: "AssignedAt",
                value: new DateTime(2025, 10, 30, 11, 40, 48, 443, DateTimeKind.Utc).AddTicks(6475));

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 12, 11, 40, 48, 443, DateTimeKind.Utc).AddTicks(6443));

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 17, 11, 40, 48, 443, DateTimeKind.Utc).AddTicks(6447));
        }
    }
}
