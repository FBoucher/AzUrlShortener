using Cloud5mins.domain;
using Microsoft.Azure.Cosmos.Table;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace shortenerTools.Abstractions
{
    public interface IStorageTableHelper
    {
        CloudStorageAccount CreateStorageAccountFromConnectionString();
        Task<ShortUrlEntity> GetShortUrlEntity(ShortUrlEntity row);
        Task<List<ShortUrlEntity>> GetAllShortUrlEntities();
        Task<List<ClickStatsEntity>> GetAllStatsByVanity(string vanity);
        Task<bool> IfShortUrlEntityExist(ShortUrlEntity row);
        Task<ShortUrlEntity> UpdateShortUrlEntity(ShortUrlEntity urlEntity);
        Task<ShortUrlEntity> ArchiveShortUrlEntity(ShortUrlEntity urlEntity);
        Task<ShortUrlEntity> SaveShortUrlEntity(ShortUrlEntity newShortUrl);
        void SaveClickStatsEntity(ClickStatsEntity newStats);
        Task<int> GetNextTableId();
    }
}