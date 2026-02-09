using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Munilytics.Server.Migrations
{
    public partial class MakeFactTableColumnar : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // This is to make sure that the columnar extension exists if for some reason it does not.
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS columnar;");

            // Hydra does not accept constraints like primary keys for duplication so we remove those.
            // Also removes indexes here for now.
            migrationBuilder.Sql("ALTER TABLE \"Fact_KpiMeasurements\" DROP CONSTRAINT IF EXISTS \"PK_Fact_KpiMeasurements\";");
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_Fact_KpiMeasurements_DimGenderId\";");
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_Fact_KpiMeasurements_DimKpiId\";");
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_Fact_KpiMeasurements_DimMunicipalityId\";");
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_Fact_KpiMeasurements_DimTimeId\";");

            // Once all of this is done, make the fact table columnar
            migrationBuilder.Sql("SELECT columnar.alter_table_set_access_method('Fact_KpiMeasurements', 'columnar');");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("SELECT columnar.alter_table_set_access_method('Fact_KpiMeasurements', 'heap');");

            migrationBuilder.Sql("ALTER TABLE \"Fact_KpiMeasurements\" ADD CONSTRAINT \"PK_Fact_KpiMeasurements\" PRIMARY KEY (\"Id\");");

            migrationBuilder.CreateIndex(
                name: "IX_Fact_KpiMeasurements_DimGenderId",
                table: "Fact_KpiMeasurements",
                column: "DimGenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Fact_KpiMeasurements_DimKpiId",
                table: "Fact_KpiMeasurements",
                column: "DimKpiId");

            migrationBuilder.CreateIndex(
                name: "IX_Fact_KpiMeasurements_DimMunicipalityId",
                table: "Fact_KpiMeasurements",
                column: "DimMunicipalityId");

            migrationBuilder.CreateIndex(
                name: "IX_Fact_KpiMeasurements_DimTimeId",
                table: "Fact_KpiMeasurements",
                column: "DimTimeId");
        }
    }
}