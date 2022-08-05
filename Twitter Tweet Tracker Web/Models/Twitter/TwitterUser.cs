
namespace Twitter_Tweet_Tracker_Web.Models.Twitter
{


    public class TwitterUser
    {
        public TwitterUserData data { get; set; }
    }

    public class TwitterUserData
    {
        public string profile_image_url { get; set; }
        public string id { get; set; }
        public string username { get; set; }
        public Public_Metrics public_metrics { get; set; }
        public string name { get; set; }
    }

    public class Public_Metrics
    {
        public int followers_count { get; set; }
        public int following_count { get; set; }
        public int tweet_count { get; set; }
        public int listed_count { get; set; }
    }


}