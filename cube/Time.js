сube("Time",{
  //sql query to get data from source, in this case from dwh schema and Dim_Time table
    sql: `
  SELECT
    "id"                  AS id,
    "year"                AS year,
    "mandate_period"       AS mandatePeriod,
    "is_election_year"      AS isElectionYear,
    "decade"              AS decade,
    "relative_year"        AS relativeYear
  FROM dwh."Dim_Time"
  `,
    // Dimensions used for grouping and filtering analytics queries
   dimensions:{
    id:{
        sql:"id",
        type:'number',
        primaryKey:true
    },

     year:{
        sql:"year",
        type:'number'
     },
     mandatePeriod:{
         sql:"mandatePeriod",
     type:'string'
     },
     isElectionYear:{
    sql:"isElectionYear",
     type:'boolean'

     },
     decade:{
         sql:"decade",
    type:'string'
     },
     relativeYear:{
             sql:"relativeYear",
    type:'number'}
     
   }


});