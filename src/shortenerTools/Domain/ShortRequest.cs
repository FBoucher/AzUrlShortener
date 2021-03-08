using System;

namespace Cloud5mins.domain
{
    public class ShortRequest
    {
        public string Vanity { get; set; }

        public string Url { get; set; }

        public string Title { get; set; }

        public DateTime Expire { get; set; }
    }
}