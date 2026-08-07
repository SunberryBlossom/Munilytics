cube("KpiMeasurements", {
  sql: `
  SELECT
    "Id"                  AS id,
    "DimTimeId"           AS timeid,
    "DimMunicipalityId"   AS municipalityid,
    "DimKpiId"            AS kpiid,
    "DimGenderId"         AS genderid,
    "Value"               AS value,
    "Status"              AS status,
    "Count"               AS observationcount,
    "ImportDate"          AS importdate,
    "LatestUpdate"        AS latestupdate
  FROM public."Fact_KpiMeasurements"
  `,

  joins: {
    Time: {
      relationship: "belongsTo",
      sql: `${CUBE}.timeid = ${Time}.id`
    },
    Municipalities: {
      relationship: "belongsTo",
      sql: `${CUBE}.municipalityid = ${Municipalities}.id`
    },
    Kpis: {
      relationship: "belongsTo",
      sql: `${CUBE}.kpiid = ${Kpis}.id`
    },
    Genders: {
      relationship: "belongsTo",
      sql: `${CUBE}.genderid = ${Genders}.id`
    }
  },

  measures: {
    rows: {
      type: "count",
      title: "Measurement Count"
    },
    avgValue: {
      sql: "value",
      type: "avg",
      title: "Average KPI Value"
    },
    sumValue: {
      sql: "value",
      type: "sum",
      title: "Sum KPI Value",
      description: "Only valid for additive KPIs"
    },
    observationCountSum: {
      sql: "observationcount",
      type: "sum",
      title: "Sum Observation Count"
    }
  },

  dimensions: {
    id: { sql: "id", type: "number", primaryKey: true },
    // '' = reported, 'Missing' = not collected, 'Privacy' = suppressed (always stored as 0)
    status: { sql: "status", type: "string" },
    importDate: { sql: "importdate", type: "time" },
    latestUpdate: { sql: "latestupdate", type: "time" }
  },

  segments: {
    // Excludes 'Missing' and 'Privacy' rows, whose values are placeholders, not data.
    reportedOnly: {
      sql: `${CUBE}.status = ''`
    }
  }
});
