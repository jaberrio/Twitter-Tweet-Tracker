using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Twitter_Tweet_Tracker_Web.Models.Twitter
{
    public class AnalyzedTweet
    {
        public string id { get; set; }
        public string text { get; set; }

        public DateTime created_at { get; set; }

        /// <summary>
        /// the replies for this tweet
        /// </summary>
        public List<TweetReply> replies { get; set; } = new List<TweetReply>();

    }

    public class TweetReply
    {
        public string id { get; set; }
        public string text { get; set; }

        public DateTime created_at { get; set; }
        /// <summary>
        /// the parent tweet this reply belongs to
        /// </summary>
        public AnalyzedTweet parent_tweet { get; set; }
    }

}

