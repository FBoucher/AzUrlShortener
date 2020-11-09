using Microsoft.Azure.Cosmos.Table;
using shortenerTools.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cloud5mins.domain
{
    public class StorageTableHelper : IStorageTableHelper
    {
        private string StorageConnectionString { get; set; }

        public StorageTableHelper() { }

        public StorageTableHelper(string storageConnectionString)
        {
            StorageConnectionString = storageConnectionString;
        }

        public CloudStorageAccount CreateStorageAccountFromConnectionString()
        {
            var storageAccount = CloudStorageAccount.Parse(this.StorageConnectionString);
            return storageAccount;
        }

        private CloudTable GetStatsTable()
        {
            var table = GetTable("ClickStats");
            return table;
        }
        private CloudTable GetUrlsTable()
        {
            var table = GetTable("UrlsDetails");
            return table;
        }

        private CloudTable GetTable(string tableName)
        {
            var storageAccount = this.CreateStorageAccountFromConnectionString();
            var tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            var table = tableClient.GetTableReference(tableName);
            table.CreateIfNotExists();

            return table;
        }

        public async Task<ShortUrlEntity> GetShortUrlEntity(ShortUrlEntity row)
        {
            var selOperation = TableOperation.Retrieve<ShortUrlEntity>(row.PartitionKey, row.RowKey);
            var result = await GetUrlsTable().ExecuteAsync(selOperation);
            var eShortUrl = result.Result as ShortUrlEntity;
            return eShortUrl;
        }

        public async Task<List<ShortUrlEntity>> GetAllShortUrlEntities()
        {
            var tblUrls = GetUrlsTable();
            TableContinuationToken token = null;
            var lstShortUrl = new List<ShortUrlEntity>();
            do
            {
                // Retrieving all entities that are NOT the NextId entity 
                // (it's the only one in the partition "KEY")
                var rangeQuery = new TableQuery<ShortUrlEntity>().Where(
                    filter: TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.NotEqual, "KEY"));

                var queryResult = await tblUrls.ExecuteQuerySegmentedAsync(rangeQuery, token);
                lstShortUrl.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            } while (token != null);
            return lstShortUrl;
        }

        public async Task<List<ClickStatsEntity>> GetAllStatsByVanity(string vanity)
        {
            var tblUrls = GetStatsTable();
            TableContinuationToken token = null;
            var lstShortUrl = new List<ClickStatsEntity>();
            do
            {
                // Retrieving all entities that are NOT the NextId entity 
                // (it's the only one in the partition "KEY")
                var rangeQuery = new TableQuery<ClickStatsEntity>().Where(
                    filter: TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, vanity));

                var queryResult = await tblUrls.ExecuteQuerySegmentedAsync(rangeQuery, token);
                lstShortUrl.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            } while (token != null);
            return lstShortUrl;
        }


        public async Task<bool> IfShortUrlEntityExist(ShortUrlEntity row)
        {
            var eShortUrl = await GetShortUrlEntity(row);
            return (eShortUrl != null);
        }

        public async Task<ShortUrlEntity> UpdateShortUrlEntity(ShortUrlEntity urlEntity)
        {
            var originalUrl = await GetShortUrlEntity(urlEntity);
            originalUrl.Url = urlEntity.Url;
            originalUrl.Title = urlEntity.Title;

            return await SaveShortUrlEntity(originalUrl);
        }

        public async Task<ShortUrlEntity> ArchiveShortUrlEntity(ShortUrlEntity urlEntity)
        {
            var originalUrl = await GetShortUrlEntity(urlEntity);
            originalUrl.IsArchived = true;

            return await SaveShortUrlEntity(originalUrl);
        }


        public async Task<ShortUrlEntity> SaveShortUrlEntity(ShortUrlEntity newShortUrl)
        {
            var insOperation = TableOperation.InsertOrMerge(newShortUrl);
            var result = await GetUrlsTable().ExecuteAsync(insOperation);
            var eShortUrl = result.Result as ShortUrlEntity;
            return eShortUrl;
        }

        public async void SaveClickStatsEntity(ClickStatsEntity newStats)
        {
            var insOperation = TableOperation.InsertOrMerge(newStats);
            await GetStatsTable().ExecuteAsync(insOperation);
        }

        public async Task<int> GetNextTableId()
        {
            //Get current ID
            var selOperation = TableOperation.Retrieve<NextId>("1", "KEY");
            var result = await GetUrlsTable().ExecuteAsync(selOperation);

            if (!(result.Result is NextId entity))
            {
                entity = new NextId
                {
                    PartitionKey = "1",
                    RowKey = "KEY",
                    Id = 1024
                };
            }
            entity.Id++;

            //Update
            var updOperation = TableOperation.InsertOrMerge(entity);

            // Execute the operation.
            await GetUrlsTable().ExecuteAsync(updOperation);

            return entity.Id;
        }
    }
}
