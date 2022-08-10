using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Twitter_Tweet_Tracker_Web.Models
{
    public class AnalyzedTweetResult
    {
        public float anger { get; set; }
        public float anticipation { get; set; }
        public float disgust { get; set; }
        public float fear { get; set; }
        public float joy { get; set; }
        public float sadness { get; set; }
        public float surprise { get; set; }
        public float trust { get; set; }
    }
}