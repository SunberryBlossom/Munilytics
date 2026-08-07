cube("Municipalities", {
  sql: `
  SELECT
    "Id"             AS id,
    "KoladaId"       AS koladaid,
    "Title"          AS title,
    "Type"           AS type,
    "County"         AS county,
    "SkrGroupCode"   AS skrgroupcode,
    "SkrGroupName"   AS skrgroupname
  FROM public."Dim_Municipalities"
  `,

  dimensions: {
    id: { sql: "id", type: "number", primaryKey: true },
    koladaId: { sql: "koladaid", type: "string" },
    title: { sql: "title", type: "string" },
    // K = kommun, L = landsting/region
    type: { sql: "type", type: "string" },
    county: { sql: "county", type: "string" },
    skrGroupCode: { sql: "skrgroupcode", type: "string" },
    skrGroupName: { sql: "skrgroupname", type: "string" }
  },

  measures: {
    count: { type: "count" }
  }
});
