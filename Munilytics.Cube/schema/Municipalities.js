cube("Municipalities",{
   //sql query to get data from source, in this case from dwh schema and Dim_Municipality table
    sql: `
  SELECT
    "id"                      AS id,
    "title"                AS title,
    "type"                AS type,
    "county"              AS county,
    "skr_group_code"  AS skrGroupCode,
    "skr_group_name"  AS skrGroupName
     FROM dwh."Dim_Municipality"
    `,

    // Dimensions used for grouping and filtering analytics queries
   dimensions:{
    id:{
        sql:"id",
        type:'number',
        primaryKey:true
    },
     
    title:{
        sql:"title",
        type:'string'
     },
    type:{
        sql:"type",
        type:'string'
     },
    county:{
        sql:"county",
        type:'string'
     },
    skrGroupCode:{
        sql:"skrGroupCode",
        type:'string'
     },
    skrGroupName:{
        sql:"skrGroupName",
        type:'string'
     },
    
     
    }

});