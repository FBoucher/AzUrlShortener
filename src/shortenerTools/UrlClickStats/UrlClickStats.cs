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
    public static class UrlClickStats
    {
        [FunctionName("UrlClickStats")]
        public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequestMessage req, 
        ILogger log, 
        ExecutionContext context)
        {
            log.LogInformation($"C# HTTP trigger function processed this request: {req}");

            // Validation of the inputs
            if (req == null)
            {
                return new NotFoundResult();
            }

            UrlClickStatsRequest input = await req.Content.ReadAsAsync<UrlClickStatsRequest>();
            if (input == null)
            {
                return new NotFoundResult();
            }

            var result = new ClickStatsEntityList();
            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            StorageTableHelper stgHelper = new StorageTableHelper(config["UlsDataStorage"]); 

            try
            {
               result.ClickStatsList = await stgHelper.GetAllStatsByVanity(input.Vanity);
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
