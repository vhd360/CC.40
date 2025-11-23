using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChargingControlSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokenToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "Users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefreshTokenExpiresAt",
                table: "Users",
                type: "datetime2",
                nullable: true);

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
                columns: new[] { "CreatedAt", "LastLoginAt", "RefreshToken", "RefreshTokenExpiresAt" },
                values: new object[] { new DateTime(2025, 10, 24, 8, 52, 28, 192, DateTimeKind.Utc).AddTicks(232), new DateTime(2025, 11, 22, 8, 52, 28, 192, DateTimeKind.Utc).AddTicks(232), null, null });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("aaaabbbb-2222-3333-4444-555555555555"),
                columns: new[] { "CreatedAt", "LastLoginAt", "RefreshToken", "RefreshTokenExpiresAt" },
                values: new object[] { new DateTime(2025, 10, 29, 8, 52, 28, 192, DateTimeKind.Utc).AddTicks(239), new DateTime(2025, 11, 21, 8, 52, 28, 192, DateTimeKind.Utc).AddTicks(239), null, null });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RefreshTokenExpiresAt",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "BillingAccounts",
                keyColumn: "Id",
                keyValue: new Guid("dddd1111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 24, 6, 9, 23, 474, DateTimeKind.Utc).AddTicks(8724));

            migrationBuilder.UpdateData(
                table: "ChargingParks",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 6, 9, 23, 474, DateTimeKind.Utc).AddTicks(8286));

            migrationBuilder.UpdateData(
                table: "ChargingParks",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 8, 6, 9, 23, 474, DateTimeKind.Utc).AddTicks(8292));

            migrationBuilder.UpdateData(
                table: "ChargingPoints",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 6, 9, 23, 474, DateTimeKind.Utc).AddTicks(8614));

            migrationBuilder.UpdateData(
                table: "ChargingPoints",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 6, 9, 23, 474, DateTimeKind.Utc).AddTicks(8618));

            migrationBuilder.UpdateData(
                table: "ChargingStations",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "LastHeartbeat" },
                values: new object[] { new DateTime(2025, 11, 3, 6, 9, 23, 474, DateTimeKind.Utc).AddTicks(8335), new DateTime(2025, 11, 23, 6, 4, 23, 474, DateTimeKind.Utc).AddTicks(8336) });

            migrationBuilder.UpdateData(
                table: "ChargingStations",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "LastHeartbeat" },
                values: new object[] { new DateTime(2025, 11, 5, 6, 9, 23, 474, DateTimeKind.Utc).AddTicks(8343), new DateTime(2025, 11, 23, 6, 6, 23, 474, DateTimeKind.Utc).AddTicks(8343) });

            migrationBuilder.UpdateData(
                table: "QrCodes",
                keyColumn: "Id",
                keyValue: new Guid("eeee2222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "ExpiresAt" },
                values: new object[] { new DateTime(2025, 11, 13, 6, 9, 23, 474, DateTimeKind.Utc).AddTicks(8755), new DateTime(2026, 11, 23, 6, 9, 23, 474, DateTimeKind.Utc).AddTicks(8752) });

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 24, 6, 9, 23, 474, DateTimeKind.Utc).AddTicks(8028));

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("aaaabbbb-1111-2222-3333-444444444444"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 29, 6, 9, 23, 474, DateTimeKind.Utc).AddTicks(8043));

            migrationBuilder.UpdateData(
                table: "UserGroupMemberships",
                keyColumn: "Id",
                keyValue: new Guid("ccccdddd-1111-2222-3333-444444444444"),
                column: "AssignedAt",
                value: new DateTime(2025, 10, 24, 6, 9, 23, 474, DateTimeKind.Utc).AddTicks(8240));

            migrationBuilder.UpdateData(
                table: "UserGroupMemberships",
                keyColumn: "Id",
                keyValue: new Guid("ccccdddd-2222-3333-4444-555555555555"),
                column: "AssignedAt",
                value: new DateTime(2025, 10, 29, 6, 9, 23, 474, DateTimeKind.Utc).AddTicks(8244));

            migrationBuilder.UpdateData(
                table: "UserGroups",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 24, 6, 9, 23, 474, DateTimeKind.Utc).AddTicks(8090));

            migrationBuilder.UpdateData(
                table: "UserGroups",
                keyColumn: "Id",
                keyValue: new Guid("aaaabbbb-3333-4444-5555-666666666666"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 29, 6, 9, 23, 474, DateTimeKind.Utc).AddTicks(8093));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "LastLoginAt" },
                values: new object[] { new DateTime(2025, 10, 24, 6, 9, 23, 474, DateTimeKind.Utc).AddTicks(8138), new DateTime(2025, 11, 22, 6, 9, 23, 474, DateTimeKind.Utc).AddTicks(8139) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("aaaabbbb-2222-3333-4444-555555555555"),
                columns: new[] { "CreatedAt", "LastLoginAt" },
                values: new object[] { new DateTime(2025, 10, 29, 6, 9, 23, 474, DateTimeKind.Utc).AddTicks(8199), new DateTime(2025, 11, 21, 6, 9, 23, 474, DateTimeKind.Utc).AddTicks(8200) });

            migrationBuilder.UpdateData(
                table: "VehicleAssignments",
                keyColumn: "Id",
                keyValue: new Guid("cccc0000-0000-0000-0000-000000000000"),
                column: "AssignedAt",
                value: new DateTime(2025, 11, 16, 6, 9, 23, 474, DateTimeKind.Utc).AddTicks(8685));

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 29, 6, 9, 23, 474, DateTimeKind.Utc).AddTicks(8657));

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 3, 6, 9, 23, 474, DateTimeKind.Utc).AddTicks(8660));
        }
    }
}
