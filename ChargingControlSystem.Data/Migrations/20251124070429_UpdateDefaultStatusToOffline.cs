using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChargingControlSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDefaultStatusToOffline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "BillingAccounts",
                keyColumn: "Id",
                keyValue: new Guid("dddd1111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 25, 7, 4, 28, 721, DateTimeKind.Utc).AddTicks(3190));

            migrationBuilder.UpdateData(
                table: "ChargingParks",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 4, 7, 4, 28, 721, DateTimeKind.Utc).AddTicks(2478));

            migrationBuilder.UpdateData(
                table: "ChargingParks",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 9, 7, 4, 28, 721, DateTimeKind.Utc).AddTicks(2487));

            migrationBuilder.UpdateData(
                table: "ChargingPoints",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 4, 7, 4, 28, 721, DateTimeKind.Utc).AddTicks(3021));

            migrationBuilder.UpdateData(
                table: "ChargingPoints",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 4, 7, 4, 28, 721, DateTimeKind.Utc).AddTicks(3023));

            migrationBuilder.UpdateData(
                table: "ChargingStations",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "LastHeartbeat" },
                values: new object[] { new DateTime(2025, 11, 4, 7, 4, 28, 721, DateTimeKind.Utc).AddTicks(2800), new DateTime(2025, 11, 24, 6, 59, 28, 721, DateTimeKind.Utc).AddTicks(2804) });

            migrationBuilder.UpdateData(
                table: "ChargingStations",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "LastHeartbeat" },
                values: new object[] { new DateTime(2025, 11, 6, 7, 4, 28, 721, DateTimeKind.Utc).AddTicks(2824), new DateTime(2025, 11, 24, 7, 1, 28, 721, DateTimeKind.Utc).AddTicks(2825) });

            migrationBuilder.UpdateData(
                table: "QrCodes",
                keyColumn: "Id",
                keyValue: new Guid("eeee2222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "ExpiresAt" },
                values: new object[] { new DateTime(2025, 11, 14, 7, 4, 28, 721, DateTimeKind.Utc).AddTicks(3293), new DateTime(2026, 11, 24, 7, 4, 28, 721, DateTimeKind.Utc).AddTicks(3273) });

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 25, 7, 4, 28, 721, DateTimeKind.Utc).AddTicks(1662));

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("aaaabbbb-1111-2222-3333-444444444444"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 30, 7, 4, 28, 721, DateTimeKind.Utc).AddTicks(1707));

            migrationBuilder.UpdateData(
                table: "UserGroupMemberships",
                keyColumn: "Id",
                keyValue: new Guid("ccccdddd-1111-2222-3333-444444444444"),
                column: "AssignedAt",
                value: new DateTime(2025, 10, 25, 7, 4, 28, 721, DateTimeKind.Utc).AddTicks(2205));

            migrationBuilder.UpdateData(
                table: "UserGroupMemberships",
                keyColumn: "Id",
                keyValue: new Guid("ccccdddd-2222-3333-4444-555555555555"),
                column: "AssignedAt",
                value: new DateTime(2025, 10, 30, 7, 4, 28, 721, DateTimeKind.Utc).AddTicks(2220));

            migrationBuilder.UpdateData(
                table: "UserGroups",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 25, 7, 4, 28, 721, DateTimeKind.Utc).AddTicks(1853));

            migrationBuilder.UpdateData(
                table: "UserGroups",
                keyColumn: "Id",
                keyValue: new Guid("aaaabbbb-3333-4444-5555-666666666666"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 30, 7, 4, 28, 721, DateTimeKind.Utc).AddTicks(1866));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "LastLoginAt" },
                values: new object[] { new DateTime(2025, 10, 25, 7, 4, 28, 721, DateTimeKind.Utc).AddTicks(2041), new DateTime(2025, 11, 23, 7, 4, 28, 721, DateTimeKind.Utc).AddTicks(2048) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("aaaabbbb-2222-3333-4444-555555555555"),
                columns: new[] { "CreatedAt", "LastLoginAt" },
                values: new object[] { new DateTime(2025, 10, 30, 7, 4, 28, 721, DateTimeKind.Utc).AddTicks(2079), new DateTime(2025, 11, 22, 7, 4, 28, 721, DateTimeKind.Utc).AddTicks(2079) });

            migrationBuilder.UpdateData(
                table: "VehicleAssignments",
                keyColumn: "Id",
                keyValue: new Guid("cccc0000-0000-0000-0000-000000000000"),
                column: "AssignedAt",
                value: new DateTime(2025, 11, 17, 7, 4, 28, 721, DateTimeKind.Utc).AddTicks(3134));

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 30, 7, 4, 28, 721, DateTimeKind.Utc).AddTicks(3084));

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 4, 7, 4, 28, 721, DateTimeKind.Utc).AddTicks(3089));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                columns: new[] { "CreatedAt", "LastHeartbeat" },
                values: new object[] { new DateTime(2025, 11, 3, 13, 44, 59, 789, DateTimeKind.Utc).AddTicks(167), new DateTime(2025, 11, 23, 13, 39, 59, 789, DateTimeKind.Utc).AddTicks(167) });

            migrationBuilder.UpdateData(
                table: "ChargingStations",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "LastHeartbeat" },
                values: new object[] { new DateTime(2025, 11, 5, 13, 44, 59, 789, DateTimeKind.Utc).AddTicks(238), new DateTime(2025, 11, 23, 13, 41, 59, 789, DateTimeKind.Utc).AddTicks(238) });

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
        }
    }
}
