using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;

namespace Cloud5mins.domain
{

    public class StorageTableHelper
    {
        private string StorageConnectionString { get; set; }

        public StorageTableHelper(){}

        public StorageTableHelper(string storageConnectionString){
            StorageConnectionString = storageConnectionString;
        }

       public CloudStorageAccount CreateStorageAccountFromConnectionString()
       {
           CloudStorageAccount storageAccount = CloudStorageAccount.Parse(this.StorageConnectionString);
           return storageAccount;
       }

        private  CloudTable GetStatsTable(){
            CloudTable table = GetTable("ClickStats");
            return table;
        }
        private  CloudTable GetUrlsTable(){
            CloudTable table = GetTable("UrlsDetails");
            return table;
        }

        private  CloudTable GetTable(string tableName){
            CloudStorageAccount storageAccount = this.CreateStorageAccountFromConnectionString();
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            CloudTable table = tableClient.GetTableReference(tableName);
            table.CreateIfNotExists();

            return table;
        }

        public async Task<ShortUrlEntity> GetShortUrlEntity(ShortUrlEntity row)
        {
             TableOperation selOperation = TableOperation.Retrieve<ShortUrlEntity>(row.PartitionKey, row.RowKey);
             TableResult result = await GetUrlsTable().ExecuteAsync(selOperation);
             ShortUrlEntity eShortUrl = result.Result as ShortUrlEntity;
             return eShortUrl;
        }

        public async Task<List<ShortUrlEntity>> GetAllShortUrlEntities()
        {
            var tblUrls = GetUrlsTable();
            TableContinuationToken token = null;
            var lstShortUrl = new List<ShortUrlEntity>();
            do
            {
                // Retreiving all entities that are NOT the NextId entity 
                // (it's the only one in the partion "KEY")
                TableQuery<ShortUrlEntity> rangeQuery = new TableQuery<ShortUrlEntity>().Where(
                    filter: TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.NotEqual, "KEY"));

                var queryResult = await tblUrls.ExecuteQuerySegmentedAsync(rangeQuery, token);
                lstShortUrl.AddRange(queryResult.Results as List<ShortUrlEntity>);
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
                // (it's the only one in the partion "KEY")
                TableQuery<ClickStatsEntity> rangeQuery = new TableQuery<ClickStatsEntity>().Where(
                    filter: TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, vanity));

                var queryResult = await tblUrls.ExecuteQuerySegmentedAsync(rangeQuery, token);
                lstShortUrl.AddRange(queryResult.Results as List<ClickStatsEntity>);
                token = queryResult.ContinuationToken;
            } while (token != null);
            return lstShortUrl;
        }

        /// <summary>
        /// Returns the ShortUrlEntity of the <paramref name="vanity"/>
        /// </summary>
        /// <param name="vanity"></param>
        /// <returns>ShortUrlEntity</returns>
        public async Task<ShortUrlEntity> GetShortUrlEntityByVanity(string vanity)
        {
            var tblUrls = GetUrlsTable();
            TableContinuationToken token = null;
            ShortUrlEntity shortUrlEntity = null;
            do
            {
                TableQuery<ShortUrlEntity> query = new TableQuery<ShortUrlEntity>().Where(
                    filter: TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, vanity));
                var queryResult = await tblUrls.ExecuteQuerySegmentedAsync(query, token);
                shortUrlEntity = queryResult.Results.FirstOrDefault();
            } while (token != null);

            return shortUrlEntity;
        }

        public async Task<bool> IfShortUrlEntityExistByVanity(string vanity)
        {
            ShortUrlEntity shortUrlEntity = await GetShortUrlEntityByVanity(vanity);
            return (shortUrlEntity != null);
        }

        public async Task<bool> IfShortUrlEntityExist(ShortUrlEntity row)
        {
             ShortUrlEntity eShortUrl = await GetShortUrlEntity(row);
             return (eShortUrl != null);
        }

         public async Task<ShortUrlEntity> UpdateShortUrlEntity(ShortUrlEntity urlEntity)
         {
            ShortUrlEntity originalUrl = await GetShortUrlEntity(urlEntity);
            originalUrl.Url = urlEntity.Url;
            originalUrl.Title = urlEntity.Title;            

            return await SaveShortUrlEntity(originalUrl);
         }

        public async Task<ShortUrlEntity> ArchiveShortUrlEntity(ShortUrlEntity urlEntity)
        {
            ShortUrlEntity originalUrl = await GetShortUrlEntity(urlEntity);
            originalUrl.IsArchived = true;

            return await SaveShortUrlEntity(originalUrl);
        }


        public async Task<ShortUrlEntity> SaveShortUrlEntity(ShortUrlEntity newShortUrl)
        {
             TableOperation insOperation = TableOperation.InsertOrMerge(newShortUrl);
             TableResult result = await GetUrlsTable().ExecuteAsync(insOperation);
             ShortUrlEntity eShortUrl = result.Result as ShortUrlEntity;
             return eShortUrl;
        }  

        public async void SaveClickStatsEntity(ClickStatsEntity newStats)
        {
             TableOperation insOperation = TableOperation.InsertOrMerge(newStats);
             TableResult result = await GetStatsTable().ExecuteAsync(insOperation);
        }  

        public async Task<int> GetNextTableId()
        {
            //Get current ID
            TableOperation selOperation = TableOperation.Retrieve<NextId>("1", "KEY");
            TableResult result = await GetUrlsTable().ExecuteAsync(selOperation);
            NextId entity = result.Result as NextId;

            if(entity == null){
                entity = new NextId{
                    PartitionKey = "1",
                    RowKey = "KEY",
                    Id = 1024
                };                   
            }
            entity.Id++;

            //Update
            TableOperation updOperation = TableOperation.InsertOrMerge(entity);

            // Execute the operation.
            await GetUrlsTable().ExecuteAsync(updOperation);

            return entity.Id;
        }


    }
}
