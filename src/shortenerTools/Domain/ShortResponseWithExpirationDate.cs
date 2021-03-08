using Cloud5mins.domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace shortenerTools.Domain
{
    class ShortResponseWithExpirationDate : ShortResponse
    {
        public DateTime ExpirationDate { get; set; }

        ShortResponseWithExpirationDate()
        {

        }

        public ShortResponseWithExpirationDate(string host, string longUrl, string endUrl, string title, DateTime expire)
        {
            LongUrl = longUrl;
            ShortUrl = string.Concat(host, "/", endUrl);
            Title = title;
            ExpirationDate = expire;
        }
    }
}
