using System.Collections.Generic;
using Antlr.Runtime.Misc;
using Twitter_Tweet_Tracker_Web.Models.Twitter;

namespace Twitter_Tweet_Tracker_Web.Models
{
    public class AnalyzedTimeline
    {
        public TwitterUser User { get; set; }

        /// <summary>
        /// my tweets along with their replies
        /// </summary>
        public List<AnalyzedTweet> Tweets { get; set; } = new List<AnalyzedTweet>();

        /// <summary>
        /// the tweets that i have retweeted
        /// </summary>
        public List<TweetReply> Retweets { get; set; } = new List<TweetReply>();

        /// <summary>
        /// the replies I have made along with their parent tweet
        /// </summary>
        public List<TweetReply> Replies { get; set; } = new ListStack<TweetReply>();
    }
}