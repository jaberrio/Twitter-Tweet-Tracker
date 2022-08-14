using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Authenticators.OAuth;
using Twitter_Tweet_Tracker_Web.Models;
using Twitter_Tweet_Tracker_Web.Models.Database._twitter_tweet_tracker_dbTableAdapters;
using Twitter_Tweet_Tracker_Web.Models.Twitter;
using ReferenceType = Twitter_Tweet_Tracker_Web.Models.Twitter.ReferenceType;

namespace Twitter_Tweet_Tracker_Web.Controllers
{
    public class EngineController : Controller
    {
        private AnalyzedTweetResult adjust(AnalyzedTweetResult results)
        {
            results.anger += (float)(-0.0534);
            results.anticipation += (float)(0.0263);
            results.disgust += (float)(-0.0368);

            results.fear += (float)(-0.0612);
            results.joy += (float)(-0.0098);
            results.sadness += (float)(-0.0701);

            results.surprise += (float)(-0.094);
            results.trust += (float)(-0.0399);

            return results;
        }

        [HttpPost]
        public string GetOverallResult(string userId)
        {
            AnalyzedTweetResult finalScores = new AnalyzedTweetResult
            {
                anger = 0,
                anticipation = 0,
                disgust = 0,
                fear = 0,
                joy = 0,
                sadness = 0,
                surprise = 0,
                trust = 0
            };
            AnalyzedTweetResultOverTime resultOverTime = new AnalyzedTweetResultOverTime()
            {
                anger = new List<float>(),
                anticipation = new List<float>(),
                disgust = new List<float>(),
                fear = new List<float>(),
                joy = new List<float>(),
                sadness = new List<float>(),
                surprise = new List<float>(),
                trust = new List<float>(),
            };
            List<DateTime> times = new List<DateTime>();

            var twitter_timeline = GetUserTimeline(userId);
            var mention_timeline = GetUserReplies(userId);
            var analyzedTimeline = AnalyzeTimeline(userId, twitter_timeline, mention_timeline);

            int countIndex = 1;
            foreach (var tweet in analyzedTimeline.Tweets)
            {
                var result = AnalyzeTweetScore(tweet.text);
                if (result == null)
                    continue;
                AppendScoresToResults(result, ref finalScores, countIndex);
                AddResultToTimeResults(result, ref resultOverTime);
                times.Add(tweet.created_at);
                countIndex++;
                foreach (var reply in tweet.replies)
                {
                    // if (reply == null || reply.text == null)
                    //     continue;
                    result = AnalyzeTweetScore(reply.text);
                    if (result == null)
                        continue;
                    AppendScoresToResults(result, ref finalScores, countIndex);
                    AddResultToTimeResults(result, ref resultOverTime);
                    times.Add(tweet.created_at);

                    countIndex++;
                }
            }

            foreach (var reply in analyzedTimeline.Replies)
            {
                var result = AnalyzeTweetScore(reply.text);
                if (result == null)
                    continue;
                AppendScoresToResults(result, ref finalScores, countIndex);
                AddResultToTimeResults(result, ref resultOverTime);
                times.Add(reply.created_at);


                countIndex++;
                if (reply.parent_tweet == null)
                    continue;
                result = AnalyzeTweetScore(reply.parent_tweet.text);
                if (result == null)
                    continue;
                AppendScoresToResults(result, ref finalScores, countIndex);
                AddResultToTimeResults(result, ref resultOverTime);
                times.Add(reply.created_at);


                countIndex++;
            }

            foreach (var retweet in analyzedTimeline.Retweets)
            {
                if (retweet.text != null)
                {
                    var _result = AnalyzeTweetScore(retweet.text);
                    if (_result == null)
                        continue;
                    AppendScoresToResults(_result, ref finalScores, countIndex);
                    AddResultToTimeResults(_result, ref resultOverTime);
                    times.Add(retweet.created_at);

                    countIndex++;
                }
                
                if (retweet.parent_tweet == null)
                    continue;
                var result = AnalyzeTweetScore(retweet.parent_tweet.text);
                if (result == null)
                    continue;
                AppendScoresToResults(result, ref finalScores, countIndex);
                AddResultToTimeResults(result, ref resultOverTime);
                times.Add(retweet.created_at);


                countIndex++;
            }
            var overallResults = adjust(finalScores);

            var results = new
            {
                overallResults = overallResults,
                resultsOverTime = resultOverTime,
                times = times,
                timeline = analyzedTimeline
            };
            return JsonConvert.SerializeObject(results);
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
                request.AddParameter("tweet.fields", "referenced_tweets,conversation_id,created_at");
                request.AddParameter("max_results", ConfigurationManager.AppSettings["maxTweetToUse"]);
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

        /// <summary>
        /// reads the unorganized timeline and classifies tweets, retweets, quotes and replies of the user
        /// </summary>
        private AnalyzedTimeline AnalyzeTimeline(string userId, TwitterTimeline timeline,
            TwitterTimeline mentionTimeline)
        {
            var access_token = Request.Cookies.Get("oauth_token");
            var access_token_secret = Request.Cookies.Get("oauth_token_secret");

            var User = AccountController.GetTwitterUser(userId, access_token.Value, access_token_secret.Value);
            var AnalyzedTimeline = new AnalyzedTimeline();
            AnalyzedTimeline.User = User;

            foreach (var item in timeline?.data)
            {
                // AnalyzedTweet tweet = new AnalyzedTweet() { id = item.id };
                if (item?.referenced_tweets != null)
                    foreach (var referenced in item.referenced_tweets)
                    {
                        if (referenced.type == ReferenceType.replied_to)
                        {
                            var _reply = new TweetReply() { id = item.id, text = item.text, created_at = item.created_at};
                            var _parent = timeline?.includes.tweets.FirstOrDefault(x => x.id == referenced?.id);
                            if (_parent != null)
                            {
                                var _analyzedParent = new AnalyzedTweet() { id = _parent.id, text = _parent.text, created_at = _parent.created_at };
                                _reply.parent_tweet = _analyzedParent;
                            }
                            AnalyzedTimeline.Replies.Add(_reply);
                        }
                        else if (referenced.type == ReferenceType.retweeted)
                        {
                            var _retweet = new TweetReply() { id = item.id, text = item.text, created_at = item.created_at};
                            var _parent = timeline.includes.tweets.First(x => x.id == referenced.id);
                            var _analyzedParent = new AnalyzedTweet() { id = _parent.id, text = _parent.text , created_at = _parent.created_at};
                            _retweet.parent_tweet = _analyzedParent;
                            AnalyzedTimeline.Retweets.Add(_retweet);
                        }

                        else if (referenced.type == ReferenceType.quoted)
                        {
                            var _retweet = new TweetReply() { id = item.id, text = item.text, created_at = item.created_at};
                            var _parent = timeline.includes.tweets.First(x => x.id == referenced.id);
                            var _analyzedParent = new AnalyzedTweet() { id = _parent.id, text = _parent.text, created_at = _parent.created_at};
                            _retweet.parent_tweet = _analyzedParent;
                            AnalyzedTimeline.Retweets.Add(_retweet);
                        }
                    }
                else
                {
                    var _analyzedTweet = new AnalyzedTweet() { id = item.id, text = item.text, created_at = item.created_at};
                    //get the replies for the tweet
                    foreach (var t in mentionTimeline?.data)
                    {
                        if (t.conversation_id == item.conversation_id)
                        {
                            var _treply = new TweetReply() { id = t.id, text = t.text, created_at = t.created_at};
                            _analyzedTweet.replies.Add(_treply);
                        }
                    }

                    AnalyzedTimeline.Tweets.Add(_analyzedTweet);
                }
            }

            return AnalyzedTimeline;
        }

        private AnalyzedTweetResult AnalyzeTweetScore(string tweetText)
        {
            var stringSplit = tweetText.Split(' ');
            var tokenizedTweet = "";
            foreach (var word in stringSplit)
            {
                tokenizedTweet += $"{word.ToLower().Replace(".", "")},";
            }

            var wordScores = new word_scoresTableAdapter().GetDataFor(tokenizedTweet);
            if (!wordScores.Any())
                return null;
            var result = new AnalyzedTweetResult
            {
                anger = (float)wordScores.Average(x => x.anger),
                anticipation = (float)wordScores.Average(x => x.anticipation),
                disgust = (float)wordScores.Average(x => x.disgust),
                fear = (float)wordScores.Average(x => x.fear),
                joy = (float)wordScores.Average(x => x.joy),
                sadness = (float)wordScores.Average(x => x.sadness),
                surprise = (float)wordScores.Average(x => x.surprise),
                trust = (float)wordScores.Average(x => x.trust),
            };


            return result;
        }

        private void AppendScoresToResults(AnalyzedTweetResult newResults, ref AnalyzedTweetResult oldResult, float count)
        {
            oldResult.anger = (oldResult.anger + newResults.anger) / count;
            oldResult.disgust = (oldResult.disgust + newResults.disgust) / count;
            oldResult.surprise = (oldResult.surprise + newResults.surprise) / count;
            oldResult.trust = (oldResult.trust + newResults.trust) / count;
            oldResult.anticipation = (oldResult.anticipation + newResults.anticipation) / count;
            oldResult.fear = (oldResult.fear + newResults.fear) / count;
            oldResult.joy = (oldResult.joy + newResults.joy) / count;
        }

        private void AddResultToTimeResults(AnalyzedTweetResult newResult, ref AnalyzedTweetResultOverTime resultOverTime)
        {
            resultOverTime.anger.Add(newResult.anger);
            resultOverTime.disgust.Add(newResult.disgust);
            resultOverTime.surprise.Add(newResult.surprise);
            resultOverTime.trust.Add(newResult.trust);
            resultOverTime.anticipation.Add(newResult.anticipation);
            resultOverTime.fear.Add(newResult.fear);
            resultOverTime.joy.Add(newResult.joy);
        }
    }
}