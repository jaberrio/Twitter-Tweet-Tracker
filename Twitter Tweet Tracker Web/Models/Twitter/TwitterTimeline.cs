using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Twitter_Tweet_Tracker_Web.Models.Twitter
{


    public class TwitterTimeline
    {
        public Tweet[] data { get; set; }
        public Includes includes { get; set; }
        public Meta meta { get; set; }
    }

    public class Includes
    {
        public Tweet[] tweets { get; set; }
    }

    public class Tweet
    {
        public string conversation_id { get; set; }
        public string id { get; set; }
        public string text { get; set; }

        public DateTime created_at { get; set; }
        public Referenced_Tweets[] referenced_tweets { get; set; }
    }

    public class Referenced_Tweets
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ReferenceType type { get; set; }
        public string id { get; set; }
    }

    public enum ReferenceType { replied_to, quoted, retweeted }

    public class Meta
    {
        public int result_count { get; set; }
        public string newest_id { get; set; }
        public string oldest_id { get; set; }
    }

}