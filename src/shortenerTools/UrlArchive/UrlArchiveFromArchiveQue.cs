using System;
using System.Text.Json;
using System.Threading.Tasks;
using Cloud5mins.domain;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace shortenerTools.UrlArchive
{
    public static class UrlArchiveFromArchiveQue
    {
        [FunctionName("UrlArchiveFromArchiveQue")]
        public static async Task Run([ServiceBusTrigger("ArchiveQue", Connection = "ServiceBusConnection")]string myQueueItem, ILogger log, ExecutionContext context)
        {
            log.LogInformation($"C# ServiceBus queue trigger function archived ShortUrlEntity: {myQueueItem}");

            ShortUrlEntity shortUrlEntity = JsonSerializer.Deserialize<ShortUrlEntity>(myQueueItem);

            var config = new ConfigurationBuilder()
                    .SetBasePath(context.FunctionAppDirectory)
                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();

            StorageTableHelper stgHelper = new StorageTableHelper(config["UlsDataStorage"]);

            var result = await stgHelper.ArchiveShortUrlEntity(shortUrlEntity);
        }
    }
}
