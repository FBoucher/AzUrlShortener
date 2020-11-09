using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace adminBlazorWebsite.Data
{
    public class ShortUrlEntity
    {
        public string PartitionKey { get; set; }

        public string RowKey { get; set; }

        public string Title { get; set; }

        [Required]
        [Url]
        public string Url { get; set; }

        public string ShortUrl { get; set; }

        public Dictionary<string, int> Clicks { get; set; } = new Dictionary<string, int>();

        public ShortUrlEntity() { }

        public static ShortUrlEntity GetEntity(string longUrl, string endUrl)
        {
            return new ShortUrlEntity
            {
                PartitionKey = endUrl.First().ToString(),
                RowKey = endUrl,
                Url = longUrl
            };
        }

        public string GetDisplayableUrl()
        {

            var length = Url.Length;
            return length >= 50 
                ? string.Concat(Url.Substring(0, 49), "...") 
                : Url;
        }
    }
}
