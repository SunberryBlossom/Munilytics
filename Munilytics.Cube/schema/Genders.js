cube("Genders",{
  //sql query to get data from source, in this case from dwh schema and Dim_Gender table
   sql: `
  SELECT 
    "id"          AS id,
    "gender_code"  AS genderCode,
    "gender_name"  AS genderName
     FROM dwh."Dim_Gender"
    `,

    
   // Dimensions used for grouping and filtering analytics queries
   dimensions:{
    id:{
        sql:"id",
        type:'number',
        primaryKey:true
    },

     genderCode:{
        sql:"genderCode",
        type:'string'
     },
	 
     genderName:{
        sql:"genderName",
        type:'string'
     },
	 
     sortOrder:{
         sql:"sortOrder",
     type:'number'
     }
   }

});