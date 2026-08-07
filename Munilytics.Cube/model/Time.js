cube("Time", {
  sql: `
  SELECT
    "Id"              AS id,
    "Year"            AS year,
    "Decade"          AS decade,
    "MandatePeriod"   AS mandateperiod,
    "IsElectionYear"  AS iselectionyear,
    "RelativeYear"    AS relativeyear
  FROM public."Dim_Time"
  `,

  dimensions: {
    id: { sql: "id", type: "number", primaryKey: true },
    year: { sql: "year", type: "number" },
    decade: { sql: "decade", type: "number" },
    mandatePeriod: { sql: "mandateperiod", type: "string" },
    isElectionYear: { sql: "iselectionyear", type: "boolean" },
    relativeYear: { sql: "relativeyear", type: "number" }
  }
});
