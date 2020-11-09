using Microsoft.Azure.Cosmos.Table;
using System;

namespace Cloud5mins.domain
{
    public class ClickStatsEntity : TableEntity
    {
        //public string Id { get; set; }
        public string Datetime { get; set; }

        public ClickStatsEntity() { }

        public ClickStatsEntity(string vanity)
        {
            PartitionKey = vanity;
            RowKey = Guid.NewGuid().ToString();
            Datetime = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        }
    }
}
