using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChargingControlSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPrivateChargingStations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Make ChargingParkId nullable
            migrationBuilder.AlterColumn<Guid>(
                name: "ChargingParkId",
                table: "ChargingStations",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            // Add IsPrivate column
            migrationBuilder.AddColumn<bool>(
                name: "IsPrivate",
                table: "ChargingStations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Add OwnerId column
            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "ChargingStations",
                type: "uniqueidentifier",
                nullable: true);

            // Create foreign key for OwnerId
            migrationBuilder.CreateIndex(
                name: "IX_ChargingStations_OwnerId",
                table: "ChargingStations",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChargingStations_Users_OwnerId",
                table: "ChargingStations",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop foreign key
            migrationBuilder.DropForeignKey(
                name: "FK_ChargingStations_Users_OwnerId",
                table: "ChargingStations");

            // Drop index
            migrationBuilder.DropIndex(
                name: "IX_ChargingStations_OwnerId",
                table: "ChargingStations");

            // Remove OwnerId column
            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "ChargingStations");

            // Remove IsPrivate column
            migrationBuilder.DropColumn(
                name: "IsPrivate",
                table: "ChargingStations");

            // Make ChargingParkId required again
            migrationBuilder.AlterColumn<Guid>(
                name: "ChargingParkId",
                table: "ChargingStations",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}

