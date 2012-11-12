using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

using Analyzer.Entity;

namespace MongoDBDemo.Code
{
    class MongoDBUtility
    {
        String _strIPAddress = "127.0.0.1";
        String _strPort = "27017";

        /// <summary>
        /// Method to get the Mongo Server reference
        /// </summary>
        /// <returns>MongoServer</returns>
        private MongoServer getServer()
        {
            //Declarations
            StringBuilder sbConnectionString = new StringBuilder();

            //Construct the connection string
            sbConnectionString.Append("mongodb://");
            sbConnectionString.Append(_strIPAddress);
            sbConnectionString.Append(":");
            sbConnectionString.Append(_strPort);
            sbConnectionString.Append("/?safe=true");

            return MongoServer.Create(sbConnectionString.ToString());
        }

        /// <summary>
        /// Returns the names of database present in the server
        /// </summary>
        /// <returns></returns>
        public List<String> getDataBaseNames()
        {
            return getServer().GetDatabaseNames().ToList<String>();
        }

        /// <summary>
        /// Returns the tables present in this database
        /// </summary>
        /// <param name="strDataBaseName"></param>
        /// <returns></returns>
        public List<String> getTableNames(String strDataBaseName)
        {
            //Declarations
            MongoDatabase objMongoDatabase = getServer().GetDatabase(strDataBaseName);

            return objMongoDatabase.GetCollectionNames().ToList<String>();
        }

        /// <summary>
        /// Returns the table data
        /// </summary>
        /// <param name="strTableName"></param>
        /// <returns></returns>
        public DataTable GetData(String strDataBaseName, String strTableName, String strFilter, out Int32 intMatches)
        {
            //Declarations
            DataTable dtData = new DataTable();
            MongoCollection<NetworkData> colData = getServer().GetDatabase(strDataBaseName).GetCollection<NetworkData>(strTableName);
            
            //Initialize
            intMatches = 0;


            #region Map Reduce definition
           
            var map =
                      @"function Map() {
	                        //Declaration
	                        var searchFactor = '###';
	                        var hasMatch = false;
	                        var parameter;
	
	                        //Get the Request Parameters
	                        var requestParameters = this.RequestParameters;
	
	                        //Check whether its not null
	                        if(requestParameters != null){			
		                        //Check whether its not in Array format <TEMP>
		                        //if(requestParameters instanceof Array){
			                        //parameter = true;
			                        for(var key in requestParameters){
				                        if(requestParameters.hasOwnProperty(key)){
					                        //Get the parameter
					                        parameter = requestParameters[key];
			
					                        //Check whether the parameter has a match
					                        if(parameter.indexOf(searchFactor) != -1){
						                        hasMatch = true;
						                        break;
					                        }
				                        }
			                        }				
		                        //}	
	                        }

	                        //Emit the results
	                        emit(hasMatch, { Count: 1, Matches: [{Key:key, Parameter:parameter, FoundOn: this}] });
		
                        }";

            //Set the search factor
            map = map.Replace("###", strFilter);

               var reduce =
                       @"function Reduce(key, values) {
	                        //Define the result structure	
	                        var result = { Count:0, Matches:[] };
	
	                        //If there is a match
	                        if(key){
		                        values.forEach(function(val){
			                        result.Count += val.Count;
			
			                        //Push all the matches
			                        val.Matches.forEach(function(match){
				                        result.Matches.push(match);
			                        });			
		                        });
	                        }
		
	                        return result;
                        }";

            #endregion

            MapReduceOptionsBuilder options = new MapReduceOptionsBuilder();            
            options.SetOutput(MapReduceOutput.Inline);

            //Do Map reduce
            MapReduceResult mapReducedResults = colData.MapReduce(map, reduce, options);

            //Serialize the results
            IEnumerable<Result> results = mapReducedResults.GetResultsAs<Result>();

            //Change it to a list
            List<Result> lstResults = results.ToList<Result>();
            
            //Get the result which had a match
            Result result = lstResults.Find(resultToFind => { return (resultToFind._id); });

            //Set the columns   
            dtData.Columns.Add("ID", typeof(String));
            dtData.Columns.Add("HostName", typeof(String));            
            dtData.Columns.Add("SentAs", typeof(String));
            dtData.Columns.Add("SentAt", typeof(String));
            
            try
            {
                //Set the number of matches from the result
                intMatches = result.value.Count;

                //Create the data for the viewer
                foreach (Match match in result.value.Matches)
                {
                    //If there is a result
                    if (result._id)
                    {
                        DataRow drRow = dtData.NewRow();

                        drRow[0] = match.FoundOn.Id;
                        drRow[1] = match.FoundOn.HostName;
                        drRow[2] = match.Key + ":" + match.Parameter;
                        drRow[3] = match.FoundOn.RequestedAt;

                        //Add the row
                        dtData.Rows.Add(drRow);
                    }
                }                 
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }
            


            return dtData;

        }


    }
}
