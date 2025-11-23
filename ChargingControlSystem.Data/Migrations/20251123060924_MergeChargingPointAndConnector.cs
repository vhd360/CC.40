using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ChargingControlSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class MergeChargingPointAndConnector : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Schritt 1: Neue Spalten zu ChargingPoints hinzufügen (nullable, damit wir Daten migrieren können)
            migrationBuilder.AddColumn<string>(
                name: "ConnectorFormat",
                table: "ChargingPoints",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConnectorType",
                table: "ChargingPoints",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxCurrent",
                table: "ChargingPoints",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxVoltage",
                table: "ChargingPoints",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhysicalReference",
                table: "ChargingPoints",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PowerType",
                table: "ChargingPoints",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            // Schritt 2: Daten von ChargingConnectors nach ChargingPoints migrieren
            migrationBuilder.Sql(@"
                UPDATE cp
                SET 
                    cp.ConnectorType = cc.ConnectorType,
                    cp.ConnectorFormat = cc.ConnectorFormat,
                    cp.PowerType = cc.PowerType,
                    cp.MaxCurrent = cc.MaxCurrent,
                    cp.MaxVoltage = cc.MaxVoltage,
                    cp.PhysicalReference = cc.PhysicalReference
                FROM ChargingPoints cp
                INNER JOIN ChargingConnectors cc ON cp.Id = cc.ChargingPointId
                WHERE cc.Id = (
                    SELECT TOP 1 cc2.Id 
                    FROM ChargingConnectors cc2 
                    WHERE cc2.ChargingPointId = cp.Id 
                    ORDER BY cc2.ConnectorId
                );
            ");

            // Schritt 3: ChargingSessions.ChargingConnectorId auf ChargingPointId ändern
            migrationBuilder.Sql(@"
                UPDATE cs
                SET cs.ChargingConnectorId = cc.ChargingPointId
                FROM ChargingSessions cs
                INNER JOIN ChargingConnectors cc ON cs.ChargingConnectorId = cc.Id;
            ");

            // Schritt 4: Spalten auf NOT NULL setzen (nach Migration)
            migrationBuilder.AlterColumn<string>(
                name: "ConnectorType",
                table: "ChargingPoints",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Type2");

            migrationBuilder.AlterColumn<int>(
                name: "MaxCurrent",
                table: "ChargingPoints",
                type: "int",
                nullable: false,
                defaultValue: 32);

            migrationBuilder.AlterColumn<int>(
                name: "MaxVoltage",
                table: "ChargingPoints",
                type: "int",
                nullable: false,
                defaultValue: 230);

            // Schritt 5: Foreign Key und Index umbenennen
            migrationBuilder.DropForeignKey(
                name: "FK_ChargingSessions_ChargingConnectors_ChargingConnectorId",
                table: "ChargingSessions");

            migrationBuilder.RenameColumn(
                name: "ChargingConnectorId",
                table: "ChargingSessions",
                newName: "ChargingPointId");

            migrationBuilder.RenameIndex(
                name: "IX_ChargingSessions_ChargingConnectorId",
                table: "ChargingSessions",
                newName: "IX_ChargingSessions_ChargingPointId");

            // Schritt 6: ChargingConnectors-Tabelle löschen
            migrationBuilder.DropTable(
                name: "ChargingConnectors");

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
                columns: new[] { "ConnectorFormat", "ConnectorType", "CreatedAt", "MaxCurrent", "MaxVoltage", "PhysicalReference", "PowerType" },
                values: new object[] { "SOCKET", "CCS", new DateTime(2025, 11, 3, 6, 9, 23, 474, DateTimeKind.Utc).AddTicks(8614), 200, 800, null, "DC" });

            migrationBuilder.UpdateData(
                table: "ChargingPoints",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "ConnectorFormat", "ConnectorType", "CreatedAt", "MaxCurrent", "MaxVoltage", "PhysicalReference", "PowerType" },
                values: new object[] { "SOCKET", "CCS", new DateTime(2025, 11, 3, 6, 9, 23, 474, DateTimeKind.Utc).AddTicks(8618), 200, 800, null, "DC" });

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

            migrationBuilder.AddForeignKey(
                name: "FK_ChargingSessions_ChargingPoints_ChargingPointId",
                table: "ChargingSessions",
                column: "ChargingPointId",
                principalTable: "ChargingPoints",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChargingSessions_ChargingPoints_ChargingPointId",
                table: "ChargingSessions");

            migrationBuilder.DropColumn(
                name: "ConnectorFormat",
                table: "ChargingPoints");

            migrationBuilder.DropColumn(
                name: "ConnectorType",
                table: "ChargingPoints");

            migrationBuilder.DropColumn(
                name: "MaxCurrent",
                table: "ChargingPoints");

            migrationBuilder.DropColumn(
                name: "MaxVoltage",
                table: "ChargingPoints");

            migrationBuilder.DropColumn(
                name: "PhysicalReference",
                table: "ChargingPoints");

            migrationBuilder.DropColumn(
                name: "PowerType",
                table: "ChargingPoints");

            migrationBuilder.RenameColumn(
                name: "ChargingPointId",
                table: "ChargingSessions",
                newName: "ChargingConnectorId");

            migrationBuilder.RenameIndex(
                name: "IX_ChargingSessions_ChargingPointId",
                table: "ChargingSessions",
                newName: "IX_ChargingSessions_ChargingConnectorId");

            migrationBuilder.CreateTable(
                name: "ChargingConnectors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChargingPointId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConnectorFormat = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ConnectorId = table.Column<int>(type: "int", nullable: false),
                    ConnectorType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastStatusChange = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MaxCurrent = table.Column<int>(type: "int", nullable: false),
                    MaxPower = table.Column<int>(type: "int", nullable: false),
                    MaxVoltage = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PhysicalReference = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PowerType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChargingConnectors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChargingConnectors_ChargingPoints_ChargingPointId",
                        column: x => x.ChargingPointId,
                        principalTable: "ChargingPoints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "BillingAccounts",
                keyColumn: "Id",
                keyValue: new Guid("dddd1111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 8, 12, 24, 19, 817, DateTimeKind.Utc).AddTicks(9421));

            migrationBuilder.InsertData(
                table: "ChargingConnectors",
                columns: new[] { "Id", "ChargingPointId", "ConnectorFormat", "ConnectorId", "ConnectorType", "CreatedAt", "IsActive", "LastStatusChange", "MaxCurrent", "MaxPower", "MaxVoltage", "Notes", "PhysicalReference", "PowerType", "Status" },
                values: new object[,]
                {
                    { new Guid("aaaa8888-8888-8888-8888-888888888888"), new Guid("88888888-8888-8888-8888-888888888888"), null, 1, "CCS", new DateTime(2025, 10, 18, 12, 24, 19, 817, DateTimeKind.Utc).AddTicks(9309), true, null, 200, 150, 800, null, null, "DC", 0 },
                    { new Guid("aaaa9999-9999-9999-9999-999999999999"), new Guid("99999999-9999-9999-9999-999999999999"), null, 1, "CCS", new DateTime(2025, 10, 18, 12, 24, 19, 817, DateTimeKind.Utc).AddTicks(9311), true, null, 200, 150, 800, null, null, "DC", 0 }
                });

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
                column: "CreatedAt",
                value: new DateTime(2025, 10, 13, 12, 24, 19, 817, DateTimeKind.Utc).AddTicks(9370));

            migrationBuilder.UpdateData(
                table: "Vehicles",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                column: "CreatedAt",
                value: new DateTime(2025, 10, 18, 12, 24, 19, 817, DateTimeKind.Utc).AddTicks(9372));

            migrationBuilder.CreateIndex(
                name: "IX_ChargingConnectors_ChargingPointId",
                table: "ChargingConnectors",
                column: "ChargingPointId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChargingSessions_ChargingConnectors_ChargingConnectorId",
                table: "ChargingSessions",
                column: "ChargingConnectorId",
                principalTable: "ChargingConnectors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
