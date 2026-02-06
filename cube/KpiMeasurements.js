cube(`KpiMeasurements`, {
    //sql query to get data from source, in this case from dwh schema and Fact_Kpi_Measurement table
  sql: `
  SELECT 
    "id"                      AS id,
     "time_id"                AS timeId, 
     "municipality_id"        AS municipalityId,
     "kpi_id"                 AS kpiId,
     "gender_id"              AS genderId,

     "value"                 AS value,
     "status"                AS status,
     "count"                 AS observationCount,
     "import_date"           AS importDate,
     "latest_update"         AS latestUpdate
     
    FROM dwh."Fact_Kpi_Measurement"`,

  preAggregations: {
    // probably can implement later if it's needed
  },

  //FK relationships
  joins: {
    Time: {
      relationship: `belongsTo`,
      sql: `${CUBE}.timeId = ${Time}.id`
    },

    Municipalities: {
      relationship: `belongsTo`,
      sql: `${CUBE}.municipalityId = ${Municipalities}.id`
    },

    Kpis: {
      relationship: `belongsTo`,
      sql: `${CUBE}.kpiId = ${Kpis}.id`
    },

    Genders: {
      relationship: `belongsTo`,
      sql: `${CUBE}.genderId = ${Genders}.id`
    }
  },
// Measures: aggregated KPI values calculated from fact records
  measures: {
    // Technical measure: number of fact rows
    rows: {
      type: `count`,
      drillMembers: [id, importDate, latestUpdate]
    },
      // Primary KPI metric: average value (safe default for most KPIs)
    avgValue: {
      sql: `value`,
      type: `avg`,
      title: `Average KPI Value`
    },
    // Optional metric: sum of values (valid only for additive KPIs)
    sumValue: {
      sql: `value`,
      type: `sum`,
      title: `Sum KPI Value`,
      description: `Only valid for additive KPIs`
    },
     // Sum of observation counts (used for weighted averages or diagnostics)
    observationCountSum: {
      sql: `observationCount`,
      type: `sum`,
      title: `Sum Observation Count`
    }
    
  },
// Dimensions used for grouping and filtering analytics queries
  dimensions: {
    id: {
      sql: `id`,
      type: `number`,
      primaryKey: true
    },
    
    status: {
      sql: `status`,
      type: `string`
    },
    importDate: {
      sql: `importDate`,
      type: `time`
    },
   latestUpdate: {
      sql: `latestUpdate`,
      type: `time`
    }

  },
  
  //This section is for defining reusable filters (segments) that can be applied to measures. For example, you can define a segment for "definitive" KPI values and then use it in your queries to filter the data accordingly.
// Segments: reusable business filters applied to fact data
  segments: {
     // Includes only definitive (final, confirmed) KPI values
    definitiveOnly: {
      sql: `${CUBE}.status = 'definitive'`
    }
  }
});