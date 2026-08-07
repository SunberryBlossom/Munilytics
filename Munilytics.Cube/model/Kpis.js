// Lowercase aliases on purpose: unquoted identifiers fold to lowercase in Postgres,
// so Cube can reference them without quoting.
cube("Kpis", {
  sql: `
  SELECT
    "Id"                  AS id,
    "KpiCode"             AS kpicode,
    "Title"               AS title,
    "Description"         AS description,
    "IsDividedByGender"   AS isdividedbygender,
    "MunicipalityType"    AS municipalitytype,
    "Auspice"             AS auspice,
    "OperatingArea"       AS operatingarea,
    "Perspective"         AS perspective,
    "Unit"                AS unit
  FROM public."Dim_KPIs"
  `,

  dimensions: {
    id: { sql: "id", type: "number", primaryKey: true },
    kpiCode: { sql: "kpicode", type: "string", shown: true },
    title: { sql: "title", type: "string" },
    description: { sql: "description", type: "string" },
    isDividedByGender: { sql: "isdividedbygender", type: "boolean" },
    municipalityType: { sql: "municipalitytype", type: "string" },
    auspice: { sql: "auspice", type: "string" },
    operatingArea: { sql: "operatingarea", type: "string" },
    perspective: { sql: "perspective", type: "string" },
    unit: { sql: "unit", type: "string" }
  },

  measures: {
    count: { type: "count" }
  }
});
