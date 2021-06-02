/*
```c#
Input:
   {
        // [Required]
       "PartitionKey": "d",

        // [Required]
       "RowKey": "doc",

   }


*/

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Cloud5mins.domain;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace Cloud5mins.Function
{
    public static class UrlArchive
    {
        [FunctionName("UrlArchive")]
        public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
        ILogger log,
        ExecutionContext context)
        {
            log.LogInformation($"C# HTTP trigger function processed this request: {req}");

            // Validation of the inputs
            if (req == null)
            {
                return new NotFoundResult();
            }

            ShortUrlEntity input = await req.Content.ReadAsAsync<ShortUrlEntity>();
            if (input == null)
            {
                return new NotFoundResult();
            }

            ShortUrlEntity result;
            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            StorageTableHelper stgHelper = new StorageTableHelper(config["UlsDataStorage"]);

            try
            {
                result = await stgHelper.ArchiveShortUrlEntity(input);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An unexpected error was encountered.");
                return new BadRequestObjectResult(ex);
            }

            return new OkObjectResult(result);
        }
    }
}
