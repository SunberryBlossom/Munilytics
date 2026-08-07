using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Munilytics.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddFactKpiIdIndex : Migration
    {
        // MakeFactTableColumnar dropped every index via raw SQL, so EF's model snapshot still
        // believes they exist and won't scaffold this itself. Raw SQL both ways to match.
        // Every dashboard query filters by KPI first; without this a comparison is a ~9M row
        // seq scan (2.3s), with it 0.10s.
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "CREATE INDEX IF NOT EXISTS \"IX_Fact_KpiMeasurements_DimKpiId\" ON \"Fact_KpiMeasurements\" (\"DimKpiId\");");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_Fact_KpiMeasurements_DimKpiId\";");
        }
    }
}
