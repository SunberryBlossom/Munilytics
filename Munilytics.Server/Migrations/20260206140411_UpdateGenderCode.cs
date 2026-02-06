using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Munilytics.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGenderCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Dim_Gender",
                keyColumn: "Id",
                keyValue: 2,
                column: "Code",
                value: "K");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Dim_Gender",
                keyColumn: "Id",
                keyValue: 2,
                column: "Code",
                value: "F");
        }
    }
}
