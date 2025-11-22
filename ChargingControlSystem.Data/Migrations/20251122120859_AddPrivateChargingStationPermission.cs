using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChargingControlSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPrivateChargingStationPermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "BillingAccounts",
                keyColumn: "Id",
                keyValue: new Guid("dddd1111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 23, 12, 8, 59, 330, DateTimeKind.Utc).AddTicks(7410));

            migrationBuilder.UpdateData(
                table: "ChargingConnectors",
                keyColumn: "Id",
                keyValue: new Guid("aaaa8888-8888-8888-8888-888888888888"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 2, 12, 8, 59, 330, DateTimeKind.Utc).AddTicks(7301));

            migrationBuilder.UpdateData(
                table: "ChargingConnectors",
                keyColumn: "Id",
                keyValue: new Guid("aaaa9999-9999-9999-9999-999999999999"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 2, 12, 8, 59, 330, DateTimeKind.Utc).AddTicks(7305));

            migrationBuilder.UpdateData(
                table: "ChargingParks",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 2, 12, 8, 59, 330, DateTimeKind.Utc).AddTicks(7131));

            migrationBuilder.UpdateData(
                table: "ChargingParks",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 7, 12, 8, 59, 330, DateTimeKind.Utc).AddTicks(7137));

            migrationBuilder.UpdateData(
                table: "ChargingPoints",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 2, 12, 8, 59, 330, DateTimeKind.Utc).AddTicks(7226));

            migrationBuilder.UpdateData(
                table: "ChargingPoints",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 2, 12, 8, 59, 330, DateTimeKind.Utc).AddTicks(7264));

            migrationBuilder.UpdateData(
                table: "ChargingStations",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "LastHeartbeat" },
                values: new object[] { new DateTime(2025, 11, 2, 12, 8, 59, 330, DateTimeKind.Utc).AddTicks(7175), new DateTime(2025, 11, 22, 12, 3, 59, 330, DateTimeKind.Utc).AddTicks(7176) });

            migrationBuilder.UpdateData(
                table: "ChargingStations",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "LastHeartbeat" },
                values: new object[] { new DateTime(2025, 11, 4, 12, 8, 59, 330, DateTimeKind.Utc).AddTicks(7181), new DateTime(2025, 11, 22, 12, 5, 59, 330, DateTimeKind.Utc).AddTicks(7182) });

            migrationBuilder.UpdateData(
                table: "QrCodes",
                keyColumn: "Id",
                keyValue: new Guid("eeee2222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "ExpiresAt" },
                values: new object[] { new DateTime(2025, 11, 12, 12, 8, 59, 330, DateTimeKind.Utc).AddTicks(7443), new DateTime(2026, 11, 22, 12, 8, 59, 330, DateTimeKind.Utc).AddTicks(7442) });

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 23, 12, 8, 59, 330, DateTimeKind.Utc).AddTicks(6940));

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("aaaabbbb-1111-2222-3333-444444444444"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 28, 12, 8, 59, 330, DateTimeKind.Utc).AddTicks(6951));

            migrationBuilder.UpdateData(
                table: "UserGroupMemberships",
                keyColumn: "Id",
                keyValue: new Guid("ccccdddd-1111-2222-3333-444444444444"),
                column: "AssignedAt",
                value: new DateTime(2025, 10, 23, 12, 8, 59, 330, DateTimeKind.Utc).AddTicks(7083));

            migrationBuilder.UpdateData(
                table: "UserGroupMemberships",
                keyColumn: "Id",
                keyValue: new Guid("ccccdddd-2222-3333-4444-555555555555"),
                column: "AssignedAt",
                value: new DateTime(2025, 10, 28, 12, 8, 59, 330, DateTimeKind.Utc).AddTicks(7087));

            migrationBuilder.UpdateData(
                table: "UserGroups",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 23, 12, 8, 59, 330, DateTimeKind.Utc).AddTicks(6992));

            migrationBuilder.UpdateData(
                table: "UserGroups",
                keyColumn: "Id",
                keyValue: new Guid("aaaabbbb-3333-4444-5555-666666666666"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 28, 12, 8, 59, 330, DateTimeKind.Utc).AddTicks(6995));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "LastLoginAt" },
                values: new object[] { new DateTime(2025, 10, 23, 12, 8, 59, 330, DateTimeKind.Utc).AddTicks(7040), new DateTime(2025, 11, 21, 12, 8, 59, 330, DateTimeKind.Utc).AddTicks(7041) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("aaaabbbb-2222-3333-4444-555555555555"),
                columns: new[] { "CreatedAt", "LastLoginAt" },
                values: new object[] { new DateTime(2025, 10, 28, 12, 8, 59, 330, DateTimeKind.Utc).AddTicks(7047), new DateTime(2025, 11, 20, 12, 8, 59, 330, DateTimeKind.Utc).AddTicks(7048) });

            migrationBuilder.UpdateData(
                table: "VehicleAssignments",
                keyColumn: "Id",
                keyValue: new Guid("cccc0000-0000-0000-0000-000000000000"),
                column: "AssignedAt",
                value: new DateTime(2025, 11, 15, 12, 8, 59, 330, DateTimeKind.Utc).AddTicks(7372));

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 28, 12, 8, 59, 330, DateTimeKind.Utc).AddTicks(7341));

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 2, 12, 8, 59, 330, DateTimeKind.Utc).AddTicks(7344));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "BillingAccounts",
                keyColumn: "Id",
                keyValue: new Guid("dddd1111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 23, 12, 8, 21, 160, DateTimeKind.Utc).AddTicks(7182));

            migrationBuilder.UpdateData(
                table: "ChargingConnectors",
                keyColumn: "Id",
                keyValue: new Guid("aaaa8888-8888-8888-8888-888888888888"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 2, 12, 8, 21, 160, DateTimeKind.Utc).AddTicks(7080));

            migrationBuilder.UpdateData(
                table: "ChargingConnectors",
                keyColumn: "Id",
                keyValue: new Guid("aaaa9999-9999-9999-9999-999999999999"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 2, 12, 8, 21, 160, DateTimeKind.Utc).AddTicks(7085));

            migrationBuilder.UpdateData(
                table: "ChargingParks",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 2, 12, 8, 21, 160, DateTimeKind.Utc).AddTicks(6946));

            migrationBuilder.UpdateData(
                table: "ChargingParks",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 7, 12, 8, 21, 160, DateTimeKind.Utc).AddTicks(6951));

            migrationBuilder.UpdateData(
                table: "ChargingPoints",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 2, 12, 8, 21, 160, DateTimeKind.Utc).AddTicks(7044));

            migrationBuilder.UpdateData(
                table: "ChargingPoints",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 2, 12, 8, 21, 160, DateTimeKind.Utc).AddTicks(7047));

            migrationBuilder.UpdateData(
                table: "ChargingStations",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "LastHeartbeat" },
                values: new object[] { new DateTime(2025, 11, 2, 12, 8, 21, 160, DateTimeKind.Utc).AddTicks(6997), new DateTime(2025, 11, 22, 12, 3, 21, 160, DateTimeKind.Utc).AddTicks(6997) });

            migrationBuilder.UpdateData(
                table: "ChargingStations",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "LastHeartbeat" },
                values: new object[] { new DateTime(2025, 11, 4, 12, 8, 21, 160, DateTimeKind.Utc).AddTicks(7003), new DateTime(2025, 11, 22, 12, 5, 21, 160, DateTimeKind.Utc).AddTicks(7004) });

            migrationBuilder.UpdateData(
                table: "QrCodes",
                keyColumn: "Id",
                keyValue: new Guid("eeee2222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "ExpiresAt" },
                values: new object[] { new DateTime(2025, 11, 12, 12, 8, 21, 160, DateTimeKind.Utc).AddTicks(7247), new DateTime(2026, 11, 22, 12, 8, 21, 160, DateTimeKind.Utc).AddTicks(7245) });

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 23, 12, 8, 21, 160, DateTimeKind.Utc).AddTicks(6761));

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("aaaabbbb-1111-2222-3333-444444444444"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 28, 12, 8, 21, 160, DateTimeKind.Utc).AddTicks(6771));

            migrationBuilder.UpdateData(
                table: "UserGroupMemberships",
                keyColumn: "Id",
                keyValue: new Guid("ccccdddd-1111-2222-3333-444444444444"),
                column: "AssignedAt",
                value: new DateTime(2025, 10, 23, 12, 8, 21, 160, DateTimeKind.Utc).AddTicks(6895));

            migrationBuilder.UpdateData(
                table: "UserGroupMemberships",
                keyColumn: "Id",
                keyValue: new Guid("ccccdddd-2222-3333-4444-555555555555"),
                column: "AssignedAt",
                value: new DateTime(2025, 10, 28, 12, 8, 21, 160, DateTimeKind.Utc).AddTicks(6904));

            migrationBuilder.UpdateData(
                table: "UserGroups",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 23, 12, 8, 21, 160, DateTimeKind.Utc).AddTicks(6811));

            migrationBuilder.UpdateData(
                table: "UserGroups",
                keyColumn: "Id",
                keyValue: new Guid("aaaabbbb-3333-4444-5555-666666666666"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 28, 12, 8, 21, 160, DateTimeKind.Utc).AddTicks(6814));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "LastLoginAt" },
                values: new object[] { new DateTime(2025, 10, 23, 12, 8, 21, 160, DateTimeKind.Utc).AddTicks(6856), new DateTime(2025, 11, 21, 12, 8, 21, 160, DateTimeKind.Utc).AddTicks(6857) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("aaaabbbb-2222-3333-4444-555555555555"),
                columns: new[] { "CreatedAt", "LastLoginAt" },
                values: new object[] { new DateTime(2025, 10, 28, 12, 8, 21, 160, DateTimeKind.Utc).AddTicks(6863), new DateTime(2025, 11, 20, 12, 8, 21, 160, DateTimeKind.Utc).AddTicks(6864) });

            migrationBuilder.UpdateData(
                table: "VehicleAssignments",
                keyColumn: "Id",
                keyValue: new Guid("cccc0000-0000-0000-0000-000000000000"),
                column: "AssignedAt",
                value: new DateTime(2025, 11, 15, 12, 8, 21, 160, DateTimeKind.Utc).AddTicks(7147));

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 28, 12, 8, 21, 160, DateTimeKind.Utc).AddTicks(7116));

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 2, 12, 8, 21, 160, DateTimeKind.Utc).AddTicks(7120));
        }
    }
}
