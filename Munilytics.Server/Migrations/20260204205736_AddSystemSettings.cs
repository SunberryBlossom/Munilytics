using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Munilytics.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Dim_Gender",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    GenderName = table.Column<string>(type: "text", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dim_Gender", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Dim_KPIs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    KpiCode = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    Description = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    IsDividedByGender = table.Column<bool>(type: "boolean", nullable: false),
                    MunicipalityType = table.Column<string>(type: "text", nullable: false),
                    Auspice = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    OperatingArea = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    Perspective = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    Unit = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dim_KPIs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Dim_Municipalities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    County = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SkrGroupCode = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SkrGroupName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dim_Municipalities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Dim_Time",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Decade = table.Column<int>(type: "integer", nullable: false),
                    MandatePeriod = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    IsElectionYear = table.Column<bool>(type: "boolean", nullable: false),
                    RelativeYear = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dim_Time", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    LastSync = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Fact_KpiMeasurements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Value = table.Column<decimal>(type: "numeric", nullable: false),
                    IsDefinite = table.Column<bool>(type: "boolean", nullable: false),
                    Count = table.Column<int>(type: "integer", nullable: false),
                    ImportDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LatestUpdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DimMunicipalityId = table.Column<int>(type: "integer", nullable: false),
                    DimKpiId = table.Column<int>(type: "integer", nullable: false),
                    DimGenderId = table.Column<int>(type: "integer", nullable: false),
                    DimTimeId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fact_KpiMeasurements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fact_KpiMeasurements_Dim_Gender_DimGenderId",
                        column: x => x.DimGenderId,
                        principalTable: "Dim_Gender",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Fact_KpiMeasurements_Dim_KPIs_DimKpiId",
                        column: x => x.DimKpiId,
                        principalTable: "Dim_KPIs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Fact_KpiMeasurements_Dim_Municipalities_DimMunicipalityId",
                        column: x => x.DimMunicipalityId,
                        principalTable: "Dim_Municipalities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Fact_KpiMeasurements_Dim_Time_DimTimeId",
                        column: x => x.DimTimeId,
                        principalTable: "Dim_Time",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Dim_Gender",
                columns: new[] { "Id", "Code", "GenderName", "SortOrder" },
                values: new object[,]
                {
                    { 1, "T", "Total", 1 },
                    { 2, "F", "Female", 2 },
                    { 3, "M", "Male", 3 }
                });

            migrationBuilder.InsertData(
                table: "Dim_Time",
                columns: new[] { "Id", "Decade", "IsElectionYear", "MandatePeriod", "RelativeYear", "Year" },
                values: new object[,]
                {
                    { 1, 1990, true, "1994-1997", -32, 1994 },
                    { 2, 1990, false, "1994-1997", -31, 1995 },
                    { 3, 1990, false, "1994-1997", -30, 1996 },
                    { 4, 1990, false, "1994-1997", -29, 1997 },
                    { 5, 1990, true, "1998-2001", -28, 1998 },
                    { 6, 1990, false, "1998-2001", -27, 1999 },
                    { 7, 2000, false, "1998-2001", -26, 2000 },
                    { 8, 2000, false, "1998-2001", -25, 2001 },
                    { 9, 2000, true, "2002-2005", -24, 2002 },
                    { 10, 2000, false, "2002-2005", -23, 2003 },
                    { 11, 2000, false, "2002-2005", -22, 2004 },
                    { 12, 2000, false, "2002-2005", -21, 2005 },
                    { 13, 2000, true, "2006-2009", -20, 2006 },
                    { 14, 2000, false, "2006-2009", -19, 2007 },
                    { 15, 2000, false, "2006-2009", -18, 2008 },
                    { 16, 2000, false, "2006-2009", -17, 2009 },
                    { 17, 2010, true, "2010-2013", -16, 2010 },
                    { 18, 2010, false, "2010-2013", -15, 2011 },
                    { 19, 2010, false, "2010-2013", -14, 2012 },
                    { 20, 2010, false, "2010-2013", -13, 2013 },
                    { 21, 2010, true, "2014-2017", -12, 2014 },
                    { 22, 2010, false, "2014-2017", -11, 2015 },
                    { 23, 2010, false, "2014-2017", -10, 2016 },
                    { 24, 2010, false, "2014-2017", -9, 2017 },
                    { 25, 2010, true, "2018-2021", -8, 2018 },
                    { 26, 2010, false, "2018-2021", -7, 2019 },
                    { 27, 2020, false, "2018-2021", -6, 2020 },
                    { 28, 2020, false, "2018-2021", -5, 2021 },
                    { 29, 2020, true, "2022-2025", -4, 2022 },
                    { 30, 2020, false, "2022-2025", -3, 2023 },
                    { 31, 2020, false, "2022-2025", -2, 2024 },
                    { 32, 2020, false, "2022-2025", -1, 2025 },
                    { 33, 2020, true, "2026-2029", 0, 2026 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Fact_KpiMeasurements");

            migrationBuilder.DropTable(
                name: "SystemSettings");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Dim_Gender");

            migrationBuilder.DropTable(
                name: "Dim_KPIs");

            migrationBuilder.DropTable(
                name: "Dim_Municipalities");

            migrationBuilder.DropTable(
                name: "Dim_Time");
        }
    }
}
