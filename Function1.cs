using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Azure.Data.Tables;


namespace FunctionApp1
{

    public static class Function1
    {
        [FunctionName("meshapp")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
            HttpRequest req,
            ExecutionContext context,
            ILogger log)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            string offlineConnectionString = config["OfflineConnectionString"];
            string onlineConnectionString = config["OnlineConectionString"];

            // offline database connection
            using (SqlConnection offlineConnection = new SqlConnection(offlineConnectionString))
            {
                offlineConnection.Open();

                string offlineQuery = "SELECT E.EntityName, F.FeatureName, F.FeatureID, F.Value FROM EntityTbl E INNER JOIN Features F ON E.EntityName = F.EntityName WHERE F.ApprovalStatus = 0";



                SqlCommand offlineCommand = new SqlCommand(offlineQuery, offlineConnection);

                var offlineReader = offlineCommand.ExecuteReader();
                while (offlineReader.Read())
                {
                    string entityname = offlineReader.GetString(0);
                    string featureName = offlineReader.GetString(1);
                    int featureId = offlineReader.GetInt32(2);
                    string value = offlineReader.GetString(3);

                    MyTableEntity entity = new MyTableEntity
                    {
                        PartitionKey = entityname,
                        RowKey= featureId.ToString(),
                        FeatureName = featureName,
                       // EntityName = entityname,
                        Value = value
                    };

                    string tableName = "OnlineStorageTable";
                    TableClient tableClient = new TableClient(onlineConnectionString, tableName);

                    await tableClient.CreateIfNotExistsAsync();

                    await tableClient.UpsertEntityAsync(entity);
                }

                offlineReader.Close();
                offlineConnection.Close();
            }

            return new OkObjectResult("Data transfer complete");
        }
    }
}
