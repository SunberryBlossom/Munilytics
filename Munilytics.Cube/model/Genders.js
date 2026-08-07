cube("Genders", {
  sql: `
  SELECT
    "Id"          AS id,
    "Code"        AS code,
    "GenderName"  AS gendername,
    "SortOrder"   AS sortorder
  FROM public."Dim_Gender"
  `,

  dimensions: {
    id: { sql: "id", type: "number", primaryKey: true },
    // T = total, K = female, M = male
    code: { sql: "code", type: "string" },
    genderName: { sql: "gendername", type: "string" },
    sortOrder: { sql: "sortorder", type: "number" }
  }
});
