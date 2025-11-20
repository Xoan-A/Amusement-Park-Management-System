using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UserDailyScoreMaintenanceScheduleProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "ScoreHistories");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "MaintenanceSchedules");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "MaintenanceSchedules");

            migrationBuilder.DropColumn(
                name: "MaintenanceType",
                table: "MaintenanceRecords");

            migrationBuilder.RenameColumn(
                name: "MaintenanceType",
                table: "MaintenanceSchedules",
                newName: "EstimatedDuration");

            migrationBuilder.AddColumn<int>(
                name: "DailyScore",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsOverdue",
                table: "MaintenanceSchedules",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "DailyScore",
                value: 0);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "DailyScore",
                value: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DailyScore",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsOverdue",
                table: "MaintenanceSchedules");

            migrationBuilder.RenameColumn(
                name: "EstimatedDuration",
                table: "MaintenanceSchedules",
                newName: "MaintenanceType");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ScoreHistories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "MaintenanceSchedules",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "MaintenanceSchedules",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaintenanceType",
                table: "MaintenanceRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
