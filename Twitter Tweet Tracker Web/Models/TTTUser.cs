using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Twitter_Tweet_Tracker_Web.Models
{
    public class TTTUser
    {
        public string oAuthToken { get; set; }
        public string oAuthSecret { get; set; }
        public string user_id { get; set; }
        public string username { get; set; }
        public string name { get; set; }
        public string profileImage { get; set; }
    }
}