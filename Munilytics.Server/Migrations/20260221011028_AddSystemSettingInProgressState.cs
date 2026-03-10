using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Munilytics.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemSettingInProgressState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "InProgress",
                table: "SystemSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "InProgressUpdatedAt",
                table: "SystemSettings",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InProgress",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "InProgressUpdatedAt",
                table: "SystemSettings");
        }
    }
}
