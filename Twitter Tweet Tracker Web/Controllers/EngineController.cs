using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Authenticators.OAuth;
using Twitter_Tweet_Tracker_Web.Models;
using Twitter_Tweet_Tracker_Web.Models.Twitter;

namespace Twitter_Tweet_Tracker_Web.Controllers
{
    public class EngineController : Controller
    {
        // GET: Data
        public ActionResult Begin(string userId)
        {
            var twitter_timeline = GetUserTimeline(userId);
            var mention_timeline = GetUserReplies(userId);
            var analyzedTimeline = AnalyzeTimeline(userId, twitter_timeline, mention_timeline);
            ViewBag.AnalyzedTimeline = analyzedTimeline;
            return View();
        }

        private TwitterTimeline GetUserTimeline(string userId)
        {
            var access_token = Request.Cookies.Get("oauth_token");
            var access_token_secret = Request.Cookies.Get("oauth_token_secret");
            var client = new RestClient("https://api.twitter.com/2")
            {
                Authenticator = new OAuth1Authenticator()
                {
                    SignatureMethod = OAuthSignatureMethod.HmacSha1,
                    ConsumerKey = ConfigurationManager.AppSettings["twitterAPIKey"],
                    ConsumerSecret = ConfigurationManager.AppSettings["twitterAPISecret"],
                    TokenSecret = access_token_secret?.Value,
                    Token = access_token?.Value,
                    Type = OAuthType.ProtectedResource
                },
            };

            try
            {
                var request = new RestRequest($"users/{userId}/tweets");
                request.AddParameter("expansions", "referenced_tweets.id");
                request.AddParameter("tweet.fields", "referenced_tweets,conversation_id");
                var response = client.Execute(request);
                var data = response.Content;
                if (!string.IsNullOrWhiteSpace(data))
                {
                    var timeline = JsonConvert.DeserializeObject<TwitterTimeline>(response.Content);
                    return timeline;
                }
            }
            catch (Exception e)
            {
                ViewBag.Errors = $"user response error {e.Message}";
            }

            return null;
        }

        private TwitterTimeline GetUserReplies(string userId)
        {
            var access_token = Request.Cookies.Get("oauth_token");
            var access_token_secret = Request.Cookies.Get("oauth_token_secret");
            var client = new RestClient("https://api.twitter.com/2")
            {
                Authenticator = new OAuth1Authenticator()
                {
                    SignatureMethod = OAuthSignatureMethod.HmacSha1,
                    ConsumerKey = ConfigurationManager.AppSettings["twitterAPIKey"],
                    ConsumerSecret = ConfigurationManager.AppSettings["twitterAPISecret"],
                    TokenSecret = access_token_secret?.Value,
                    Token = access_token?.Value,
                    Type = OAuthType.ProtectedResource
                },
            };

            try
            {
                var request = new RestRequest($"users/{userId}/mentions");
                request.AddParameter("tweet.fields", "conversation_id");
                var response = client.Execute(request);
                var data = response.Content;
                if (!string.IsNullOrWhiteSpace(data))
                {
                    var timeline = JsonConvert.DeserializeObject<TwitterTimeline>(response.Content);
                    return timeline;
                }
            }
            catch (Exception e)
            {
                ViewBag.Errors = $"user response error {e.Message}";
            }

            return null;
        }

        private AnalyzedTimeline AnalyzeTimeline(string userId, TwitterTimeline timeline,
            TwitterTimeline mentionTimeline)
        {
            var access_token = Request.Cookies.Get("oauth_token");
            var access_token_secret = Request.Cookies.Get("oauth_token_secret");

            var User = AccountController.GetTwitterUser(userId, access_token.Value, access_token_secret.Value);
            var AnalyzedTimeline = new AnalyzedTimeline();
            AnalyzedTimeline.User = User;

            foreach (var item in timeline.data)
            {
                // AnalyzedTweet tweet = new AnalyzedTweet() { id = item.id };
                if (item?.referenced_tweets != null)
                    foreach (var referenced in item.referenced_tweets)
                    {
                        if (referenced.type == ReferenceType.replied_to)
                        {
                            var _reply = new TweetReply() { id = item.id, text = item.text };
                            var _parent = timeline.includes.tweets.First(x => x.id == referenced.id);
                            var _analyzedParent = new AnalyzedTweet() { id = _parent.id, text = _parent.text };
                            _reply.parent_tweet = _analyzedParent;
                            AnalyzedTimeline.Replies.Add(_reply);
                        }
                        else if (referenced.type == ReferenceType.retweeted)
                        {
                            var _retweet = new TweetReply() { id = item.id, text = item.text };
                            var _parent = timeline.includes.tweets.First(x => x.id == referenced.id);
                            var _analyzedParent = new AnalyzedTweet() { id = _parent.id };
                            _retweet.parent_tweet = _analyzedParent;
                            AnalyzedTimeline.Retweets.Add(_retweet);
                        }

                        else if (referenced.type == ReferenceType.quoted)
                        {
                            var _retweet = new TweetReply() { id = item.id, text = item.text };
                            var _parent = timeline.includes.tweets.First(x => x.id == referenced.id);
                            var _analyzedParent = new AnalyzedTweet() { id = _parent.id, text = _parent.text };
                            _retweet.parent_tweet = _analyzedParent;
                            AnalyzedTimeline.Retweets.Add(_retweet);
                        }
                    }
                else
                {
                    var _analyzedTweet = new AnalyzedTweet() { id = item.id, text = item.text };
                    //get the replies for the tweet
                    foreach (var t in mentionTimeline?.data)
                    {
                        if (t.conversation_id == item.conversation_id)
                        {
                            var _treply = new TweetReply() { id = t.id, text = t.text };
                            _analyzedTweet.replies.Add(_treply);
                        }
                    }

                    AnalyzedTimeline.Tweets.Add(_analyzedTweet);
                }
            }

            return AnalyzedTimeline;
        }
    }
}