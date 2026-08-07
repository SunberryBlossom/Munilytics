using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Munilytics.Server.Migrations
{
    /// <inheritdoc />
    public partial class FactData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDefinite",
                table: "Fact_KpiMeasurements");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Fact_KpiMeasurements",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "KoladaId",
                table: "Dim_Municipalities",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Fact_KpiMeasurements");

            migrationBuilder.DropColumn(
                name: "KoladaId",
                table: "Dim_Municipalities");

            migrationBuilder.AddColumn<bool>(
                name: "IsDefinite",
                table: "Fact_KpiMeasurements",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
