using Cloud5mins.domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using shortenerTools.Abstractions;
using System;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Cloud5mins.Function
{
    public class UrlClickStats : FunctionBase
    {
        private readonly IStorageTableHelper _storageTableHelper;

        public UrlClickStats(IStorageTableHelper storageTableHelper)
        {
            _storageTableHelper = storageTableHelper;
        }

        [FunctionName("UrlClickStats")]
        public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
        ILogger log,
        ExecutionContext context,
        ClaimsPrincipal principal)
        {
            log.LogInformation($"C# HTTP trigger function processed this request: {req}");

            var (requestValid, invalidResult, clickStatsRequest) = await ValidateRequestAsync<UrlClickStatsRequest>(context, req, principal, log);

            // Validation of the inputs
            if (!requestValid)
            {
                return invalidResult;
            }

            try
            {
                var result = new ClickStatsEntityList
                {
                    ClickStatsList = await _storageTableHelper.GetAllStatsByVanity(clickStatsRequest.Vanity)
                };

                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "{functionName} failed due to an unexpected error: {errorMessage}.",
                    context.FunctionName, ex.GetBaseException().Message);

                return new BadRequestObjectResult(new
                {
                    message = ex.Message,
                    StatusCode = HttpStatusCode.BadRequest
                });
            }
        }
    }
}
