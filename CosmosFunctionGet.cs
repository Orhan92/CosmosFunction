using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents.Client;
using System.ComponentModel;
using Microsoft.Azure.Documents.Linq;

namespace CosmosFunction
{
    public static class CosmosFunctionGet
    {
        [FunctionName("CosmosFunctionGet")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            [CosmosDB(
            databaseName: "Music-database", 
            collectionName: "songs",
            ConnectionStringSetting = "CosmosDbConnectionString")] DocumentClient client,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            try 
            {
                var searchterm = req.Query["searchterm"];
                if (string.IsNullOrWhiteSpace(searchterm))
                {
                    return (ActionResult)new NotFoundResult();
                }

                Uri collectionUri = UriFactory.CreateDocumentCollectionUri("Music-database", "songs");

                log.LogInformation($"Searching for: {searchterm}");

                IDocumentQuery<SongModel> query = client.CreateDocumentQuery<SongModel>(collectionUri, new FeedOptions { EnableCrossPartitionQuery = true})
                    .Where(p => p.Artist.Contains(searchterm))
                    .AsDocumentQuery();
                
                //list to store objects inside a temporary list so that we can print the object in OkObjectResult below.
                var list = new List<SongModel>();
                while (query.HasMoreResults)
                {
                    foreach (SongModel result in await query.ExecuteNextAsync())
                    {
                        list.Add(result);
                        log.LogInformation($"{result.Artist} - {result.Title}");
                    }
                }
                return new OkObjectResult(list);
            }
            catch (Exception ex)
            {
                log.LogError($"We could not GET your requested data from the database. Exception thrown: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status400BadRequest);
            }
        }
    }
}
